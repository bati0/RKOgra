using System;
using System.Collections.Generic;

namespace RKOTrainer
{
    public class GameLogic
    {
        public const double Gravity = 0.2;
        public const double VelocityDamping = 0.92;
        public const double RateSmoothing = 0.5;
        public const int MaxCompressionHistory = 3;

        public double CalculateCurrentRate(Queue<long> compressionTimes)
        {
            if (compressionTimes.Count < 2) return 110; // Domyślne tempo

            var times = compressionTimes.ToArray();
            double totalRate = 0;
            int validIntervals = 0;

            for (int i = 1; i < times.Length; i++)
            {
                long interval = times[i] - times[i - 1];
                if (interval > 0)
                {
                    double rate = 60000.0 / interval; // Konwersja interwału na uderzenia na minutę
                    // Ograniczenie zakresu tempa
                    rate = Math.Max(60, Math.Min(160, rate));
                    totalRate += rate;
                    validIntervals++;
                }
            }

            return validIntervals > 0 ? totalRate / validIntervals : 110;
        }

        public int CalculatePoints(double rate)
        {
            // Optymalne tempo to 110 uderzeń/min
            const double optimalRate = 110;
            const double rateTolerance = 15; // +/- 15 uderzeń/min

            double deviation = Math.Abs(rate - optimalRate);

            if (deviation <= 3) // Prawie idealne tempo
                return 100;
            else if (deviation <= rateTolerance) // W akceptowalnym zakresie
            {
                // Liniowa interpolacja punktów od 100 do -100 based on deviation
                return (int)(100 * (1 - deviation / rateTolerance));
            }
            else // Poza zakresem
                return -100;
        }
    }
}