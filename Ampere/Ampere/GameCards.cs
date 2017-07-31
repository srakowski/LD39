using System;

namespace Ampere
{
    static class GameCards
    {
        public static GameCard BaseAttack => new GameCard(
            "Base Damage Card",
            "This card is played once automatically every round. The loser of the round will take N points of damage where N is the value of the opponent's base attack damage.",
            null,
            ApplyBaseDamage);

        public static GameCard Run => new GameCard(
            "Run",
            "Play this card to try to leave a battle. If you win the round with it in play then the battle will be exited. If you lose the round with it in play then you will take an additional point of damage.",
            ApplyRun,
            ApplyDamageToFromFunc(1));

        public static GameCard Pass => new GameCard(
            "Pass",
            "Play this card when you do not wish to augment the battle.",
            null,
            null);

        // Positive Effects

        private static bool ApplyRun(GamePlayer fromPlayer, GamePlayer toPlayer) => false;

        // Negative Effects

        private static void ApplyBaseDamage(GamePlayer fromPlayer, GamePlayer toPlayer) =>
            toPlayer.BattleRoundEffect.Damage += fromPlayer.PlayerState.BaseDamage.Value;

        private static Action<GamePlayer, GamePlayer> ApplyDamageToFromFunc(int amount) =>
            (fromPlayer, toPlayer) => fromPlayer.BattleRoundEffect.Damage += amount;
    }
}
