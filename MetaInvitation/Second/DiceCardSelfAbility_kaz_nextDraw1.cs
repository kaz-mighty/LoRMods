namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_nextDraw1 : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "DrawCard_Keyword" };

		public override void OnUseCard()
		{
			owner.bufListDetail.AddBuf(new BattleUnitBuf_kaz_NextRoundDraw { stack = 1 });
		}
	}
}
