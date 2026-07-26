namespace BobaKami
{
    /// <summary>
    /// Single source of truth for the scoring curve. Each eaten bean scores
    /// <see cref="BeanBaseScore"/> multiplied by the combo tier, so longer chains are worth
    /// chasing. Tiers step at combo 5/10/20 — the tier-2 boundary at 5 matches the combo popup
    /// (<c>GameStatPresenter.ShowCombo</c> shows nothing below 5).
    /// </summary>
    internal static class ScoreRules
    {
        internal const int BeanBaseScore = 100;

        internal static int MultiplierFor(int combo) =>
            combo switch
            {
                < 5 => 1,
                < 10 => 2,
                < 20 => 3,
                < 50 => 4,
                _ => 5
            };

        internal static int GetScore(int combo)
        {
            return BeanBaseScore * MultiplierFor(combo);
        }
    }
}
