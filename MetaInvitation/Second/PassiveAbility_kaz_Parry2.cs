using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class PassiveAbility_kaz_Parry2 : PassiveAbility_kaz_Parry1
	{
		public override int GetDamageReduction(BattleDiceBehavior behavior)
		{
			var range = behavior.card.card.GetSpec().Ranged;
			if (range == CardRange.FarArea)
			{
				return GetDefenderDiceValueForFarArea();
			}
			else if (range == CardRange.FarAreaEach)
			{
				return GetDefenderDiceValueForFarAreaEach();
			}
			return 0;
		}

		public override int GetBreakDamageReduction(BattleDiceBehavior behavior)
		{
			return GetDamageReduction(behavior);
		}
	}
}
