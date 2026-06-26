using System;
using BobaKami.DataTransferObjects;
using BobaKami.Interfaces;

namespace BobaKami
{
    [Serializable]
    public class Player
    {
        public int hp = 100;
        
        internal int CurrentHp { get; set; }
        internal int BeanEatenCount { get; set; }

        private int comboCount;
        public int ComboCount
        {
            get => comboCount;
            set
            {
                comboCount = value;
                MaxComboCount = Math.Max(MaxComboCount, comboCount);
            }
        }

        public int MaxComboCount { get; internal set; }
        
        public DirectionEnum Direction { get; set; }

        internal bool IsAlive => CurrentHp > 0;
        internal float HealthPercentage => (float)CurrentHp / hp;

        private const int BeanHeal = 1;
        private const int BeanDamage = BeanHeal * 20;
        
        public Player()
        {
            Initialize();
        }

        public GameStatsDto GameStats => new(BeanEatenCount, ComboCount);

        internal void Initialize()
        {
            CurrentHp = hp;
            BeanEatenCount = 0;
            ComboCount = 0;
            Direction = DirectionEnum.Forward;
        }

        internal void EatBean()
        {
            BeanEatenCount++;
            ComboCount++;
            CurrentHp = Math.Min(CurrentHp + BeanHeal, hp);
        }

        internal void Damaged()
        {
            ComboCount = 0;
            CurrentHp = Math.Max(CurrentHp - BeanDamage, 0);
        }
    }
}