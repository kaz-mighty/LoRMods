namespace MetaInvitation.Second
{
	public class DiceCardSelfAbility_MetaSmoke : DiceCardSelfAbilityBase_Meta
	{
		public override void ManagerActivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
			{
				if (target.bufListDetail.HasBuf<BattleUnitBuf_MetaSmoke>())
				{
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaSmoke());
			}
		}

		public override void ManagerDeactivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
			{
				target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaSmoke>()?.Destroy();
			}
		}
	}
}
