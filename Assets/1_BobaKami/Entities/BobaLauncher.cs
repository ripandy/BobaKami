using System;
using System.Collections.Generic;
using BobaKami.Interfaces;

namespace BobaKami
{
    [Serializable]
    public class BobaLauncher
    {
        public float launchRate = 2;

        // Starting pace, restored on Initialize so a restart doesn't inherit the previous run's
        // speed. Serialized as a designer knob (see BobaLauncherData.asset); curve tuning is Feature 17.
        public float initialLaunchRate = 1;

        private readonly Dictionary<int, Boba> bobas = new();
        private readonly Random rnd = new();
        private int nextId;

        public int LaunchedBobaCount => bobas.Count;
        public int LaunchDelay => (int)Math.Floor(1000 / launchRate);

        public BobaLauncher()
        {
        }

        // Seeded constructor for deterministic tests.
        internal BobaLauncher(int seed)
        {
            rnd = new Random(seed);
        }

        public void Initialize()
        {
            nextId = 0;
            bobas.Clear();
            launchRate = initialLaunchRate;
        }

        public Boba LaunchBoba()
        {
            var rndVal = rnd.Next(0, 3) - 1;
            var boba = new Boba(nextId++, (DirectionEnum)rndVal);
            bobas.Add(boba.Id, boba);
            return boba;
        }
        
        public bool TryGetBoba(int id, out Boba boba)
        {
            return bobas.TryGetValue(id, out boba);
        }

        public void RemoveBoba(int id)
        {
            bobas.Remove(id);
        }

        // Fed the current combo so pace rises with the chain. On a drop the combo resets and
        // ResetLaunchRate() drops the pace back to the floor, giving the player a breather to
        // recover instead of a fast barrage cascading into more drops.
        // Clamped to initialLaunchRate — the game never gets slower than its starting pace.
        public void UpdateLaunchRate(int combo)
        {
            launchRate = (float)Math.Max(initialLaunchRate, Math.Log(combo, 2) * 0.5f);
        }

        // Called when a boba drops: reset the pace to the starting floor so the player gets a
        // breather. The pace then rebuilds via UpdateLaunchRate as the combo climbs again.
        public void ResetLaunchRate()
        {
            launchRate = initialLaunchRate;
        }
    }
}