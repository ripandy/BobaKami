using System.Collections.Generic;
using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    /// <summary>
    /// Recording presenter double: remembers everything it was shown so tests can assert
    /// on domain output without a real UI.
    /// </summary>
    public class DummyPlayerPresenter : IPlayerDirectionPresenter, IPlayerHealthPresenter, IPlayerStatsPresenter
    {
        public List<DirectionEnum> ShownDirections { get; } = new();
        public List<float> ShownHealthPercentages { get; } = new();
        public List<GameStatsDto> ShownStats { get; } = new();

        public DirectionEnum LastDirection => ShownDirections.Count > 0 ? ShownDirections[^1] : DirectionEnum.Forward;
        public float LastHealthPercentage => ShownHealthPercentages.Count > 0 ? ShownHealthPercentages[^1] : 1f;

        public void Show(DirectionEnum direction)
        {
            ShownDirections.Add(direction);
        }

        public void Show(float healthPercentage)
        {
            ShownHealthPercentages.Add(healthPercentage);
        }

        public void Show(GameStatsDto statsDto)
        {
            ShownStats.Add(statsDto);
        }
    }
}
