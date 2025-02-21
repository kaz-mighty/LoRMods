namespace MetaInvitation.Second
{
	public class DiceCardSelfAbility_MetaOverPower : DiceCardSelfAbilityBase_Meta
	{
		public override void ManagerActivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
			{
				var buf = target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaOverPower>();
				if (buf != null)
				{
					buf.Mode |= RelativeFactions.Enemy;
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaOverPower(RelativeFactions.Enemy));
			}
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				var buf = target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaOverPower>();
				if (buf != null)
				{
					buf.Mode |= RelativeFactions.Ally;
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaOverPower(RelativeFactions.Ally));
			}
		}

		public override void ManagerDeactivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
			{
				var buf = target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaOverPower>();
				if (buf != null)
				{
					buf.Mode &= ~RelativeFactions.Enemy;
				}
			}
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				var buf = target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaOverPower>();
				if (buf != null)
				{
					buf.Mode &= ~RelativeFactions.Ally;
				}
			}
		}
	}
}
