using System.Collections.Generic;

namespace Ampere
{
    class Stat
    {
        public int MaxValue { get; }
        public int Value { get; }
        public Stat(int maxValue, int value)
        {
            MaxValue = maxValue;
            Value = value;
        }

        internal Stat ModifyLevel(int amount) => new Stat(MaxValue, Util.Clamp(Value + amount, 0, MaxValue));
    }

    class PlayerState
    {
        public Stat PowerLevel { get; private set; }

        public Stat BaseDamage { get; private set; }

        public bool IsDead => PowerLevel.Value == 0;

        private List<GameCard> _cards = new List<GameCard>();

        public IEnumerable<GameCard> CardInventory => _cards;

        public PlayerState(int maxPowerLevel, int initBaseDamage)
        {
            PowerLevel = new Stat(maxPowerLevel, maxPowerLevel);
            BaseDamage = new Stat(20, initBaseDamage);
        }

        internal void ModifyPowerLevel(int amount) => PowerLevel = PowerLevel.ModifyLevel(amount);
    }
}
