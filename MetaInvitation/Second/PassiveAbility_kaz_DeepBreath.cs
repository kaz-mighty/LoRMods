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
			var num = _canUseSlotNum;
			num -= owner.cardHistory.GetCurrentRoundCardList(Singleton<StageController>.Instance.RoundTurn).Count;
			if (num > 0)
			{
				owner.cardSlotDetail.RecoverPlayPoint(Mathf.Max(num, 2));
			}
		}

		int _canUseSlotNum = 0;
	}
}
