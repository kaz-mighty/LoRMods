using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class PassiveAbility_kaz_DeepBreath : PassiveAbilityBase
	{
		public override void OnStartBattle()
		{
			_canUseSlotNum = 0;
			if (!owner.IsBreakLifeZero())
			{
				foreach (var dice in owner.speedDiceResult)
				{
					if (!dice.breaked)
					{
						_canUseSlotNum += 1;
					}
				}
			}
		}

		public override void OnRoundEnd()
		{
			var useCardNum = owner.cardHistory.GetCurrentRoundCardList(Singleton<StageController>.Instance.RoundTurn).Count;
			var num = _canUseSlotNum - useCardNum;
			// Debug.Log(String.Format("Character: {0}, RecoverLight: {1} - {2}", owner.UnitData.unitData.name, _canUseSlotNum, useCardNum));
			if (num > 0)
			{
				owner.cardSlotDetail.RecoverPlayPoint(Mathf.Min(num, 2));
			}
		}

		int _canUseSlotNum = 0;
	}
}
