using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_Parry1 : PassiveAbilityBase
	{
		public override int GetDamageReduction(BattleDiceBehavior behavior)
		{
			var range = behavior.card.card.GetSpec().Ranged;
			if (range == CardRange.FarArea)
			{
				return GetDefenderDiceValueForFarArea() / 2;
			}
			else if (range == CardRange.FarAreaEach)
			{
				return GetDefenderDiceValueForFarAreaEach() / 2;
			}
			return 0;
		}

		public override int GetBreakDamageReduction(BattleDiceBehavior behavior)
		{
			return GetDamageReduction(behavior);
		}

		public int GetDefenderDiceValueForFarArea()
		{
			var list = owner.battleCardResultLog.playingCard?.GetDiceBehaviorList();
			if (list == null)
			{
				return 0;
			}
			int sum = 0;
			foreach (var x in list)
			{
				sum += x.DiceResultValue;
			}
			return sum;
		}

		public int GetDefenderDiceValueForFarAreaEach()
		{
			return owner.battleCardResultLog.playingCard?.currentBehavior?.DiceResultValue ?? 0;
		}
	}
}
