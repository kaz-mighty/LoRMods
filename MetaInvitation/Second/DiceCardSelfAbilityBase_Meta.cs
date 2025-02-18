using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	abstract class DiceCardSelfAbilityBase_Meta : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "kaz_MetaKeyword" };

		public override bool IsTargetableSelf() => true;

		public override bool IsTargetableAllUnit() => true;

		public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
		{
			Activate(unit);

			// 並び順を固定したいため、いったん消して次ラウンドに追加する
			var passive = unit.passiveDetail.PassiveList.Find(x => x is PassiveAbility_kaz_MetaTactics) as PassiveAbility_kaz_MetaTactics;
			passive?.OnPassiveCardUsed();
			foreach (var cardId in PassiveAbility_kaz_MetaTactics.cardIds)
			{
				unit.personalEgoDetail.RemoveCard(cardId);
			}
		}

		public void Activate(BattleUnitModel unit)
		{
			foreach (var ally in BattleObjectManager.instance.GetAliveList(unit.faction))
			{
				var buf = ally.bufListDetail.GetActivatedBufList().Find(x => x is BattleUnitBuf_MetaManager && !x.IsDestroyed());
				if (buf != null)
				{
					buf.Destroy();
				}
			}
			unit.bufListDetail.AddBuf(new BattleUnitBuf_MetaManager(ManagerActivate, ManagerDeactivate));
		}

		// 付与時とラウンド開始時に実行される
		public abstract void ManagerActivate(BattleUnitModel owner);

		// マネージャーのDestroy時に実行される
		public abstract void ManagerDeactivate(BattleUnitModel owner);

	}

	delegate void ManagerActivater(BattleUnitModel owner);
}
