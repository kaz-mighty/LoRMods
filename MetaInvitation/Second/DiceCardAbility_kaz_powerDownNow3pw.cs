using HarmonyLib;

namespace MetaInvitation.Second
{
	class DiceCardAbility_kaz_powerDownNow3pw: DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "ThisDice_Keyword" };

		public override void OnWinParrying()
		{
			var firstStatBonus = AccessTools.Field(typeof(BattleDiceBehavior), "_firstStatBonus").GetValue(behavior) as DiceStatBonus;
			if (firstStatBonus != null)
			{
				firstStatBonus.power -= 3;
			}
		}
	}
}
