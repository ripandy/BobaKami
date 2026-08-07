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
        /// <param name="highScoreRank">
        /// The 1-based rank the finished run took in the high-score table: 1 means it is a new
        /// record, 2..<see cref="HighScoreTable.Capacity"/> means it placed, 0 means it did not
        /// place at all. A tie never yields 1 — <see cref="HighScoreTable.TrySubmit"/> puts equal
        /// scores after the incumbent, so the badge only fires for a score that was actually beaten.
        /// </param>
        /// <returns>true to replay or false to exit.</returns>
        ValueTask<bool> Show(GameStatsDto statsDto, int highScoreRank, CancellationToken cancellationToken = default);
    }
}