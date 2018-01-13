using System;
using System.Timers;
using GameSparks.RT;

namespace GSCSharpExample
{
    public class GameSparksRtService
    {
        public GameSparksRtService()
        {
            _timer = new Timer();
        }

        /**
         * Starts a Real Time session onMatchFound
         */
        public void Initialize()
        {
            GameSparks.Api.Messages.MatchFoundMessage.Listener += r =>
            {
                if (r.Port == null) return;
                Console.WriteLine("Creating New Game Session...");
                Console.WriteLine("Host: {0} Port: {1}", r.Host, r.Port);
                Console.WriteLine("Token: {0}", r.AccessToken);
                Console.WriteLine("Starting Session...");
                _session = new GameSparksRTSessionBuilder()
                    .SetHost(r.Host)
                    .SetPort((int)r.Port)
                    .SetConnectToken(r.AccessToken)
                    .SetListener(new RealTimeListener(OnPacketReceived))
                    .Build();
                _session.Start();

                _timer.Elapsed += (source, e) => { _session.Update(); };
                _timer.Interval = 300;
                _timer.Enabled = true;
            };
        }

        /**
         * Shuts down a session 
         */
        public void StopRealTimeSession()
        {
            if (_session == null) return;
            Console.WriteLine("Shutting down Game Session...");
            _session.Stop();
            _timer.Stop();
            _timer.Enabled = false;
        }

        private void OnPacketReceived(RTPacket p)
        {
            if (p.OpCode != 998) return;
            if (p.Data == null) return;
            var r = p.Data.GetInt(1);
            var l = p.Data.GetLong(2);
            if (r == null || l == null) return;

            var op = 999;
            var d = new RTData();
            d.SetInt(1, (int)r);
            d.SetLong(2, (long)l);
            d.SetLong(3, DateTime.UtcNow.Ticks);
            _session.SendRTData(op, GameSparksRT.DeliveryIntent.RELIABLE, d);
            Console.WriteLine("Packet Sent - OpCode: {0}", op);
        }

        private IRTSession _session;
        private readonly Timer _timer;

        private class RealTimeListener : IRTSessionListener
        {
            public RealTimeListener(Action<RTPacket> onPacketRecevied)
            {
                _onPacketReceivedListener = onPacketRecevied;
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
                _onPacketReceivedListener(packet);
            }

            private readonly Action<RTPacket> _onPacketReceivedListener;
        }
    }
}