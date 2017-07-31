using System;
using System.Collections.Generic;
using System.Linq;

namespace Ampere
{
    interface IGamePiece
    {
        Point Pos { get; set; }
        char Character { get; }
    }

    class Wall : IGamePiece
    {
        public Point Pos { get; set; }
        public virtual char Character => ' ';
    }

    class HWall : Wall
    {
        public override char Character => '-';
    }

    class VWall : Wall
    {
        public override char Character => '|';
    }


    class Floor : IGamePiece
    {
        public Point Pos { get; set; }
        public char Character => '.';
    }

    class PrevStairs : IGamePiece
    {
        public Point Pos { get; set; }
        public char Character => '<';
    }

    class NextStairs : IGamePiece
    {
        public Point Pos { get; set; }
        public char Character => '>';
    }

    class Corridore : IGamePiece
    {
        public Point Pos { get; set; }
        public char Character => '#';
    }

    class Door : IGamePiece
    {
        public Point Pos { get; set; }
        public char Character => '+';
    }

    class Void : IGamePiece
    {
        public Point Pos { get; set; }
        public char Character => ' ';
    }

    class DungeonFloor
    {
        public List<IGamePiece> Pieces { get; }

        protected DungeonFloor(List<IGamePiece> pieces) => Pieces = pieces;

        public static DungeonFloor Generate(int i)
        {
            var pieces = new List<IGamePiece>();
            var testFloor =
@"
 ------------                                
 |..........|                                
 |.<..????..+##################              
 |..........|     #           #              
 ------------     #  ---------+----          
                  #  |............|          
       ##############+.?????????..|          
       #             |.?????????..+####      
    ---+---------    |............|   #      
    |...........|    -+------------   #      
    |..???????..|     #               #      
    |..???????..|     #         ------+----- 
    |...........+######         |..........| 
    -+-----------               |..???.....| 
     #                      -----..???..>..| 
     #######################+..............| 
                            ---------------- 
                                             
".Split('\n').Where(l => !String.IsNullOrWhiteSpace(l));


            int monstersToSpawn = Rnd.NextRange(1, 3);
            int x = 0;
            List<Point> spawnPoints = new List<Point>();
            int y = 0;
            foreach (var line in testFloor)
            {
                x = 0;
                foreach (var c in line)
                {
                    var pos = new Point(x, y);
                    switch (c)
                    {
                        case '+': pieces.Add(new Door() {  Pos = pos }); break;
                        case '#': pieces.Add(new Corridore() { Pos = pos }); break;
                        case '-': pieces.Add(new HWall() { Pos = pos }); break;
                        case '|': pieces.Add(new VWall() { Pos = pos }); break;
                        case '<': pieces.Add(i == 0 ? new Floor() { Pos = pos } as IGamePiece : new PrevStairs() { Pos = pos }); break;
                        case '>': pieces.Add(i == 6 ? new Floor() { Pos = pos } as IGamePiece : new NextStairs() { Pos = pos }); break;
                        case '.': pieces.Add(new Floor() { Pos = pos }); break;
                        case ' ': pieces.Add(new Void() { Pos = pos }); break;
                        case '?':
                            spawnPoints.Add(pos);
                            pieces.Add(new Floor() { Pos = pos });
                            break;
                    }
                    x++;
                }
                y++;
            }

            var sp = new Queue<Point>(spawnPoints.OrderBy(s => Rnd.Next));
            for (int j = 0; j < monstersToSpawn; j++)
            {
                pieces.Add(Monster.CreateForLevel(j, sp.Dequeue()));
            }


            return new DungeonFloor(pieces);
        }

        internal void AddPlayer(HumanGamePlayer player)
        {
            Pieces.Add(player);
        }
    }

    class Dungeon
    {
        public int CurrentFloorIdx { get; set; }
        public DungeonFloor CurrentFloor => Floors[CurrentFloorIdx];
        public DungeonFloor[] Floors { get; }
        public Dungeon()
        {
            Floors = new DungeonFloor[7];
            for (int i = 0; i < Floors.Length; i++)
                Floors[i] = DungeonFloor.Generate(i);
        }
    }
}
