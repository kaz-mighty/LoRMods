namespace MetaInvitation.Second
{
	public class DiceCardAbility_kaz_disarm1pl : DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "Disarm_Keyword" };

		public override void OnLoseParrying()
		{
			card.target?.bufListDetail.AddKeywordBufByCard(KeywordBuf.Disarm, 1, owner);
		}
	}
}
