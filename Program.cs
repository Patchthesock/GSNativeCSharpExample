using System;

namespace GSCSharpExample
{
    internal static class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Press ESC to stop");
            var rtService = new GameSparksRtService();
            rtService.Initialize();
            
            GameSparksService.Intialize(
                GameSparksConfig.ApiKey,
                GameSparksConfig.ApiSecret,
                GameSparksConfig.ApiCredential,
                GameSparksConfig.MatchShortCode);

            while (true)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Escape) continue;
                rtService.StopRealTimeSession();
                GameSparksService.Shutdown();
                Console.WriteLine("Press Any Key to close");
                Console.ReadKey();
                return;
            }
        }
    }
}
