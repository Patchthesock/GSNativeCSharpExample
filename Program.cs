namespace GSCSharpExample
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            GameSparksRtService.Initialize();
            GameSparksService.Intialize(
                GameSparksConfig.ApiKey,
                GameSparksConfig.ApiSecret,
                GameSparksConfig.ApiCredential,
                GameSparksConfig.MatchShortCode);

            while (true) { } // Should never return
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
