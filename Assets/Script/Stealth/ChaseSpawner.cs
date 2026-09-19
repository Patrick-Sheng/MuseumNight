using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>Spawn one enemy per point from any UnityEvent or script.</summary>
public class ChaseSpawner : MonoBehaviour
{
    [SerializeField] private ChaseEnemy enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("Optional. Finds the active Player-tagged Rigidbody2D when empty.")]
    [SerializeField] private Rigidbody2D playerTarget;
    [Tooltip("Optional integration: capture fails this room and room outcomes stop all registered enemies.")]
    [SerializeField] private StealthRoomController room;
    [SerializeField] private bool spawnOnce = true;
    [Tooltip("For other room types, connect this to their failure/restart logic.")]
    [SerializeField] private UnityEvent onPlayerCaught = new UnityEvent();

    private readonly List<ChaseEnemy> enemies = new List<ChaseEnemy>();
    private bool hasSpawned;
    private bool caught;

    public bool HasSpawned => hasSpawned;

    // Void wrapper appears in UnityEvent's Inspector method picker.
    public void Spawn() => TrySpawn();

    public bool TrySpawnForRoom(StealthRoomController owner)
    {
        if (room != null && room != owner)
        {
            Debug.LogError("ChaseSpawner is assigned to a different room controller.", this);
            return false;
        }
        room = owner;
        return TrySpawn();
    }

    public bool TrySpawn()
    {
        if (!CanChase()) return false;
        if (spawnOnce && hasSpawned) return true;
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.GetComponent<Rigidbody2D>();
        }

        if (playerTarget == null || enemyPrefab == null || !enemyPrefab.IsConfigured ||
            spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("ChaseSpawner needs a valid ChaseEnemy prefab, a player Rigidbody2D, and at least one spawn point.", this);
            return false;
        }
        if (room != null && room.gameObject.scene != gameObject.scene)
        {
            Debug.LogError("ChaseSpawner and its optional room controller must belong to the same scene.", this);
            return false;
        }
        foreach (Transform point in spawnPoints)
        {
            if (point == null)
            {
                Debug.LogError("Every ChaseSpawner spawn point must be assigned. No enemies were spawned.", this);
                return false;
            }
        }

        hasSpawned = true;
        foreach (Transform point in spawnPoints)
        {
            Vector3 position = new Vector3(point.position.x, point.position.y, playerTarget.transform.position.z);
            ChaseEnemy enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            SceneManager.MoveGameObjectToScene(enemy.gameObject, gameObject.scene);
            enemy.Initialize(playerTarget, HandleCaught, CanChase);
            enemies.Add(enemy);
            if (room != null) room.RegisterChaseEnemy(enemy);
        }
        return true;
    }

    private bool CanChase() => this != null && isActiveAndEnabled && !caught &&
        !PauseMenu.IsPaused && (room == null || room.IsPlaying) &&
        (RoomManager.Instance == null || !RoomManager.Instance.IsTransitioning);

    private void HandleCaught()
    {
        if (!CanChase()) return;
        caught = true;
        StopSpawnedEnemies();
        if (room != null) room.CatchPlayer();
        onPlayerCaught.Invoke();
    }

    public void StopSpawnedEnemies()
    {
        foreach (ChaseEnemy enemy in enemies)
            if (enemy != null) enemy.StopChasing();
    }

    private void OnDisable() => StopSpawnedEnemies();
}
