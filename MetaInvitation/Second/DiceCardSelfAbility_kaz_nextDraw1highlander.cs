namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_nextDraw1Highlander : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "OnlyOne_Keyword", "DrawCard_Keyword" };

		public override void OnUseCard()
		{
			if (!owner.allyCardDetail.IsHighlander()) { return; }
			owner.bufListDetail.AddBuf(new BattleUnitBuf_kaz_NextRoundDraw { stack = 1 });
		}
	}
}
