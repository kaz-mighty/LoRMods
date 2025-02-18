using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	// バフを付与するバフを用意することで、後から生成された敵へも効果を発揮する
	// 死亡時に他の味方へ移動することで、舞台中永続するようにしている
	class BattleUnitBuf_MetaManager : BattleUnitBuf
	{
		public BattleUnitBuf_MetaManager(ManagerActivater activater, ManagerActivater deactivater)
		{
			_activater = activater;
			_deactivater = deactivater;
		}

		public override bool Hide => true;

		public override void Init(BattleUnitModel owner)
		{
			base.Init(owner);
			_activater(_owner);
		}

		public override void OnRoundStartAfter() => _activater(_owner);

		public override void Destroy()
		{
			_deactivater(_owner);
			base.Destroy();
		}

		public override void OnDie()
		{
			if (IsDestroyed())
			{
				return;
			}
			var others = BattleObjectManager.instance.GetAliveList(_owner.faction);
			if (others.Count == 0)
			{
				return;
			}
			var target = RandomUtil.SelectOne(others);
			target.bufListDetail.AddBuf(new BattleUnitBuf_MetaManager(_activater, _deactivater));
		}

		private ManagerActivater _activater;
		private ManagerActivater _deactivater;
	}
}
