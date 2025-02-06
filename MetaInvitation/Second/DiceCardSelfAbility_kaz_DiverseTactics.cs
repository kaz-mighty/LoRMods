using HarmonyLib;

namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_DiverseTactics : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] {
			"OnlyOne_Keyword",
			MetaInvitation.packageId + "_DiverseTactics_Keyword"
		};

		public override void OnUseCard()
		{
			if (!owner.allyCardDetail.IsHighlander()) { return; }
			var card = owner.allyCardDetail.GetRandomCardInHand(
				x => x.GetCost() >= 1
				&& (int)AccessTools.Field(typeof(BattleDiceCardModel), "_costAdder").GetValue(x) >= 0
			);
			card?.AddCost(-1);
		}
	}
}
