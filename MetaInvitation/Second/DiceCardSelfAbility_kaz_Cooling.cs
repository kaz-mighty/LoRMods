namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_Cooling : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "Burn_Keyword", "Energy_Keyword", "Recover_Keyword", "RecoverBreak_Keyword" };

		public override void OnUseCard()
		{
			owner.cardSlotDetail.RecoverPlayPointByCard(2);
			owner.breakDetail.RecoverBreak(3);
			var buf = owner.bufListDetail.GetActivatedBuf(KeywordBuf.Burn) as BattleUnitBuf_burn;
			if (buf != null)
			{
				buf.stack -= 3;
				if (buf.stack <= 0)
				{
					buf.Destroy();
				}
			}
		}
	}
}
