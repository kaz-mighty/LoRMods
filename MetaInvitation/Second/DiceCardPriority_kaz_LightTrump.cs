namespace MetaInvitation.Second
{
	class DiceCardPriority_kaz_LightTrump : DiceCardPriorityBase
	{
		public override int GetPriorityBonus(BattleUnitModel owner)
		{
			var maxLight = owner.MaxPlayPoint;
			var light = owner.PlayPoint - owner.cardSlotDetail.ReservedPlayPoint;
			if (maxLight - light >= 5)
			{
				return 2;
			}
			else if (maxLight - light >= 2)
			{
				return -2;
			}
			return -10;
		}
	}
}