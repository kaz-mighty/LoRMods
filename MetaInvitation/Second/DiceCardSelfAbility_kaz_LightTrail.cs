namespace MetaInvitation.Second
{
	public class DiceCardSelfAbility_kaz_LightTrail : DiceCardSelfAbility_bandFinalBase
	{
		public override string[] Keywords => new string[] { "Energy_Keyword" };

		public override void OnUseCard()
		{
			ExhaustAndReturn();
			owner.cardSlotDetail.RecoverPlayPointByCard(5);
		}
	}
}
