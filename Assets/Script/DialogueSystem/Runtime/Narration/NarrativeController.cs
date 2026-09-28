using System;
using System.Collections.Generic;
using DialogueSystem.Data;
using DialogueSystem.Runtime.Command;
using DialogueSystem.Runtime.Interaction;
using DialogueSystem.Runtime.UI;
using DialogueSystem.Runtime.Utility;
using DialogueSystem.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DialogueSystem.Runtime.Narration
{
    [RequireComponent(typeof(NarrativeLoader)), RequireComponent(typeof(CommandExecutionHandler))]
    public class NarrativeController : MonoBehaviour
    {
        [SerializeField] private NarrativeUI narrativeUI;
        [SerializeField] private NarrativeLoader narrativeLoader;
        [SerializeField] private CommandExecutionHandler commandExecutionHandler;
        
        [Header("Options")]
        [SerializeField] private bool resetNarrativeOnLoad;
        [SerializeField] private bool lockPlayerControlWhileNarrating = true;

        [SerializeField] private UnityEvent onNarrativeStart;
        [SerializeField] private UnityEvent onNarrativeEnd;
        
        [Space, Header("Default Values"), SerializeField]
        private CharacterData defaultCharacterData;

        private string NarrativePathID { get; set; }
    
        public bool IsChoosing { get; private set; }
        public bool IsNarrating { get; private set; }
        
        public UnityEvent OnNarrativeStart => onNarrativeStart;
        public UnityEvent OnNarrativeEnd => onNarrativeEnd;
        
        private NarrativeNode _currentNarrative;
        private Queue<DialogueMessage> _narrativeQueue;
        private Narrative _narrative;

        private const string PathSeparator = ".";

        private DialogueMonoBehaviour.DialogueEvent[] _events;
        private readonly HashSet<string> _missingCharacterWarnings = new HashSet<string>();
        private PlayerMovement _lockedPlayerMovement;
        private PlayerInteract _lockedPlayerInteract;
        private Animator _lockedPlayerAnimator;
        private bool _movementWasEnabled;
        private bool _interactWasEnabled;

        
        public void BeginNarration(DialogueContainer narrativeToLoad, DialogueMonoBehaviour.DialogueEvent[] dialogueEvents)
        {
            _events = dialogueEvents;
            _missingCharacterWarnings.Clear();
            _narrative = narrativeLoader.LoadNarrative(narrativeToLoad);

            if (_narrative == null)
            {
                LogHandler.Alert("Can't start narrative because the narrative was not loaded properly.");
                return;
            }

            if (resetNarrativeOnLoad)
            {
                narrativeLoader.ResetNarrative();
            }
        
            StartNarrative();
        }

        private void StartNarrative()
        {
            onNarrativeStart?.Invoke();
            IsNarrating = true;
            LockPlayerControl();
        
            narrativeUI.SetUIActive(true);
            narrativeUI.InitializeUI();

            SetupNarrativeEvents();
        
            var startNode = GetStartNode();
            StartNewDialogue(startNode);
        }

        private NarrativeNode GetStartNode()
        {
            NarrativePathID = narrativeLoader.GetSavedNarrativePathID();
            return _narrative.FindStartNodeFromPath(NarrativePathID);
        }

        private void ContinueToChoiceAutomatically()
        {
            var continueAutomatically = _narrativeQueue.Count == 0 && 
                                        (_currentNarrative.HasNextChoice() || _currentNarrative.HasChoiceAfterSimpleNode() 
                                            && !_currentNarrative.IsCheckpoint);

            if (!continueAutomatically)
            {
                return;
            }

            FindNextPath();
        }

        private void StartNewDialogue(NarrativeNode narrative)
        {
            if (narrative == null)
            {
                return;
            }
            _currentNarrative = narrative;
            _narrativeQueue = new Queue<DialogueMessage>(narrative.Dialogue);
            AdvanceNarrative(false);
        }

        public void NextNarrative()
        {
            AdvanceNarrative(true);
        }

        private void AdvanceNarrative(bool playSound)
        {
            IsChoosing = false;
            if (playSound && IsNarrating) GameAudio.Play(GameAudio.Cue.DialogueNext);
            if (narrativeUI.IsMessageDisplaying())
            {
                SkipCurrentMessage();
                LogHandler.Log("Skip", LogHandler.Color.Yellow);
                return;
            }
        
            ContinueNarrative();
        }

        private void ContinueNarrative()
        {
            if (_narrativeQueue == null)
            {
                FinishDialogue();
                return;
            }
        
            if (_narrativeQueue.Count == 0)
            {
                FindNextPath();
                return;
            }

            var currentDialogueMessage = _narrativeQueue.Dequeue();
            ShowNextMessage(currentDialogueMessage);
        }

        private void SkipCurrentMessage()
        {
            narrativeUI.DisplayAllMessage();
            commandExecutionHandler.ExecuteAllCommands();
        }

        private void FindNextPath()
        {
            if (_currentNarrative.IsCheckpoint)
            {
                FinishAtCheckpoint();
                return;
            }
        
            if (_currentNarrative.IsSimpleNode())
            {
                StartNewDialogue(_currentNarrative.DefaultPath);
                return;
            }

            if (_currentNarrative.HasNextChoice())
            {
                SetupDialogueOptions();
                return;
            }

            if (!_currentNarrative.IsTipNarrativeNode())
            {
                return;
            }
            FinishDialogue();
        }

        private void SetupDialogueOptions()
        {
            IsChoosing = true;
            narrativeUI.DisplayOptions(_currentNarrative.Options, _currentNarrative.DisableAlreadyChosenOptions, ChooseNarrativePath);
        }
        
        private CharacterData GetCharacter(string characterName)
        {
            var character = _narrative.FindCharacter(characterName);
            if (character == null && !string.IsNullOrWhiteSpace(characterName))
            {
                var key = characterName.Trim();
                if (_missingCharacterWarnings.Add(key))
                {
                    LogHandler.Warn($"Character '{key}' was not found in this DialogueContainer. Falling back to default character.");
                }
            }

            return character ? character : defaultCharacterData;
        }

        private void ShowNextMessage(DialogueMessage nextDialogueMessage)
        {
            var currentSpeakerData = GetCharacter(nextDialogueMessage.CharacterName);
            
            //Gather message commands and data
            commandExecutionHandler.GatherCommandData(nextDialogueMessage, currentSpeakerData, _events);
            commandExecutionHandler.ExecuteDefaultCommands();
            
            var messageWithoutCommands = commandExecutionHandler.ParseDialogueCommands(nextDialogueMessage.Content);
            
            //Display dialogue ui
            narrativeUI.DisplayDialogueBubble(nextDialogueMessage, currentSpeakerData);
            narrativeUI.DisplayMessage(messageWithoutCommands);
        }

        private void ChooseNarrativePath(int choiceIndex)
        {
            NarrativePathID += choiceIndex.ToString();
        
            UnsetNarrativeEvents();

            _currentNarrative.Options[choiceIndex].HasAlreadyBeenChosen = true;
            var nextNarrative = _currentNarrative.Options[choiceIndex].TargetNarrative;

            if (nextNarrative != null)
            {
                SetupNarrativeEvents();
                StartNewDialogue(nextNarrative);
                return;
            }
        
            FinishDialogue();
        }

        private void SetupNarrativeEvents()
        {
            narrativeUI.OnMessageEnd += ContinueToChoiceAutomatically;
        }

        private void UnsetNarrativeEvents()
        {
            narrativeUI.OnMessageEnd -= ContinueToChoiceAutomatically;
        }

        private void FinishAtCheckpoint()
        {
            NarrativePathID += PathSeparator;
            FinishDialogue();
        }
        
        private void FinishDialogue()
        {
            narrativeUI.SetUIActive(false);
            IsNarrating = false;
            UnlockPlayerControl();

            narrativeLoader.SaveNarrativePath(NarrativePathID, _currentNarrative?.IsTipNarrativeNode() ?? false);
        
            onNarrativeEnd?.Invoke();
            LogResults();
        }

        private void LogResults()
        {
            LogHandler.Log("Dialogue finished!", LogHandler.Color.Blue);
            LogHandler.Log($"Final narrative path ID: {NarrativePathID}", LogHandler.Color.Blue);
        }

        private void LockPlayerControl()
        {
            if (!lockPlayerControlWhileNarrating)
                return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;

            _lockedPlayerMovement = player.GetComponent<PlayerMovement>();
            _lockedPlayerInteract = player.GetComponent<PlayerInteract>();
            _lockedPlayerAnimator = player.GetComponent<Animator>();

            if (_lockedPlayerMovement != null)
            {
                _movementWasEnabled = _lockedPlayerMovement.enabled;
                _lockedPlayerMovement.enabled = false;
                _lockedPlayerMovement.isMoving = false;
            }

            if (_lockedPlayerInteract != null)
            {
                _interactWasEnabled = _lockedPlayerInteract.enabled;
                _lockedPlayerInteract.ClearInteraction();
                _lockedPlayerInteract.enabled = false;
            }

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            if (body != null)
                body.linearVelocity = Vector2.zero;

            if (_lockedPlayerAnimator != null)
                _lockedPlayerAnimator.SetBool("isMoving", false);
        }

        private void UnlockPlayerControl()
        {
            if (!lockPlayerControlWhileNarrating)
                return;

            if (_lockedPlayerMovement != null)
                _lockedPlayerMovement.enabled = _movementWasEnabled;

            if (_lockedPlayerInteract != null)
                _lockedPlayerInteract.enabled = _interactWasEnabled;

            _lockedPlayerMovement = null;
            _lockedPlayerInteract = null;
            _lockedPlayerAnimator = null;
        }
    }
}
