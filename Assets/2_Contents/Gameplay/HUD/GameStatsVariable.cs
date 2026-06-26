using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;
using Soar.Variables;

namespace BobaKami.Gameplay.HUD
{
    public class GameStatsVariable : Variable<GameStatsDto>, IPlayerStatsPresenter
    {
        public void Show(GameStatsDto statsDto)
        {
            Value = statsDto;
        }
    }
}