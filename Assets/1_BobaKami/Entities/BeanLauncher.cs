using System;
using System.Collections.Generic;
using BobaKami.Interfaces;

namespace BobaKami
{
    [Serializable]
    public class BeanLauncher
    {
        public float launchRate = 2;
        
        private readonly Dictionary<int, Bean> beans = new();
        private readonly Random rnd = new();
        
        public int LaunchedBeanCount => beans.Count;
        public int LaunchDelay => (int)Math.Floor(1000 / launchRate);

        public void Initialize()
        {
            Bean.id = 0;
            beans.Clear();
        }
        
        public Bean LaunchBean()
        {
            var rndVal = rnd.Next(0, 3) - 1;
            var bean = new Bean((DirectionEnum)rndVal);
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

        public void UpdateLaunchRate(int comboCount)
        {
            launchRate = (float)Math.Max(1, Math.Log(comboCount, 2) * 0.5f);
        }
    }
}