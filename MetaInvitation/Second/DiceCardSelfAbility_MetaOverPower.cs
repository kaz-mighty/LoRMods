using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_MetaOverPower : DiceCardSelfAbilityBase_Meta
	{
		public override void ManagerActivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
			{
				var buf = target.bufListDetail.GetActivatedBufList().Find(x => x is BattleUnitBuf_MetaOverPower) as BattleUnitBuf_MetaOverPower;
				if (buf != null)
				{
					buf.Mode |= RelativeFactions.Enemy;
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaOverPower(RelativeFactions.Enemy));
			}
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				var buf = target.bufListDetail.GetActivatedBufList().Find(x => x is BattleUnitBuf_MetaOverPower) as BattleUnitBuf_MetaOverPower;
				if (buf != null)
				{
					buf.Mode |= RelativeFactions.Ally;
					continue;
				}
				target.bufListDetail.AddBuf(new BattleUnitBuf_MetaOverPower(RelativeFactions.Ally));
			}
		}
	}


}
