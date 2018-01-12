using System;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Core;
using GameSparksRealTime;

namespace GSCSharpExample
{
    public class GameSparksService
    {
        /**
         * Initialize GameSparks
         * Perform the Setup, Authentication, MatchMaking Logic
         */
        public static void Intialize(
            string apiKey,
            string apiSecret,
            string apiCredential,
            string matchShortCode)
        {
            const int skill = 0;
            var name = DateTime.UtcNow.ToShortTimeString();

            GS.Initialise(new GSPlatform(apiKey, apiSecret, apiCredential));
            GS.TraceMessages = false;
            GS.GameSparksAvailable = a =>
            {
                Console.WriteLine(a ? "GameSparks is available" : "GameSparks is not available");
                if (a) Register(name, name, () => { MatchmakingRequest(skill, matchShortCode); });
            };
            MatchNotFoundMessage.Listener += obj =>
            {
                Console.WriteLine("Match Not Found, retrying...");
                MatchmakingRequest(skill, matchShortCode);
            };
        }

        /**
         * Register a GameSparks Player
         */
        private static void Register(string username, string password, Action onReg)
        {
            var req = new RegistrationRequest();
            req.SetUserName(username);
            req.SetPassword(password);
            req.SetDisplayName(username);
            req.Send(r =>
            {
                if (r.HasErrors) Console.WriteLine("RegistrationError: {0}", r.JSONString);
                else
                {
                    Console.WriteLine("Registration Successful");
                    onReg();
                }
            });
        }

        /**
         * Submit a Matchmaking Request
         */
        private static void MatchmakingRequest(int skill, string matchShortcode)
        {
            var req = new MatchmakingRequest();
            req.SetSkill(skill);
            req.SetMatchShortCode(matchShortcode);
            req.Send(r =>
            {
                if (r.HasErrors) Console.WriteLine("MatchmakingError: {0}", r.JSONString);
                else Console.WriteLine("MatchmakingRequest Successful");
            });
        }
    }
}