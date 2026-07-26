using System;

namespace BobaKami.DataTransferObjects
{
    /// <summary>
    /// DTO for game statistics surfaced to presenters. <see cref="Combo"/> is the *current*
    /// chain length (0 right after a hit); <see cref="MaxCombo"/> is the run's peak, shown on
    /// game over. <see cref="Multiplier"/> is the active scoring tier, carried here so the tier
    /// thresholds stay in the domain rather than being recomputed adapter-side.
    /// </summary>
    [Serializable]
    public readonly struct GameStatsDto
    {
        public int Score { get; }
        public int Combo { get; }
        public int MaxCombo { get; }
        public int BeansEaten { get; }
        public int Multiplier { get; }

        public GameStatsDto(int score, int combo, int maxCombo, int beansEaten, int multiplier)
        {
            Score = score;
            Combo = combo;
            MaxCombo = maxCombo;
            BeansEaten = beansEaten;
            Multiplier = multiplier;
        }
    }
}
