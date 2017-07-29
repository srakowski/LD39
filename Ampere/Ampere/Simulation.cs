using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Ampere.Util;

namespace Ampere
{
    enum Suit
    {
        Clubs,
        Hearts,
        Diamonds,
        Spades
    }

    enum Rank
    {
        Ace = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        AcesHigh,
    }

    struct Card
    {
        public Suit Suit { get; }
        public string SuitName => Suit.ToString();

        public Rank Rank { get; }
        public string RankName => Rank.ToString();



        public Card(Suit suit, Rank rank)
        {
            Suit = suit;
            Rank = rank;
        }
    }

    struct Deck
    {
        public IEnumerable<Card> Cards { get; }
        public Deck(IEnumerable<Card> cards) => Cards = cards;

        public static Deck NewDeck() => new Deck(Suits().SelectMany(s => Ranks().Select(r => new Card(s, r)))).Shuffle();
        public Deck Shuffle() => new Deck(this.Cards.OrderBy(c => Rnd.Next));
        public (Card[] Cards, Deck Deck) Draw(int count) => (Cards.Take(count).ToArray(), new Deck(Cards.Skip(count)));
        private static IEnumerable<Rank> Ranks() => Enum.GetValues(typeof(Rank)).Cast<Rank>().Where(r => r != Rank.AcesHigh);
        private static IEnumerable<Suit> Suits() => Enum.GetValues(typeof(Suit)).Cast<Suit>();
    }

    struct Hand
    {
        public Card[] Cards { get; }
        public bool[] CardsToKeep { get; }

        public Hand(Card[] cards)
            : this(cards, cards.Select(c => false).ToArray())
        {
        }

        public Hand(Card[] cards, bool[] cardsToToss)
        {
            Cards = cards;
            CardsToKeep = cardsToToss;
        }

        public static (Hand Hand, Deck Deck) NewHand(Deck deck)
        {
            var result = deck.Draw(5);
            return (new Hand(result.Cards), result.Deck);
        }

        public (Hand Hand, Deck Deck) Update(Deck deck)
        {
            if (CardsToKeep.All(keep => keep)) return (this, deck);
            var result = deck.Draw(CardsToKeep.Count(keep => !keep));
            var cardQueue = new Queue<Card>(result.Cards);
            var cardsToKeep = CardsToKeep;
            return (new Hand(Cards.Select((card, index) =>
                cardsToKeep[index]
                    ? card
                    : cardQueue.Dequeue())
                    .ToArray(), 
                    cardsToKeep.Select(c => false).ToArray()
                ), result.Deck);
        }

        public (bool HasHand, string Name)[] HandChecks =>
            new[]
            {
                (IsOnePair, "One Pair"),
                (IsTwoPair, "Two Pair"),
                (IsThreeOfAKind, "Three of a Kind"),
                (IsStraight, "Straight"),
                (IsFlush, "Flush"),
                (IsFullHouse, "Full House"),
                (IsFourOfAKind, "Four of a Kind"),
                (IsStraightFlush, "Straight Flush"),
                (IsRoyalFlush, "Royal Flush")
            };

        public bool IsOnePair => HasOfAKind(2);

        public bool IsTwoPair =>
            Cards.GroupBy(c => c.Rank).GroupBy(g => g.Count()).Any(g => g.Key == 2 && g.Count() == 2);

        public bool IsThreeOfAKind => HasOfAKind(3);

        public bool IsStraight => AreRanksSequential(Cards) ||
            AreRanksSequential(Cards.Select(c => c.Rank == Rank.Ace ? new Card(c.Suit, Rank.AcesHigh) : c).ToArray());

        public bool IsFlush =>
            Cards.Select(c => c.Suit).Distinct().Count() == 1;

        public bool IsFullHouse =>
            IsThreeOfAKind && HasOfAKind(2);

        public bool IsFourOfAKind => HasOfAKind(4);

        public bool IsStraightFlush => IsFlush & IsStraight;

        public bool IsRoyalFlush =>
            IsStraightFlush && Cards.OrderBy(c => c.Rank).First().Rank == Rank.Ace && 
                Cards.OrderByDescending(c => c.Rank).First().Rank == Rank.King;

