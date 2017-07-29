using System;
using System.Linq;

namespace Ampere
{
    class Program
    {
        static void Main(string[] args)
        {
            var board = (IGameBoard)new DungeonBoard()
                .InitializeBoard()
                .WithPlayerAt(10, 10)
                .WithRoomAt(8, 8)
                .WithRatAt(15, 12);

            while (true)
            {
                Draw(board);
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow: board = board.PlayerUp(); break;
                    case ConsoleKey.DownArrow: board = board.PlayerDown(); break;
                    case ConsoleKey.LeftArrow: board = board.PlayerLeft(); break;
                    case ConsoleKey.RightArrow: board = board.PlayerRight(); break;

                    case ConsoleKey.D0:
                    case ConsoleKey.D1:
                    case ConsoleKey.D2:
                    case ConsoleKey.D3:
                    case ConsoleKey.D4:
                    case ConsoleKey.D5:
                    case ConsoleKey.D6:
                    case ConsoleKey.D7:
                    case ConsoleKey.D8:
                    case ConsoleKey.D9:
                        board = board.PlayerOption(key.Key - ConsoleKey.D0);
                        break;

                    case ConsoleKey.Enter:
                        board = board.PlayerSelect();
                        break;
                }
            }
        }

        private static void Draw(IGameBoard gameBoard)
        {
            Console.Clear();
            Console.CursorVisible = false;
            switch (gameBoard)
            {
                case DungeonBoard dungeon:
                    dungeon.Pieces
                        .Where(ShouldDisplayInConsole)
                        .ToList()
                        .ForEach(gp =>
                        {
                            Console.SetCursorPosition(gp.Pos.X, gp.Pos.Y);
                            Console.Write(gp.Character);
                        });
                    break;

                case BattleBoard battle:
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("BATTLE!");
                    Console.WriteLine($"Opponent: {battle.Round.Opponent.Piece.Type.ToString()}");
                    Console.WriteLine($"Your hand:");
                    Console.Write(string.Join("\n",
                        battle.Round.Player.Hand.Cards
                            .Select((c, i) => $" {i + 1}. {c.RankName} of {c.SuitName} [{(battle.Round.Player.Hand.CardsToKeep[i] ? "KEEP" : " ")}] ")));
                    Console.WriteLine();
                    break;
            }
        }

        private static bool ShouldDisplayInConsole(GamePiece piece) =>
            PointIsInConsole(piece.Pos);

        private static bool PointIsInConsole(Point pos) =>
            pos.X >= 0 && pos.X < Console.BufferWidth && pos.Y >= 0 && pos.Y < Console.BufferHeight;
    }
}
