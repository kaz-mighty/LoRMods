namespace MetaInvitation.Second
{
	class DiceCardAbility_kaz_weak1pl : DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "Weak_Keyword" };

		public override void OnLoseParrying()
		{
			card.target?.bufListDetail.AddKeywordBufByCard(KeywordBuf.Weak, 1, owner);
		}
	}
}
