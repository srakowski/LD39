using System;
using System.Collections.Generic;
using System.Linq;

namespace Ampere
{
    abstract class GamePlayer : IGamePiece
    {
        public Point Pos { get; set; }
        public abstract char Character { get; }
        public BattleRoundEffect BattleRoundEffect { get; }
        public PlayerState PlayerState { get; }
        public BattleCard BaseAttackCard => new BattleCard(GameCards.BaseAttack, this);
        public bool IsDead => PlayerState.IsDead;
        public abstract void ConfigureKeeps(Hand hand);
        public abstract IBettableItem[] ReceiveBet(int requiredCards, BattleCard[] cardsToChooseFrom);

        public GamePlayer(PlayerState state)
        {
            this.BattleRoundEffect = new BattleRoundEffect();
            this.PlayerState = state;
        }
    }

    class GameCard
    {
        public string Name { get; }

        public string Description { get; }

        private Func<GamePlayer, GamePlayer, bool> _applyPositiveEffect;

        private Action<GamePlayer, GamePlayer> _applyNegativeEffect;
        private string v1;
        private string v2;
        private GameCard run;
        private object damageOpponent;

        public GameCard(string name, string description, 
            Func<GamePlayer, GamePlayer, bool> applyPositiveEffect,
            Action<GamePlayer, GamePlayer> applyNegativeEffect)
        {
            Name = name;
            Description = description;
            _applyPositiveEffect = applyPositiveEffect;
            _applyNegativeEffect = applyNegativeEffect;
        }

        public GameCard(string v1, string v2, GameCard run, object damageOpponent)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.run = run;
            this.damageOpponent = damageOpponent;
        }

        /// <returns>true if the battle should continue, allow for run card</returns>
        public bool ApplyPositiveEffect(GamePlayer fromPlayer, GamePlayer toPlayer) =>
            _applyPositiveEffect?.Invoke(fromPlayer, toPlayer) ?? true;

        public void ApplyNegativeEffect(GamePlayer fromPlayer, GamePlayer toPlayer) =>
            _applyNegativeEffect?.Invoke(fromPlayer, toPlayer);
    }

    class BattleCard : IBettableItem
    {
        public GameCard Card { get; }

        public GamePlayer Player { get; }

        public bool Selected { get; set; }

        public BattleCard(GameCard card, GamePlayer player)
        {
            Card = card;
            Player = player;
            Selected = false;
        }

        /// <returns>true if the battle should continue, allow for run card</returns>
        public bool ApplyPositiveEffect(GamePlayer fromPlayer, GamePlayer toPlayer) =>
            Card.ApplyPositiveEffect(fromPlayer, toPlayer);

        public void ApplyNegativeEffect(GamePlayer fromPlayer, GamePlayer toPlayer) =>
            Card.ApplyNegativeEffect(fromPlayer, toPlayer);
    }

    class BattleDeck
    {
        public BattleCard[] Current => _inHand.ToArray();

        private List<BattleCard> _inHand;

        private Queue<BattleCard> _pile;

        private List<BattleCard> _discardedCards;

        public BattleDeck(GamePlayer player)
        {
            _pile = new Queue<BattleCard>(player.PlayerState.CardInventory.OrderBy(c => Rnd.Next).Select(c => new BattleCard(c, player)));
            _discardedCards = new List<BattleCard>();
            _inHand = new List<BattleCard>();
            for (int i = 0; i < 5 && _pile.Any(); i++)
                _inHand.Add(_pile.Dequeue());
        }

        internal void Discard(IEnumerable<BattleCard> battleCards)
        {
            foreach (var card in battleCards)
            {
                if (card.Card.Name == "Run" || card.Card.Name == "Pass")
                    continue;

                _inHand.Remove(card);
                _discardedCards.Add(card);
                if (!_pile.Any())
                    _pile = new Queue<BattleCard>(_discardedCards.OrderBy(c => Rnd.Next));
                if (_pile.Any())
                    _inHand.Add(_pile.Dequeue());
            }
        }
    }

    class BattleRoundEffect
    {
        public int Damage;
        public int Blocked;
        public int Healed;

        public void Apply(PlayerState playerState)
        {
            var powerDelta = Util.Clamp((-Damage) + Blocked, 0, int.MaxValue) + Healed;
            playerState.ModifyPowerLevel(powerDelta);
        }

        public void Reset()
        {
            Damage = 0;
            Blocked = 0;
            Healed = 0;
        }
    }

    class BattlePlayer : IPokerPlayer
    {
        public GamePlayer Player { get; }

        private GamePlayer Opponent { get; }

        private BattleDeck BattleDeck { get; }

        public bool HasRun { get; private set; }

        public bool IsDeadOrRanFromBattle => Player.IsDead || HasRun;

        public BattlePlayer(GamePlayer player, GamePlayer opponent)
        {
            Player = player;
            Opponent = opponent;
            BattleDeck = new BattleDeck(player);
        }

        public void ConfigureKeeps(Hand hand) => Player.ConfigureKeeps(hand);

        public IBettableItem[] ReceiveBet(int allowedCards)
        {
            var bet = Player.ReceiveBet(allowedCards, BattleDeck.Current);
            var battleCards = bet.Cast<BattleCard>();
            BattleDeck.Discard(battleCards);
            return bet;
        }

        public IBettableItem ReceiveAnte() => Player.BaseAttackCard;

        public void Lose(Pot pot)
        {
            var gameCards = pot.Items.Cast<BattleCard>();
            foreach (var gameCard in gameCards)
                gameCard.ApplyNegativeEffect(Opponent, Player);
        }

        public void Tie() { }

        public void Win(Pot pot)
        {
            var gameCards = pot.Items.Cast<BattleCard>();
            foreach (var gameCard in gameCards)
                if (!gameCard.ApplyPositiveEffect(Opponent, Player))
                    HasRun = true;
        }
    }

    class Battle
    {
        public BattlePlayer PlayerBattlePlayer { get; private set; }

        public BattlePlayer OpponentBattlePlayer { get; private set; }

        public PokerRound PokerRound { get; private set; }

        public Battle(GamePlayer p1, GamePlayer p2)
        {
            this.PlayerBattlePlayer = new BattlePlayer(p1, p2);
            this.OpponentBattlePlayer = new BattlePlayer(p2, p1);
        }

        public void Execute()
        {
            do
            {
                PlayerBattlePlayer.Player.BattleRoundEffect.Reset();
                OpponentBattlePlayer.Player.BattleRoundEffect.Reset();
                PokerRound = new PokerRound();
                PokerRound.Play(PlayerBattlePlayer, OpponentBattlePlayer);
                if (!PlayerBattlePlayer.IsDeadOrRanFromBattle && !OpponentBattlePlayer.IsDeadOrRanFromBattle)
                {
                    PlayerBattlePlayer.Player.BattleRoundEffect.Apply(PlayerBattlePlayer.Player.PlayerState);
                    OpponentBattlePlayer.Player.BattleRoundEffect.Apply(OpponentBattlePlayer.Player.PlayerState);
                }
            } while (!PlayerBattlePlayer.IsDeadOrRanFromBattle && !OpponentBattlePlayer.IsDeadOrRanFromBattle);
        }
    }
}
