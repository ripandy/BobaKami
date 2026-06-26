using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    public class DummyPlayerPresenter : IPlayerDirectionPresenter, IPlayerHealthPresenter, IPlayerStatsPresenter
    {
        public void Show(DirectionEnum direction)
        {
        }

        public void Show(float healthPercentage)
        {
        }

        public void Show(GameStatsDto statsDto)
        {
        }
    }
}