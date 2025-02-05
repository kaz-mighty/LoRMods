﻿using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_MetaSmoke : DiceCardSelfAbility_MetaBase
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
	}
}
