using System.Threading;
using System.Threading.Tasks;
using BobaKami.DataTransferObjects;

namespace BobaKami.Interfaces
{
    public interface IGameOverPresenter
    {
        /// <summary>
        /// Presents with Game Over screen and returns true if user wants to restart the game.
        /// </summary>
        /// <returns>true to replay or false to exit.</returns>
        ValueTask<bool> Show(GameStatsDto statsDto, CancellationToken cancellationToken = default);
    }
}