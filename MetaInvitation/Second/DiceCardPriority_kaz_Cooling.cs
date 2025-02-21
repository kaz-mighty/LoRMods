namespace MetaInvitation.Second
{
	public class DiceCardPriority_kaz_Cooling : DiceCardPriorityBase
	{
		public override int GetPriorityBonus(BattleUnitModel owner)
		{
			var maxLight = owner.MaxPlayPoint;
			var light = owner.PlayPoint - owner.cardSlotDetail.ReservedPlayPoint;
			int priority = -2;
			if (maxLight > light)
			{
				priority += 1;
			}
			if ((owner.bufListDetail.GetActivatedBuf(KeywordBuf.Burn)?.stack ?? 0) >= 3)
			{
				priority += 2;
			}
			return priority;
		}
	}
}
