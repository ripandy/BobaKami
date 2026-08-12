using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace BobaKami.Tests
{
    public class HighScoreTableTests
    {
        private static readonly DateTime When = new(2026, 8, 8, 12, 0, 0, DateTimeKind.Utc);

        private const bool Face = true;
        private const bool Touch = false;

        private static HighScoreTable TableOf(bool faceTracking, params int[] scores)
        {
            var table = new HighScoreTable();
            foreach (var score in scores) table.TrySubmit(score, faceTracking, When);
            return table;
        }

        private static int[] ScoresOf(HighScoreTable table, bool faceTracking) =>
            table.EntriesFor(faceTracking).Select(entry => entry.score).ToArray();

        private static int[] FullTableScores() =>
            Enumerable.Range(1, HighScoreTable.Capacity).Select(i => i * 100).ToArray();

        // The ranking rules are run against both tables rather than just one: the split would be
        // worthless if the touch table quietly behaved differently from the face table.

        [TestCase(Face)]
        [TestCase(Touch)]
        public void BestScore_IsZero_WhenEmpty(bool faceTracking)
        {
            var table = new HighScoreTable();

            Assert.AreEqual(0, table.BestScoreFor(faceTracking));
            Assert.AreEqual(0, table.EntriesFor(faceTracking).Count);
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void TrySubmit_SortsDescending_AndReturnsRank(bool faceTracking)
        {
            var table = new HighScoreTable();

            Assert.AreEqual(1, table.TrySubmit(500, faceTracking, When));
            Assert.AreEqual(1, table.TrySubmit(900, faceTracking, When)); // beats the incumbent, takes rank 1
            Assert.AreEqual(2, table.TrySubmit(700, faceTracking, When));

            CollectionAssert.AreEqual(new[] { 900, 700, 500 }, ScoresOf(table, faceTracking));
            Assert.AreEqual(900, table.BestScoreFor(faceTracking));
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void TrySubmit_PlacesTieAfterIncumbent(bool faceTracking)
        {
            var table = new HighScoreTable();
            table.TrySubmit(500, faceTracking, When);
            var incumbent = table.EntriesFor(faceTracking)[0];

            Assert.AreEqual(2, table.TrySubmit(500, faceTracking, When.AddMinutes(1)),
                "An equal score must not displace the incumbent.");
            Assert.AreEqual(incumbent.recordedAtUtc, table.EntriesFor(faceTracking)[0].recordedAtUtc);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void TrySubmit_RejectsNonPositiveScores(int score)
        {
            var table = new HighScoreTable();

            Assert.AreEqual(0, table.TrySubmit(score, Face, When));
            Assert.AreEqual(0, table.TrySubmit(score, Touch, When));
            Assert.AreEqual(0, table.EntriesFor(Face).Count);
            Assert.AreEqual(0, table.EntriesFor(Touch).Count);
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void TrySubmit_TrimsToCapacity(bool faceTracking)
        {
            // Capacity + 1 descending scores; the smallest must fall off.
            var scores = Enumerable.Range(1, HighScoreTable.Capacity + 1).Select(i => i * 100).ToArray();
            var table = TableOf(faceTracking, scores);

            Assert.AreEqual(HighScoreTable.Capacity, table.EntriesFor(faceTracking).Count);
            Assert.AreEqual(1100, table.BestScoreFor(faceTracking));
            CollectionAssert.DoesNotContain(ScoresOf(table, faceTracking), 100);
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void TrySubmit_RejectsScoreBelowCutoff_WhenFull(bool faceTracking)
        {
            var table = TableOf(faceTracking, FullTableScores());
            var before = ScoresOf(table, faceTracking);

            Assert.AreEqual(0, table.TrySubmit(50, faceTracking, When));
            CollectionAssert.AreEqual(before, ScoresOf(table, faceTracking),
                "A rejected submission must not mutate the table.");
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void TrySubmit_RejectsTieWithLastPlace_WhenFull(bool faceTracking)
        {
            // Cutoff is 100 and ties place after the incumbent, so 100 cannot make a full table.
            var table = TableOf(faceTracking, FullTableScores());

            Assert.AreEqual(0, table.TrySubmit(100, faceTracking, When));
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void TrySubmit_InsertsMidTable_AndDropsLastPlace_WhenFull(bool faceTracking)
        {
            var table = TableOf(faceTracking, FullTableScores());

            Assert.AreEqual(6, table.TrySubmit(550, faceTracking, When));

            Assert.AreEqual(HighScoreTable.Capacity, table.EntriesFor(faceTracking).Count);
            Assert.AreEqual(550, table.EntriesFor(faceTracking)[5].score);
            CollectionAssert.DoesNotContain(ScoresOf(table, faceTracking), 100);
        }

        [Test]
        public void TrySubmit_KeepsTheTwoTablesIsolated()
        {
            var table = new HighScoreTable();

            table.TrySubmit(900, Face, When);
            table.TrySubmit(200, Touch, When);

            CollectionAssert.AreEqual(new[] { 900 }, ScoresOf(table, Face));
            CollectionAssert.AreEqual(new[] { 200 }, ScoresOf(table, Touch));
            Assert.AreEqual(900, table.BestScoreFor(Face));
            Assert.AreEqual(200, table.BestScoreFor(Touch));
        }

        [Test]
        public void TrySubmit_RanksAgainstOwnTableOnly()
        {
            // A touch score that would not place at all among the face scores still takes rank 1
            // on an empty touch table. This is the whole point of the split.
            var table = TableOf(Face, FullTableScores());

            Assert.AreEqual(0, table.TrySubmit(50, Face, When), "50 cannot place on a full face table.");
            Assert.AreEqual(1, table.TrySubmit(50, Touch, When), "The same score tops an empty touch table.");
        }

        [Test]
        public void TrySubmit_FillingOneTable_DoesNotConsumeTheOthersCapacity()
        {
            var table = TableOf(Face, FullTableScores());

            for (var i = 1; i <= HighScoreTable.Capacity; i++) table.TrySubmit(i * 10, Touch, When);

            Assert.AreEqual(HighScoreTable.Capacity, table.EntriesFor(Face).Count);
            Assert.AreEqual(HighScoreTable.Capacity, table.EntriesFor(Touch).Count);
        }

        [Test]
        public void Normalize_ReplacesNullLists()
        {
            var table = new HighScoreTable { entries = null, touchEntries = null };

            table.Normalize();

            Assert.IsNotNull(table.entries);
            Assert.IsNotNull(table.touchEntries);
            Assert.AreEqual(0, table.BestScoreFor(Face));
            Assert.AreEqual(0, table.BestScoreFor(Touch));
        }

        [Test]
        public void Normalize_MigratesPreSplitSave_AsFaceScores()
        {
            // A save written before the split has `entries` but no `touchEntries` at all, which
            // JsonUtility surfaces as null. Those runs came from a device where Auto resolved to
            // face tracking, so they belong to the face table — which is exactly the field they
            // are already in, making the migration a no-op beyond repairing the null.
            var table = new HighScoreTable
            {
                entries = new List<HighScoreEntry> { new(900, When), new(400, When) },
                touchEntries = null
            };

            table.Normalize();

            CollectionAssert.AreEqual(new[] { 900, 400 }, ScoresOf(table, Face));
            Assert.AreEqual(0, table.EntriesFor(Touch).Count);
        }

        [TestCase(Face)]
        [TestCase(Touch)]
        public void Normalize_SortsTrimsAndDropsNonPositiveScores(bool faceTracking)
        {
            var messy = new List<HighScoreEntry>(
                Enumerable.Range(1, HighScoreTable.Capacity + 2)
                    .Select(i => new HighScoreEntry(i * 100, When))
                    .Append(new HighScoreEntry(0, When)));

            var table = new HighScoreTable();
            if (faceTracking) table.entries = messy; else table.touchEntries = messy;

            table.Normalize();

            var scores = ScoresOf(table, faceTracking);
            Assert.AreEqual(HighScoreTable.Capacity, scores.Length);
            Assert.AreEqual(1200, table.BestScoreFor(faceTracking));
            CollectionAssert.AreEqual(scores.OrderByDescending(score => score).ToArray(), scores);
            CollectionAssert.DoesNotContain(scores, 0);
        }

        [Test]
        public void Entry_RecordsTimestamp_AsRoundTrippableUtc()
        {
            var table = new HighScoreTable();
            table.TrySubmit(500, Face, When);

            var parsed = DateTime.Parse(table.EntriesFor(Face)[0].recordedAtUtc, null,
                System.Globalization.DateTimeStyles.RoundtripKind);

            Assert.AreEqual(When, parsed);
            Assert.AreEqual(DateTimeKind.Utc, parsed.Kind);
        }
    }
}