        private static bool AreRanksSequential(Card[] cards) =>
            cards.Skip(1).Select((c, i) => c.Rank == (cards[i].Rank + 1)).All(x => x);

        private bool HasOfAKind(int count) => 
            Cards.GroupBy(c => c.Rank).Any(g => g.Count() == count);

        public Hand ToggleKeep(int num) =>
            new Hand(this.Cards, CardsToKeep.Select((c, i) => i == (num - 1) ? !c : c).ToArray());
        
    }

    struct Battery
    {
        public int Power { get; }
        public int MaxPower { get; }
        public Battery(int levelAndMax) : this(levelAndMax, levelAndMax) { }
        public Battery(int power, int maxPower)
        {
            Power = Clamp(power, 0, maxPower);
            MaxPower = maxPower;
        }
        
        public Battery Charge(int amount) => AdjustPowerTo(Power + Math.Abs(amount));
        public Battery Discharge(int amount) => AdjustPowerTo(Power - Math.Abs(amount));
        private Battery AdjustPowerTo(int amount) => new Battery(amount, MaxPower);

    }

    enum GamePieceType
    {
        Void = ' ',
        Wall = '#',
        Player = '@',
        Rat = 'r'
    }

    struct GamePiece
    {
        public GamePieceType Type { get; }
        public int Id { get; }
        public Point Pos { get; }
        public Battery Battery { get; }
        public char Character => (char)Type;
        public GamePiece(GamePieceType type, int id, Point pos, Battery battery)
        {
            Type = type;
            Id = id;
            Pos = pos;
            Battery = battery;
        }

        public GamePiece MoveTo(Point pos) =>
            new GamePiece(Type, Id, pos, Battery);

        private static int _next = 0;
        public static int NextId() => (_next++);
        private static GamePiece NewGamePiece(GamePieceType type, Point pos, int batteryPower) => 
            new GamePiece(type, NextId(), pos, new Battery(batteryPower));

        public static GamePiece NewVoid(Point pos) =>   new GamePiece(GamePieceType.Void, 0, pos, new Battery(0));
        public static GamePiece NewWall(Point pos) =>   NewGamePiece(GamePieceType.Wall,    pos, 0);
        public static GamePiece NewPlayer(Point pos) => NewGamePiece(GamePieceType.Player,  pos, 10);
        public static GamePiece NewRat(Point pos) =>    NewGamePiece(GamePieceType.Rat,     pos, 10);
    }

    interface IGameBoard
    {
        IGameBoard PlayerUp();
        IGameBoard PlayerDown();
        IGameBoard PlayerLeft();
        IGameBoard PlayerRight();
        IGameBoard PlayerSelect();
        IGameBoard PlayerOption(int num);
    }

    struct DungeonBoard : IGameBoard
    {
        public IEnumerable<GamePiece> Pieces { get; }

        public DungeonBoard(IEnumerable<GamePiece> board) => Pieces = board;

        public DungeonBoard InitializeBoard() => new DungeonBoard(Enumerable.Empty<GamePiece>());

        public DungeonBoard WithRoomAt(int x, int y) => new DungeonBoard(
            this.Pieces.Concat(
                Enumerable.Range(y, 6)
                    .SelectMany(ry => (ry == y || ry == (y + 5))
                        ? Enumerable.Range(x, 22).Select(rx => GamePiece.NewWall(new Point(rx, ry))).Cast<GamePiece>()
                        : new GamePiece[] { GamePiece.NewWall(new Point(x, ry)), GamePiece.NewWall(new Point(x + 21, ry)) })
                ));

        public DungeonBoard WithPlayerAt(int x, int y) => WithPlayerAt(new Point(x, y));

        public DungeonBoard WithPlayerAt(Point pos) => Place(GamePiece.NewPlayer(pos));

        public DungeonBoard WithRatAt(int x, int y) => WithRatAt(new Point(x, y));

        public DungeonBoard WithRatAt(Point pos) => Place(GamePiece.NewRat(pos));

        public IGameBoard PlayerUp() =>
            PlayerActOn(OccupantAt(Player().Pos + new Point(0, -1)));

