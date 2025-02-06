﻿namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_PowerDown2TargetHighlander : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "OnlyOne_Keyword" };
		public override void OnStartParrying()
		{
			var target = card.target;
			if (target == null || !target.allyCardDetail.IsHighlander())
			{
				return;
			}
			target.currentDiceAction?.ApplyDiceStatBonus(DiceMatch.AllDice, new DiceStatBonus
			{
				power = -2,
			});
		}
	}
}
