namespace MetaInvitation
{
	public class DiceCardAbility_endurance1atk : DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "Endurance_Keyword" };

		public override void OnSucceedAttack()
		{
			owner.bufListDetail.AddKeywordBufByCard(KeywordBuf.Endurance, 1, owner);
		}
	}
}
