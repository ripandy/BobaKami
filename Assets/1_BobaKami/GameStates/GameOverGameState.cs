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

        public GameStateEnum Id => GameStateEnum.GameOver;

        public GameOverGameState(Player player, IGameOverPresenter gameOverPresenter,
            HighScoreTable highScoreTable, IHighScoreStore highScoreStore)
        {
            this.player = player;
            this.gameOverPresenter = gameOverPresenter;
            this.highScoreTable = highScoreTable;
            this.highScoreStore = highScoreStore;
        }

        public async ValueTask<GameStateEnum> Running(CancellationToken cancellationToken = default)
        {
            SubmitHighScore();
            var restart = await gameOverPresenter.Show(player.GameStats, cancellationToken);
            return restart ? GameStateEnum.Intro : GameStateEnum.None;
        }

        /// <summary>
        /// Records the finished run, before <see cref="IntroGameState"/> resets the player.
        /// Only a run that actually ended in death counts: PlayGameState also returns GameOver
        /// when it is cancelled externally (app quit / PlayMode exit), and an aborted run should
        /// not take a slot.
        /// </summary>
        private void SubmitHighScore()
        {
            if (player.IsAlive) return;
            if (highScoreTable.TrySubmit(player.Score, DateTime.UtcNow) == 0) return;
            highScoreStore.Save(highScoreTable);
        }
    }
}