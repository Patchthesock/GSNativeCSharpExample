using System;
using System.Timers;
using GameSparks.RT;

namespace GSCSharpExample
{
    public static class GameSparksRtService
    {
        public static void Initialize()
        {
            GameSparks.Api.Messages.MatchFoundMessage.Listener += r =>
            {
                if (r.Port == null) return;
                StartRtSession(new RtSession(r.Host, (int) r.Port, r.AccessToken));
            };
        }

        public static void Shutdown()
        {
            
        }

        private static void StartRtSession(RtSession s)
        {
            var sess = new GameSession();
            sess.StartSession(s.Port, s.Token, s.Host);

            var t = new Timer();
            t.Elapsed += sess.OnTimedEvent;
            t.Interval = 300;
            t.Enabled = true;
        }

        private class RtSession
        {
            public readonly int Port;
            public readonly string Host;
            public readonly string Token;

            public RtSession(string host, int port, string token)
            {
                Host = host;
                Port = port;
                Token = token;
            }
        }
        
        private class GameSession : IRTSessionListener
        {
            public void StartSession(int port, string token, string host)
            {
                Console.WriteLine("Creating New Game Session...");
                Console.WriteLine("Host: {0} Port: {1}", host, port);
                Console.WriteLine("Token: {0}", token);
                Console.WriteLine("Starting Session...");
                _session = new GameSparksRTSessionBuilder()
                    .SetPort(port)
                    .SetConnectToken(token)
                    .SetHost(host)
                    .SetListener(this)
                    .Build();
                _session.Start();
            }

            public void OnTimedEvent(object source, ElapsedEventArgs e)
            {
                _session.Update();
            }

            public void OnPlayerConnect(int peerId)
            {
                Console.WriteLine("Player {0} Connected", peerId);
            }

            public void OnPlayerDisconnect(int peerId)
            {
                Console.WriteLine("Player {0} Disconnected", peerId);
            }

            public void OnReady(bool ready)
            {
                Console.WriteLine("Server Ready: {0}", ready);
            }
            
            public void OnPacket(RTPacket packet)
            {
                Console.WriteLine("Packet Received - OpCode: {0}, Data: {1}", packet.OpCode, packet.Data);
            }
            
            private IRTSession _session;
        }
    }
}