        public IGameBoard PlayerDown() =>
            PlayerActOn(OccupantAt(Player().Pos + new Point(0, 1)));

        public IGameBoard PlayerLeft() =>
            PlayerActOn(OccupantAt(Player().Pos + new Point(-1, 0)));

        public IGameBoard PlayerRight() =>
            PlayerActOn(OccupantAt(Player().Pos + new Point(1, 0)));

        public IGameBoard PlayerSelect() => this;

        public IGameBoard PlayerOption(int num) => this;

        public IGameBoard PlayerActOn(GamePiece target) =>
            target.Type == GamePieceType.Void
                ? MovePieceTo(Player(), target.Pos)
            : target.Type == GamePieceType.Rat
                ? Battle(target)
            : this;

        public GamePiece Player() =>
            Pieces.FirstOrDefault(gp => gp.Type == GamePieceType.Player);

        private DungeonBoard MovePieceTo(GamePiece gamePiece, Point pos) =>
            Place(gamePiece.MoveTo(pos));

        private DungeonBoard Place(GamePiece gamePiece) =>
            new DungeonBoard(Pieces.Where(gp => gp.Id != gamePiece.Id).Concat(new[] { gamePiece }));

        public GamePiece OccupantAt(Point pos) =>
            Pieces.Any(p => p.Pos == pos)
                ? Pieces.First(p => p.Pos == pos)
                : GamePiece.NewVoid(pos);

        private IGameBoard Battle(GamePiece opponent) =>
            new BattleBoard(this, Player(), opponent);
    }

    struct Battler
    {
        public Hand Hand { get; }
        public GamePiece Piece { get; }
        public Battler(Hand hand, GamePiece piece)
        {
            this.Hand = hand;
            this.Piece = piece;
        }

        public Battler ToggleKeep(int num) =>
            new Battler(Hand.ToggleKeep(num), this.Piece);

        public (Battler Battler, Deck Deck) UpdateHand(Deck deck)
        {
            var result = Hand.Update(deck);
            return (new Battler(result.Hand, Piece), result.Deck);
        }
    }

    struct Round
    {
        public Deck Deck { get; }
        public Battler Player { get; }
        public Battler Opponent { get; }
        public Round(Deck deck, Battler player, Battler opponent)
        {
            this.Deck = deck;
            this.Player = player;
            this.Opponent = opponent;
        }

        public Round TogglePlayerKeep(int num) =>
            new Round(this.Deck, this.Player.ToggleKeep(num), this.Opponent);

        public Round Step() =>
            PlayOpponent()
                .UpdatePlayerHand()
                .UpdateOpponentHand();

        private Round PlayOpponent() => this;

        private Round UpdatePlayerHand()
        {
            var result = Player.UpdateHand(Deck);
            return new Round(result.Deck, result.Battler, this.Opponent);
        }

        private Round UpdateOpponentHand()
        {
            var result = Opponent.UpdateHand(Deck);
            return new Round(result.Deck, this.Player, result.Battler);
        }
    }

    struct BattleBoard : IGameBoard
    {
        private DungeonBoard DungeonBoard { get; }
        public Round Round { get; }
        public BattleBoard(DungeonBoard dungeonBoard, GamePiece player, GamePiece opponent)
        {
            DungeonBoard = dungeonBoard;
            var (hand, deck) = Hand.NewHand(Deck.NewDeck());
            var pBattler = new Battler(hand, player);
            (hand, deck) = Hand.NewHand(deck);
            var oBattler = new Battler(hand, opponent);
            Round = new Round(deck, pBattler, oBattler);
        }

        public BattleBoard(BattleBoard prev, Round round)
        {
            this.DungeonBoard = prev.DungeonBoard;
            this.Round = round;
        }

        public IGameBoard PlayerDown() => this;

        public IGameBoard PlayerLeft() => this;

        public IGameBoard PlayerRight() => this;

        public IGameBoard PlayerUp() => this;

        public IGameBoard PlayerOption(int num) =>
            num > 0 && num <= 5
                ? new BattleBoard(this, Round.TogglePlayerKeep(num))
                : this;

        public IGameBoard PlayerSelect() =>
            new BattleBoard(this, Round.Step());
    }
}
