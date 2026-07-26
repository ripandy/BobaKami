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
        internal int BobaEatenCount { get; set; }
        public int Score { get; internal set; }

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

        private const int BobaHeal = 1;
        private const int BobaDamage = BobaHeal * 20;
        
        public Player()
        {
            Initialize();
        }

        public GameStatsDto GameStats =>
            new(Score, ComboCount, MaxComboCount, BobaEatenCount, ScoreRules.MultiplierFor(ComboCount));

        internal void Initialize()
        {
            CurrentHp = hp;
            BobaEatenCount = 0;
            Score = 0;
            MaxComboCount = 0;
            ComboCount = 0;
            Direction = DirectionEnum.Forward;
        }

        internal void EatBoba()
        {
            BobaEatenCount++;
            ComboCount++;
            Score += ScoreRules.GetScore(ComboCount);
            CurrentHp = Math.Min(CurrentHp + BobaHeal, hp);
        }

        internal void Damaged()
        {
            ComboCount = 0;
            CurrentHp = Math.Max(CurrentHp - BobaDamage, 0);
        }
    }
}