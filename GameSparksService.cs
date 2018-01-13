using System;
using GameSparks.Api.Messages;
using GameSparks.Api.Requests;
using GameSparks.Core;
using GameSparksRealTime;

namespace GSCSharpExample
{
    public static class GameSparksService
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
            var name = DateTime.UtcNow.ToLongTimeString();

            if (string.IsNullOrEmpty(apiKey)) Console.WriteLine("Warning: apiKey Empty");
            if (string.IsNullOrEmpty(apiSecret)) Console.WriteLine("Warning: apiSecret Empty");
            if (string.IsNullOrEmpty(apiCredential)) Console.WriteLine("Warning: apiCredential is Empty");
            if (string.IsNullOrEmpty(matchShortCode)) Console.WriteLine("Warning: matchShortCode is Empty");
            
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
         * Signout and Shutdown GameSparks
         */
        public static void Shutdown()
        {
            EndSessionRequest();
            GS.ShutDown();
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
                    Console.WriteLine("Registration Successful, Username: {0}", r.DisplayName);
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

        private static void EndSessionRequest()
        {
            var req = new EndSessionRequest();
            req.Send(r =>
            {
                if (r.HasErrors) Console.WriteLine("EndSessionError: {0}", r.JSONString);
                else Console.WriteLine("EndSessionRequest Successful");
            });
        }
    }
}