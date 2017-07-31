using System;
using System.Linq;

namespace Ampere
{
    class Program
    {
        public static string PlayerName { get; private set; }

        public static Dungeon Dungeon { get; set; }

        public static Battle Battle { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Ampere - A Roguelike Deck Builder With Poker Combat");
            Console.WriteLine("Create by Shawn Rakowski");
            Console.WriteLine("For Ludum Dare 39");
            Console.WriteLine();
            Console.Write("Name: ");
            PlayerName = Console.ReadLine();
            Console.Clear();

            Dungeon = new Dungeon();
            var player = new HumanGamePlayer(new PlayerState(10, 1))
            {
                Pos = new Point(4, 2)
            };
            Dungeon.CurrentFloor.AddPlayer(player);

            while (true)
            {
                Console.CursorVisible = false;
                Draw(Dungeon, player);
                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.NumPad7:
                        Battle = player.ActNorthWest(Dungeon);
                        break;
                    case ConsoleKey.NumPad8:
                        Battle = player.ActNorth(Dungeon);
                        break;
                    case ConsoleKey.NumPad9:
                        Battle = player.ActNorthEast(Dungeon);
                        break;
                    case ConsoleKey.NumPad4:
                        Battle = player.ActWest(Dungeon);
                        break;
                    case ConsoleKey.NumPad6:
                        Battle = player.ActEast(Dungeon);
                        break;
                    case ConsoleKey.NumPad1:
                        Battle = player.ActSouthWest(Dungeon);
                        break;
                    case ConsoleKey.NumPad2:
                        Battle = player.ActSouth(Dungeon);
                        break;
                    case ConsoleKey.NumPad3:
                        Battle = player.ActSouthEast(Dungeon);
                        break;
                }

                if (Battle != null) Battle.Execute();
                Battle = null;
            }
        }

        public static void DrawBattleBet(GameCard[] availCards)
        {
            Console.Clear();
            DrawStats(Dungeon, Battle.PlayerBattlePlayer.Player as HumanGamePlayer, (Battle.OpponentBattlePlayer.Player as Monster));

            Console.WriteLine($"BATTLE POKER");
            Console.WriteLine();
            Console.WriteLine("YOUR HAND:");
            Console.WriteLine("--------------------");
            Console.WriteLine(String.Join("\n", Battle.PokerRound.PlayerHand.CardSlots.Select((c, j) => $"{j + 1}. {c.Card.RankName} of {c.Card.SuitName}")));
            //Console.WriteLine(String.Join("\n", Battle.PokerRound.PlayerHand.CardSlots.Select((c, i) => $"{i + 1}. [{(c.Keep ? "KEEP" : "TOSS")}] {c.Card.RankName} of {c.Card.SuitName}")));
            Console.WriteLine();
            //foreach (var card in Battle.PokerRound.Pot.Items.Cast<BattleCard>())
            //    Console.WriteLine(card.Card.Name);

            Console.WriteLine("SELECT A CARD TO ADD TO THE POT:");
            Console.WriteLine("--------------------");
            int i = 1;
            foreach (var card in availCards)
            {
                Console.WriteLine($"{i}. {card.Name}");
                i++;
            }
        }

        private static void Draw(Dungeon dungeon, HumanGamePlayer player)
        {
            dungeon.CurrentFloor.Pieces.ForEach(p =>
            {
                Console.SetCursorPosition(p.Pos.X, p.Pos.Y);
                Console.Write(p.Character);
            });

            DrawStats(dungeon, player);
        }

        private static void DrawStats(Dungeon dungeon, HumanGamePlayer player, Monster monster = null)
        {
            int i = 0;
            Console.SetCursorPosition(50, i++);
            Console.Write("Dungeon Level " + (dungeon.CurrentFloorIdx + 1).ToString());
            Console.SetCursorPosition(50, i++);
            Console.Write("--------------------");
            Console.SetCursorPosition(50, i++);
            Console.Write($"NAME: {PlayerName}");
            Console.SetCursorPosition(50, i++);
            Console.Write($"POWR: {player.PlayerState.PowerLevel.Value}/{player.PlayerState.PowerLevel.MaxValue}");

            if (monster != null)
            {
                i++;
                Console.SetCursorPosition(50, i++);
                Console.Write("Opponent");
                Console.SetCursorPosition(50, i++);
                Console.Write("--------------------");
                Console.SetCursorPosition(50, i++);
                Console.Write($"NAME: {monster.Name}");
                Console.SetCursorPosition(50, i++);
                Console.Write($"POWR: {monster.PlayerState.PowerLevel.Value}/{player.PlayerState.PowerLevel.MaxValue}");
            }
        }
    }
}
