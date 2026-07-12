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

        public ScriptedGameOverPresenter(params bool[] answers)
        {
            restartAnswers = new Queue<bool>(answers);
        }

        public ValueTask<bool> Show(GameStatsDto stats, CancellationToken cancellationToken = default)
        {
            ShownStats.Add(stats);
            return new ValueTask<bool>(restartAnswers.Count > 0 && restartAnswers.Dequeue());
        }
    }
}
