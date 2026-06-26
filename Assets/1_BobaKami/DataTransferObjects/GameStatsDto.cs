using System;

namespace BobaKami.DataTransferObjects
{
    /// <summary>
    /// DTO for game statistics.
    /// </summary>
    [Serializable]
    public readonly struct GameStatsDto
    {
        public int Score { get; }
        public int Combo { get; }
        
        public GameStatsDto(int score, int combo)
        {
            Score = score;
            Combo = combo;
        }
    }
}