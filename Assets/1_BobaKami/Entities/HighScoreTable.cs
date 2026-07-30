using System;
using System.Collections.Generic;
using System.Linq;

namespace BobaKami
{
    /// <summary>
    /// A single persisted high-score record. Fields are public (not properties) because the
    /// adapter serializes this with Unity's JsonUtility, which only sees fields.
    /// <see cref="recordedAtUtc"/> is an ISO-8601 round-trip string ("o") — JsonUtility
    /// cannot serialize DateTime. <see cref="initials"/> is unused until initials entry ships;
    /// it exists now so adding it later needs no save-format migration.
    /// </summary>
    [Serializable]
    public struct HighScoreEntry
    {
        public int score;
        public string recordedAtUtc;
        public string initials;

        public HighScoreEntry(int score, DateTime recordedAtUtc, string initials = "")
        {
            this.score = score;
            this.recordedAtUtc = recordedAtUtc.ToString("o");
            this.initials = initials;
        }
    }

    /// <summary>
    /// Local top-<see cref="Capacity"/> score table. Entries are kept sorted descending;
    /// a tie places *after* the incumbent, so an equal score never displaces an older one.
    /// </summary>
    [Serializable]
    public class HighScoreTable
    {
        public const int Capacity = 10;

        public List<HighScoreEntry> entries = new();

        public IReadOnlyList<HighScoreEntry> Entries => entries;

        public int BestScore => entries.Count > 0 ? entries[0].score : 0;

        /// <summary>
        /// Inserts <paramref name="score"/> if it places, trimming the table to <see cref="Capacity"/>.
        /// </summary>
        /// <returns>The 1-based rank of the new entry, or 0 if it did not place.</returns>
        public int TrySubmit(int score, DateTime recordedAtUtc)
        {
            if (score <= 0) return 0;
            if (entries.Count >= Capacity && score <= entries[Capacity - 1].score) return 0;

            // Ties go after the incumbent: skip while the existing score is >= the new one.
            var index = 0;
            while (index < entries.Count && entries[index].score >= score) index++;

            entries.Insert(index, new HighScoreEntry(score, recordedAtUtc));
            Trim();

            return index + 1;
        }

        /// <summary>
        /// Repairs the table after a load. JsonUtility leaves <see cref="entries"/> null for an
        /// empty or hand-edited file, and nothing stops a save file from being out of order.
        /// </summary>
        public void Normalize()
        {
            entries ??= new List<HighScoreEntry>();
            entries.RemoveAll(entry => entry.score <= 0);
            // OrderByDescending is a stable sort; List.Sort is not, and ties must keep the
            // incumbent first to match TrySubmit's ordering.
            entries = entries.OrderByDescending(entry => entry.score).ToList();
            Trim();
        }

        private void Trim()
        {
            if (entries.Count <= Capacity) return;
            entries.RemoveRange(Capacity, entries.Count - Capacity);
        }
    }
}
