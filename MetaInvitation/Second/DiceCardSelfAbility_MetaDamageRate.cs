namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_MetaDamageRate : DiceCardSelfAbilityBase_Meta
	{
		public override void ManagerActivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
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
	}
}
