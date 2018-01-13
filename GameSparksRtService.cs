using System;
using System.Timers;
using GameSparks.RT;

namespace GSCSharpExample
{
    public class GameSparksRtService
    {
        public GameSparksRtService()
        {
            _rtConnected = false;
            _pingTimer = new Timer();
            _sessionTimer = new Timer();
        }

        /**
         * Starts a Real Time session onMatchFound
         */
        public void Initialize(bool sendRegularPing)
        {
            GameSparks.Api.Messages.MatchFoundMessage.Listener += r =>
            {
                if (r.Port == null) return;
                Console.WriteLine("Creating New Game Session...");
                Console.WriteLine("Host: {0} Port: {1}", r.Host, r.Port);
                Console.WriteLine("Token: {0}", r.AccessToken);
                Console.WriteLine("Starting Session...");

                _session = new GameSparksRTSessionBuilder() // Build Session
                    .SetHost(r.Host)
                    .SetPort((int)r.Port)
                    .SetConnectToken(r.AccessToken)
                    .SetListener(new RealTimeListener(OnPacketReceived))
                    .Build();

                _session.Start(); // Start Session
                _rtConnected = true;

                _sessionTimer.Elapsed += (source, e) => { _session.Update(); };
                _sessionTimer.Interval = 300;
                _sessionTimer.Enabled = true;

                if (!sendRegularPing) return;
                _pingTimer.Elapsed += (s, e) => { SendPing(); };
                _pingTimer.Interval = 5000;
                _pingTimer.Enabled = true;
            };
        }

        /**
         * Shuts down a session 
         */
        public void StopRealTimeSession()
        {
            if (!_rtConnected) return;
            Console.WriteLine("Shutting down Game Session...");
            _session.Stop();
            _pingTimer.Stop();
            _sessionTimer.Stop();
            _pingTimer.Enabled = false;
            _sessionTimer.Enabled = false;
        }

        private void OnPacketReceived(RTPacket p)
        {
            if (p.Data == null) return;
            switch (p.OpCode)
            {
                case (int)OpCode.Ping:
                    {
                        var r = p.Data.GetInt(1);
                        var l = p.Data.GetLong(2);
                        if (r == null || l == null) return;
                        SendPong((int)r, (long)l);
                        break;
                    }
                case (int)OpCode.Pong:
                    {
                        var r = p.Data.GetInt(1);
                        var ping = p.Data.GetLong(2);
                        var pong = p.Data.GetLong(3);
                        if (r == null || ping == null || pong == null) return;
                        var latency = new Latency((long)ping, (long)pong);
                        Console.WriteLine("Pong Packet Received: Latency {0}, Round Trip {1}, Speed {2} kbit/s",
                            latency.Lag, latency.RoundTrip, latency.Speed);
                        break;
                    }
            }
        }

        private void SendPing()
        {
            if (!_rtConnected) return;
            var d = new RTData();
            d.SetInt(1, GetNextRequestId());
            d.SetLong(2, DateTime.UtcNow.Ticks);
            Console.WriteLine("Sending Ping Packet - OpCode: {0}", OpCode.Ping);
            _session.SendRTData((int)OpCode.Ping, GameSparksRT.DeliveryIntent.RELIABLE, d);
        }

        private void SendPong(int requestId, long pingTime)
        {
            if (!_rtConnected) return;
            var d = new RTData();
            d.SetInt(1, requestId);
            d.SetLong(2, pingTime);
            d.SetLong(3, DateTime.UtcNow.Ticks);
            Console.WriteLine("Sending Pong Packet - OpCode: {0}", OpCode.Pong);
            _session.SendRTData((int)OpCode.Pong, GameSparksRT.DeliveryIntent.RELIABLE, d);
        }

        private int GetNextRequestId()
        {
            _requestIdCounter++;
            if (_requestIdCounter >= int.MaxValue - 1) _requestIdCounter = 0;
            return _requestIdCounter;
        }

        private enum OpCode
        {
            Ping = 998,
            Pong = 999
        }

        private bool _rtConnected;
        private IRTSession _session;
        private int _requestIdCounter;
        private readonly Timer _pingTimer;
        private readonly Timer _sessionTimer;

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