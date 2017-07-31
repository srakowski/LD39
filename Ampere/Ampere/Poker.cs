using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class Deck
    {
        public List<Card> Cards { get; private set; }

        public Deck() =>
            Cards = new List<Card>(Suits().SelectMany(s => Ranks().Select(r => new Card(s, r))));

        public Deck Shuffle()
        {
            Cards = new List<Card>(Cards.OrderBy(c => Rnd.Next));
            return this;
        }

        public Card[] Draw(int count)
        {
            var cards = Cards.Take(count).ToArray();
            Cards = Cards.Skip(count).ToList();
            return cards;
        }

        private static IEnumerable<Rank> Ranks() => 
            Enum.GetValues(typeof(Rank)).Cast<Rank>().Where(r => r != Rank.AcesHigh);

        private static IEnumerable<Suit> Suits() => 
            Enum.GetValues(typeof(Suit)).Cast<Suit>();
    }

    class CardSlot
    {
        public Card Card { get; set; }
        public bool Keep { get; set; }
    }

    class Hand
    {
        public CardSlot[] CardSlots { get; }

        private Card[] Cards => CardSlots.Select(cs => cs.Card).ToArray();

        public Hand(Deck deck) =>
            CardSlots = deck.Draw(5).Select(c => new CardSlot
            {
                Card = c,
                Keep = false
            }).ToArray();

        public Hand Update(Deck deck)
        {
            var slotsToReplace = CardSlots.Where(c => !c.Keep);
            if (!slotsToReplace.Any()) return this;
            var newCards = new Queue<Card>(deck.Draw(slotsToReplace.Count()));
            foreach (var slotToReplace in slotsToReplace)
                slotToReplace.Card = newCards.Dequeue();
            return this;
        }

        public (int Rank, bool HasHand, string Name)[] HandChecks =>
            new[]
            {
                (0, true, "No Hand"),
                (1, IsOnePair, "One Pair"),
                (2, IsTwoPair, "Two Pair"),
                (3, IsThreeOfAKind, "Three of a Kind"),
                (4, IsStraight, "Straight"),
                (5, IsFlush, "Flush"),
                (6, IsFullHouse, "Full House"),
                (7, IsFourOfAKind, "Four of a Kind"),
                (8, IsStraightFlush, "Straight Flush"),
                (9, IsRoyalFlush, "Royal Flush")
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
    }

    class Pot
    {
        private List<IBettableItem> _items = new List<IBettableItem>();

        public IEnumerable<IBettableItem> Items => _items;

        internal void Add(params IBettableItem[] bet) => _items.AddRange(bet);
    }

    interface IBettableItem { }

    interface IPokerPlayer
    {
        IBettableItem ReceiveAnte();
        IBettableItem[] ReceiveBet(int allowedCards);
        void ConfigureKeeps(Hand hand);
        void Lose(Pot pot);
        void Win(Pot pot);
        void Tie();
    }

    class PokerRound
    {
        public Pot Pot { get; private set; }

        public Hand PlayerHand { get; private set; }

        public Hand OpponentHand { get; private set; }

        public void Play(IPokerPlayer player, IPokerPlayer opponent)
        {
            Pot = new Pot();

            Pot.Add(player.ReceiveAnte());
            Pot.Add(opponent.ReceiveAnte());

            var deck = new Deck().Shuffle();
            PlayerHand = new Hand(deck);
            OpponentHand = new Hand(deck);

            Pot.Add(player.ReceiveBet(1));
            var opponentBet = opponent.ReceiveBet(2);
            Pot.Add(opponentBet);
            if (opponentBet.Count() > 1)
                Pot.Add(player.ReceiveBet(1));

            player.ConfigureKeeps(this.PlayerHand);
            PlayerHand.Update(deck);

            opponent.ConfigureKeeps(this.OpponentHand);
            OpponentHand.Update(deck);

            var playerBet = player.ReceiveBet(1);
            Pot.Add(playerBet);
            if (playerBet.Any())
                Pot.Add(opponent.ReceiveBet(1));

            var playerHandValue = PlayerHand.HandChecks.Where(hc => hc.HasHand).OrderByDescending(hc => hc.Rank).FirstOrDefault();
            var opponentHandValue = PlayerHand.HandChecks.Where(hc => hc.HasHand).OrderByDescending(hc => hc.Rank).FirstOrDefault();

            
            if (playerHandValue.Rank < opponentHandValue.Rank)
            {
                player.Lose(Pot);
                opponent.Win(Pot);
            }
            else if (playerHandValue.Rank > opponentHandValue.Rank)
            {
                player.Win(Pot);
                opponent.Lose(Pot);
            }
            else
            {
                player.Tie();
                opponent.Tie();
            }
        }
    }
}
