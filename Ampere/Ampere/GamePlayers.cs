using System;
using System.Collections.Generic;
using System.Linq;

namespace Ampere
{
    class HumanGamePlayer : GamePlayer
    {
        public HumanGamePlayer(PlayerState state) : base(state)
        {
        }

        public override char Character => '@';

        public override void ConfigureKeeps(Hand hand)
        {
            var draw = false;
            while (!draw)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.NumPad5:
                        hand.CardSlots[key.Key - ConsoleKey.NumPad1].Keep = !hand.CardSlots[key.Key - ConsoleKey.NumPad1].Keep;
                        break;

                    case ConsoleKey.Enter:
                        draw = true;
                        break;
                }
            }
        }

        public override IBettableItem[] ReceiveBet(int requiredCards, BattleCard[] cardsToChooseFrom)
        {
            cardsToChooseFrom.ToList().ForEach(c => c.Selected = false);

            var allCards = cardsToChooseFrom.Concat(new[] {
                new BattleCard(GameCards.Run, this),
                new BattleCard(GameCards.Pass, this),
                });

            var availableCards = new List<BattleCard>(allCards);

            Program.DrawBattleBet(allCards.Select(c => c.Card).ToArray());

            var selectedCards = new List<BattleCard>();
            while (selectedCards.Count < requiredCards)
            {
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.NumPad1:
                    case ConsoleKey.NumPad2:
                    case ConsoleKey.NumPad3:
                    case ConsoleKey.NumPad4:
                    case ConsoleKey.NumPad5:
                    case ConsoleKey.NumPad6:
                    case ConsoleKey.NumPad7:
                        availableCards.ToList().ForEach(c => c.Selected = false);
                        availableCards[key.Key - ConsoleKey.NumPad1].Selected = true;
                        break;

                    case ConsoleKey.Enter:
                        
                        break;
                }
            }

            cardsToChooseFrom.ToList().ForEach(c => c.Selected = false);
            return selectedCards.Cast<IBettableItem>().ToArray();
        }

        internal Battle ActNorthWest(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(-1, -1));
        internal Battle ActNorth(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(0, -1));
        internal Battle ActNorthEast(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(1, -1));
        internal Battle ActWest(Dungeon dungeon) => Act(dungeon,this.Pos + new Point(-1, 0));
        internal Battle ActEast(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(1, 0));
        internal Battle ActSouthWest(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(-1, 1));
        internal Battle ActSouth(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(0, 1));
        internal Battle ActSouthEast(Dungeon dungeon) => Act(dungeon, this.Pos + new Point(1, 1));

        private Battle Act(Dungeon dg, Point point)
        {
            var mon = dg.CurrentFloor.Pieces.OfType<Monster>().FirstOrDefault(p => p.Pos == point);
            if (mon != null)
            {
                return new Battle(this, mon);
            }

            var target = dg.CurrentFloor.Pieces.FirstOrDefault(p => p.Pos == point);
            switch (target)
            {
                case Floor floor:
                case Door door:
                case Corridore corridore:
                    Pos = point;
                    break;

                case NextStairs next:
                    dg.CurrentFloor.Pieces.Remove(this);
                    dg.CurrentFloorIdx++;
                    this.Pos = dg.CurrentFloor.Pieces.OfType<PrevStairs>().First().Pos;
                    dg.CurrentFloor.Pieces.Add(this);
                    break;

                default: break;
            }
            return null;
        }
    }

    class Monster : GamePlayer
    {
        public string Name { get; }

        public override char Character { get; }

        public Monster(string name, char character, PlayerState state) : base(state)
        {
            this.Name = name;
            this.Character = character;
        }

        public override void ConfigureKeeps(Hand hand)
        {
        }

        public override IBettableItem[] ReceiveBet(int requiredCards, BattleCard[] cardsToChooseFrom)
        {
            return null;
        }

        internal static IGamePiece CreateForLevel(int level, Point pos)
        {
            var rat = Rat;
            rat.Pos = pos;
            return rat;
        }

        public static Monster Rat => new Monster("Rat", 'r', new PlayerState(5, 1));
    }

}
