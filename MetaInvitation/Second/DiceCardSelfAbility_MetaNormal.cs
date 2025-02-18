using System;
using UnityEngine;

namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_MetaNormal : DiceCardSelfAbilityBase_Meta
	{
		public override void ManagerActivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				if (target.bufListDetail.HasBuf<BattleUnitBuf_MetaNormal>())
				{
					continue;
				}
				KeepRatio(target, () => target.bufListDetail.AddBuf(new BattleUnitBuf_MetaNormal()));
			}
		}

		public override void ManagerDeactivate(BattleUnitModel owner)
		{
			foreach (var target in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				var buf = target.bufListDetail.GetActivatedBufList().Find(x => x is BattleUnitBuf_MetaNormal && !x.IsDestroyed());
				if (buf != null)
				{
					KeepRatio(target, () => buf.Destroy());
				}
			}
		}

		private void KeepRatio(BattleUnitModel target, Action action)
		{
			float maxHpBefore = target.MaxHp;
			float hpBefore = target.hp;
			float maxBreakBefore = target.breakDetail.GetDefaultBreakGauge();
			float breakBefore = target.breakDetail.breakGauge;

			action();

			float maxHpAfter = target.MaxHp;
			float maxBreakAfter = target.breakDetail.GetDefaultBreakGauge();
			target.SetHp(Mathf.Max(1, Mathf.RoundToInt(hpBefore * maxHpAfter / maxHpBefore)));
			if (breakBefore > 0)
			{
				target.breakDetail.breakGauge = Mathf.Max(1, Mathf.RoundToInt(breakBefore * maxBreakAfter / maxBreakBefore));
			}
		}
	}
}
