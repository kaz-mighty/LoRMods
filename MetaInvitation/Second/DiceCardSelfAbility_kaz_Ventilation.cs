namespace MetaInvitation.Second
{
	public class DiceCardSelfAbility_kaz_Ventilation : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "Energy_Keyword", "Smoke_Keyword" };

		public override void OnUseCard()
		{
			owner.cardSlotDetail.RecoverPlayPointByCard(1);
			var buf = owner.bufListDetail.GetActivatedBuf(KeywordBuf.Smoke) as BattleUnitBuf_smoke;
			if (buf != null)
			{
				buf.stack -= 3;
				if (buf.stack <= 0)
				{
					buf.Destroy();
				}
			}
			buf = card.target?.bufListDetail.GetActivatedBuf(KeywordBuf.Smoke) as BattleUnitBuf_smoke;
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
