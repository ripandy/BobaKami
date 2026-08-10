using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    /// <summary>
    /// Game-over double with explicit scripted restart answers (replaces the old
    /// ComboCount==99 sentinel). Records the stats it was shown for assertions.
    /// </summary>
    internal class ScriptedGameOverPresenter : IGameOverPresenter
    {
        private readonly Queue<bool> restartAnswers;

        public List<GameStatsDto> ShownStats { get; } = new();

        /// <summary>The high-score rank passed to each <see cref="Show"/> call, in order.</summary>
        public List<int> ShownRanks { get; } = new();

        public ScriptedGameOverPresenter(params bool[] answers)
        {
            restartAnswers = new Queue<bool>(answers);
        }

        public ValueTask<bool> Show(GameStatsDto stats, int highScoreRank,
            CancellationToken cancellationToken = default)
        {
            ShownStats.Add(stats);
            ShownRanks.Add(highScoreRank);
            return new ValueTask<bool>(restartAnswers.Count > 0 && restartAnswers.Dequeue());
        }
    }
}
