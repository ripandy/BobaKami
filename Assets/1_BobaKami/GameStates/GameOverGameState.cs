using System;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.Interfaces;

namespace BobaKami.GameStates
{
    public class GameOverGameState : IGameState
    {
        private readonly Player player;
        private readonly IGameOverPresenter gameOverPresenter;
        private readonly HighScoreTable highScoreTable;
        private readonly IHighScoreStore highScoreStore;
        private readonly IFaceTrackingStateProvider faceTrackingState;

        public GameStateEnum Id => GameStateEnum.GameOver;

        public GameOverGameState(Player player, IGameOverPresenter gameOverPresenter,
            HighScoreTable highScoreTable, IHighScoreStore highScoreStore,
            IFaceTrackingStateProvider faceTrackingState)
        {
            this.player = player;
            this.gameOverPresenter = gameOverPresenter;
            this.highScoreTable = highScoreTable;
            this.highScoreStore = highScoreStore;
            this.faceTrackingState = faceTrackingState;
        }

        public async ValueTask<GameStateEnum> Running(CancellationToken cancellationToken = default)
        {
            var rank = SubmitHighScore();
            var restart = await gameOverPresenter.Show(player.GameStats, rank, cancellationToken);
            return restart ? GameStateEnum.Intro : GameStateEnum.None;
        }

        /// <summary>
        /// Records the finished run, before <see cref="IntroGameState"/> resets the player.
        /// Only a run that actually ended in death counts: PlayGameState also returns GameOver
        /// when it is cancelled externally (app quit / PlayMode exit), and an aborted run should
        /// not take a slot.
        /// </summary>
        /// <returns>The 1-based rank the run placed at within its own table, or 0 if it did not place.</returns>
        private int SubmitHighScore()
        {
            if (player.IsAlive) return 0;
            var rank = highScoreTable.TrySubmit(player.Score, faceTrackingState.IsFaceTracking, DateTime.UtcNow);
            if (rank == 0) return 0;
            highScoreStore.Save(highScoreTable);
            return rank;
        }
    }
}