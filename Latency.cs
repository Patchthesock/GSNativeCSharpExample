using System;

namespace GSCSharpExample
{
    public class Latency
    {
        public readonly double Lag;
        public readonly double Speed;
        public readonly double RoundTrip;
        
        public Latency(long pingTime, long pongTime)
        {
            if (pingTime == 0 || pongTime == 0)
            {
                Lag = 0;
                Speed = 0;
                RoundTrip = 0;
            }
            else
            {
                var r = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - pingTime).TotalSeconds;
                Lag = Math.Round(TimeSpan.FromTicks(pongTime - pingTime).TotalSeconds, 3);
                RoundTrip = Math.Round(r, 3);
                // Convert 1400 bytes (Packet Limit / Window Size) to bits 1400 * 8 = 14400 bits
                // Maximum network throughput equals the window size divided by the round trip time
                // Divide by 1000 to convert to kbits, round to two decimal places.
                Speed = Math.Round(14400 / r / 1000, 2);
            }
        }
    }
}