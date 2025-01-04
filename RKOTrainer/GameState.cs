using System.Collections.Generic;

namespace RKOTrainer
{
    public class GameState
    {
        public double CurrentRate { get; set; } = 110;
        public int IndicatorPosition { get; set; } = 0;
        public int DifficultyLevel { get; set; } = 1;
        public double IndicatorVelocity { get; set; } = 0;
        public Queue<long> CompressionTimes { get; private set; } = new Queue<long>();
        public int CompressionCount { get; set; } = 0;
        public int BreathCount { get; set; } = 0;
        public int TotalScore { get; set; } = 0;
        public bool IsCompressionPhase { get; set; } = true;
    }
    
}