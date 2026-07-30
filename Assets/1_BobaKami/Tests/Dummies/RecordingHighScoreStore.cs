using BobaKami.Interfaces;

namespace BobaKami.Tests
{
    /// <summary>
    /// High-score store double. Records how many times the domain asked for a save, so tests can
    /// assert that a run persisted (or deliberately did not) without touching disk.
    /// </summary>
    internal class RecordingHighScoreStore : IHighScoreStore
    {
        public int SaveCount { get; private set; }

        public HighScoreTable LastSaved { get; private set; }

        public void Save(HighScoreTable table)
        {
            SaveCount++;
            LastSaved = table;
        }
    }
}
