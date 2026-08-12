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
    /// Local top-<see cref="Capacity"/> score tables, kept separately for face tracking and for
    /// touch/pointer/key play. The booth showed the two are not comparable — aiming with your
    /// head is a different game from dragging a finger — so they rank against their own kind
    /// rather than being reconciled with a fudge factor.
    /// <para>
    /// Within each table, entries are kept sorted descending and a tie places *after* the
    /// incumbent, so an equal score never displaces an older one.
    /// </para>
    /// </summary>
    [Serializable]
    public class HighScoreTable
    {
        public const int Capacity = 10;

        /// <summary>
        /// Face-tracking scores. Deliberately keeps the pre-split field name: every save written
        /// before the tables were separated came from a device where Auto resolved to face
        /// tracking, so those entries *are* face scores and land in the right table for free.
        /// </summary>
        public List<HighScoreEntry> entries = new();

        /// <summary>Pointer, touch and key/gamepad scores. Null in any pre-split save; <see cref="Normalize"/> repairs it.</summary>
        public List<HighScoreEntry> touchEntries = new();

        public IReadOnlyList<HighScoreEntry> EntriesFor(bool faceTracking) => TableFor(faceTracking);

        public int BestScoreFor(bool faceTracking)
        {
            var table = TableFor(faceTracking);
            return table.Count > 0 ? table[0].score : 0;
        }

        /// <summary>
        /// Inserts <paramref name="score"/> into the table for this input mode if it places there,
        /// trimming that table to <see cref="Capacity"/>. The two tables rank independently.
        /// </summary>
        /// <returns>The 1-based rank of the new entry within its own table, or 0 if it did not place.</returns>
        public int TrySubmit(int score, bool faceTracking, DateTime recordedAtUtc)
        {
            if (score <= 0) return 0;

            var table = TableFor(faceTracking);
            if (table.Count >= Capacity && score <= table[Capacity - 1].score) return 0;

            // Ties go after the incumbent: skip while the existing score is >= the new one.
            var index = 0;
            while (index < table.Count && table[index].score >= score) index++;

            table.Insert(index, new HighScoreEntry(score, recordedAtUtc));
            Trim(table);

            return index + 1;
        }

        /// <summary>
        /// Repairs both tables after a load. JsonUtility leaves a list null when the file is empty,
        /// hand-edited, or predates the face/touch split, and nothing stops a save file from being
        /// out of order.
        /// </summary>
        public void Normalize()
        {
            entries = Repair(entries);
            touchEntries = Repair(touchEntries);

            static List<HighScoreEntry> Repair(List<HighScoreEntry> table)
            {
                table ??= new List<HighScoreEntry>();
                table.RemoveAll(entry => entry.score <= 0);
                // OrderByDescending is a stable sort; List.Sort is not, and ties must keep the
                // incumbent first to match TrySubmit's ordering.
                table = table.OrderByDescending(entry => entry.score).ToList();
                Trim(table);
                return table;
            }
        }

        private List<HighScoreEntry> TableFor(bool faceTracking) => faceTracking ? entries : touchEntries;

        private static void Trim(List<HighScoreEntry> table)
        {
            if (table.Count <= Capacity) return;
            table.RemoveRange(Capacity, table.Count - Capacity);
        }
    }
}
