namespace MetaInvitation.Second
{
	class DiceCardAbility_kaz_vulnerable1pl : DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "Vulnerable_Keyword" };

		public override void OnLoseParrying()
		{
			card.target?.bufListDetail.AddKeywordBufByCard(KeywordBuf.Vulnerable, 1, owner);
		}
	}
}
