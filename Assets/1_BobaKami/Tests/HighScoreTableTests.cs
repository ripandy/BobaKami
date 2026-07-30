using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BobaKami.Tests
{
    public class HighScoreTableTests
    {
        private static readonly DateTime When = new(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);

        private static HighScoreTable TableOf(params int[] scores)
        {
            var table = new HighScoreTable();
            foreach (var score in scores) table.TrySubmit(score, When);
            return table;
        }

        private static int[] ScoresOf(HighScoreTable table) => table.Entries.Select(entry => entry.score).ToArray();

        [Test]
        public void BestScore_IsZero_WhenEmpty()
        {
            var table = new HighScoreTable();

            Assert.AreEqual(0, table.BestScore);
            Assert.AreEqual(0, table.Entries.Count);
        }

        [Test]
        public void TrySubmit_SortsDescending_AndReturnsRank()
        {
            var table = new HighScoreTable();

            Assert.AreEqual(1, table.TrySubmit(500, When));
            Assert.AreEqual(1, table.TrySubmit(900, When)); // beats the incumbent, takes rank 1
            Assert.AreEqual(2, table.TrySubmit(700, When));

            CollectionAssert.AreEqual(new[] { 900, 700, 500 }, ScoresOf(table));
            Assert.AreEqual(900, table.BestScore);
        }

        [Test]
        public void TrySubmit_PlacesTieAfterIncumbent()
        {
            var table = new HighScoreTable();
            table.TrySubmit(500, When);
            var incumbent = table.Entries[0];

            Assert.AreEqual(2, table.TrySubmit(500, When.AddMinutes(1)), "An equal score must not displace the incumbent.");
            Assert.AreEqual(incumbent.recordedAtUtc, table.Entries[0].recordedAtUtc);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void TrySubmit_RejectsNonPositiveScores(int score)
        {
            var table = new HighScoreTable();

            Assert.AreEqual(0, table.TrySubmit(score, When));
            Assert.AreEqual(0, table.Entries.Count);
        }

        [Test]
        public void TrySubmit_TrimsToCapacity()
        {
            // Capacity + 1 descending scores; the smallest must fall off.
            var scores = Enumerable.Range(1, HighScoreTable.Capacity + 1).Select(i => i * 100).ToArray();
            var table = TableOf(scores);

            Assert.AreEqual(HighScoreTable.Capacity, table.Entries.Count);
            Assert.AreEqual(1100, table.BestScore);
            CollectionAssert.DoesNotContain(ScoresOf(table), 100);
        }

        [Test]
        public void TrySubmit_RejectsScoreBelowCutoff_WhenFull()
        {
            var table = TableOf(Enumerable.Range(1, HighScoreTable.Capacity).Select(i => i * 100).ToArray());
            var before = ScoresOf(table);

            Assert.AreEqual(0, table.TrySubmit(50, When));
            CollectionAssert.AreEqual(before, ScoresOf(table), "A rejected submission must not mutate the table.");
        }

        [Test]
        public void TrySubmit_RejectsTieWithLastPlace_WhenFull()
        {
            // Cutoff is 100 and ties place after the incumbent, so 100 cannot make a full table.
            var table = TableOf(Enumerable.Range(1, HighScoreTable.Capacity).Select(i => i * 100).ToArray());

            Assert.AreEqual(0, table.TrySubmit(100, When));
        }

        [Test]
        public void TrySubmit_InsertsMidTable_AndDropsLastPlace_WhenFull()
        {
            var table = TableOf(Enumerable.Range(1, HighScoreTable.Capacity).Select(i => i * 100).ToArray());

            Assert.AreEqual(6, table.TrySubmit(550, When));

            Assert.AreEqual(HighScoreTable.Capacity, table.Entries.Count);
            Assert.AreEqual(550, table.Entries[5].score);
            CollectionAssert.DoesNotContain(ScoresOf(table), 100);
        }

        [Test]
        public void Normalize_ReplacesNullList()
        {
            var table = new HighScoreTable { entries = null };

            table.Normalize();

            Assert.IsNotNull(table.entries);
            Assert.AreEqual(0, table.BestScore);
        }

        [Test]
        public void Normalize_SortsTrimsAndDropsNonPositiveScores()
        {
            var table = new HighScoreTable
            {
                entries = new List<HighScoreEntry>(
                    Enumerable.Range(1, HighScoreTable.Capacity + 2)
                        .Select(i => new HighScoreEntry(i * 100, When))
                        .Append(new HighScoreEntry(0, When)))
            };

            table.Normalize();

            Assert.AreEqual(HighScoreTable.Capacity, table.Entries.Count);
            Assert.AreEqual(1200, table.BestScore);
            CollectionAssert.AreEqual(ScoresOf(table).OrderByDescending(score => score).ToArray(), ScoresOf(table));
            CollectionAssert.DoesNotContain(ScoresOf(table), 0);
        }

        [Test]
        public void Entry_RecordsTimestamp_AsRoundTrippableUtc()
        {
            var table = new HighScoreTable();
            table.TrySubmit(500, When);

            var parsed = DateTime.Parse(table.Entries[0].recordedAtUtc, null,
                System.Globalization.DateTimeStyles.RoundtripKind);

            Assert.AreEqual(When, parsed);
            Assert.AreEqual(DateTimeKind.Utc, parsed.Kind);
        }
    }
}
