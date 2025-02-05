using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{

	abstract class DiceCardSelfAbility_MetaBase : DiceCardSelfAbilityBase
	{
		public override bool IsTargetableSelf() => true;

		public override bool IsTargetableAllUnit() => true;

		public override void OnUseInstance(BattleUnitModel unit, BattleDiceCardModel self, BattleUnitModel targetUnit)
		{
			Activate(unit);
			foreach (var cardId in PassiveAbility_kaz_MetaTactics.cards)
			{
				unit.personalEgoDetail.RemoveCard(cardId);
			}
		}

		public void Activate(BattleUnitModel unit)
		{
			unit.bufListDetail.AddBuf(new BattleUnitBuf_MetaManager(ManagerActivate));
		}

		public abstract void ManagerActivate(BattleUnitModel owner);


	}

	delegate void ManagerActivater(BattleUnitModel owner);
}
