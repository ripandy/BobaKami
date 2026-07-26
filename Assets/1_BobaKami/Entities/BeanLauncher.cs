using System;
using System.Collections.Generic;
using BobaKami.Interfaces;

namespace BobaKami
{
    [Serializable]
    public class BeanLauncher
    {
        public float launchRate = 2;

        // Starting pace, restored on Initialize so a restart doesn't inherit the previous run's
        // speed. Serialized as a designer knob (see BeanLauncherData.asset); curve tuning is Feature 17.
        public float initialLaunchRate = 1;

        private readonly Dictionary<int, Bean> beans = new();
        private readonly Random rnd = new();
        private int nextId;

        public int LaunchedBeanCount => beans.Count;
        public int LaunchDelay => (int)Math.Floor(1000 / launchRate);

        public BeanLauncher()
        {
        }

        // Seeded constructor for deterministic tests.
        internal BeanLauncher(int seed)
        {
            rnd = new Random(seed);
        }

        public void Initialize()
        {
            nextId = 0;
            beans.Clear();
            launchRate = initialLaunchRate;
        }

        public Bean LaunchBean()
        {
            var rndVal = rnd.Next(0, 3) - 1;
            var bean = new Bean(nextId++, (DirectionEnum)rndVal);
            beans.Add(bean.Id, bean);
            return bean;
        }
        
        public bool TryGetBean(int id, out Bean bean)
        {
            return beans.TryGetValue(id, out bean);
        }

        public void RemoveBean(int id)
        {
            beans.Remove(id);
        }

        // Fed the run's peak combo (not current) so pace never falls back after a hit.
        // Clamped to initialLaunchRate — the game never gets slower than its starting pace.
        public void UpdateLaunchRate(int peakCombo)
        {
            launchRate = (float)Math.Max(initialLaunchRate, Math.Log(peakCombo, 2) * 0.5f);
        }
    }
}