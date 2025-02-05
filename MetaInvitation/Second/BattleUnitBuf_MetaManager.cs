using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class BattleUnitBuf_MetaManager : BattleUnitBuf
	{
		public BattleUnitBuf_MetaManager(ManagerActivater activater)
		{
			_activater = activater;
		}

		public override bool Hide => true;

		public override void Init(BattleUnitModel owner)
		{
			base.Init(owner);
			_activater(_owner);
		}

		public override void OnRoundStartAfter() => _activater(_owner);

		public override void OnDie()
		{
			var others = BattleObjectManager.instance.GetAliveList(_owner.faction);
			if (others.Count == 0)
			{
				return;
			}
			var target = RandomUtil.SelectOne(others);
			target.bufListDetail.AddBuf(new BattleUnitBuf_MetaManager(_activater));
		}

		private ManagerActivater _activater;
	}
}
