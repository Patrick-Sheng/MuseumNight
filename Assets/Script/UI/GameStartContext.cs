public static class GameStartContext
{
    private static bool playStoryIntroOnNextLoad;

    public static void SetStoryIntroForNextLoad(bool shouldPlay)
    {
        playStoryIntroOnNextLoad = shouldPlay;
    }

    public static bool ConsumeStoryIntroRequest()
    {
        if (!playStoryIntroOnNextLoad)
            return false;

        playStoryIntroOnNextLoad = false;
        return true;
    }
}