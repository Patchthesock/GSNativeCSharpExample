using System;

namespace GSCSharpExample
{
    internal static class MainClass
    {
        
        /**
         * GSCSharpExample
         * Stephen Callaghan (stephen.callaghan@gamesparks.com)
         * Edited - 2018/01/13
         * Created - 2018/01/12
         */
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Press ESC to stop");
            var rtService = new GameSparksRtService();
            rtService.Initialize(Config.SendRegularPing);
            
            GameSparksService.Intialize(
                Config.ApiKey,
                Config.ApiSecret,
                Config.ApiCredential,
                Config.MatchShortCode);

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
