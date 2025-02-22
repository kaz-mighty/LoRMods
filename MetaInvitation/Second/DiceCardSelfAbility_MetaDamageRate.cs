namespace MetaInvitation.Second
{
	public class DiceCardSelfAbility_MetaDamageRate : DiceCardSelfAbilityBase_Meta
	{
		public override void ManagerActivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction.GetOther()))
			{
				if (target.bufListDetail.HasBuf<BattleUnitBuf_MetaDamageRateEnemy>())
				{
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaDamageRateEnemy());
			}

			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				if (target.bufListDetail.HasBuf<BattleUnitBuf_MetaDamageRateAlly>())
				{
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaDamageRateAlly());
			}
		}

		public override void ManagerDeactivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction.GetOther()))
			{
				target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaDamageRateEnemy>()?.Destroy();
			}
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				target.bufListDetail.GetActivatedBuf<BattleUnitBuf_MetaDamageRateAlly>()?.Destroy();
			}
		}
	}
}
