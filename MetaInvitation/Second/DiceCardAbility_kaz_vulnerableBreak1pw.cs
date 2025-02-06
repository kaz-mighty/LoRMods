namespace MetaInvitation.Second
{
	class DiceCardAbility_kaz_vulnerableBreak1pw : DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "Vulnerable_break_Keyword" };

		public override void OnWinParrying()
		{
			card.target?.bufListDetail.AddKeywordBufByCard(KeywordBuf.Vulnerable_break, 1, owner);
		}
	}
}
