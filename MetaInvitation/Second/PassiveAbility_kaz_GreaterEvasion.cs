using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class PassiveAbility_kaz_GreaterEvasion : PassiveAbilityBase
	{
		public override void BeforeRollDice(BattleDiceBehavior behavior)
		{
			if (behavior.Detail == BehaviourDetail.Evasion)
			{
				owner.battleCardResultLog?.SetPassiveAbility(this);
				behavior.ApplyDiceStatBonus(new DiceStatBonus
				{
					power = 1,
					min = 3,
				});
				behavior.forbiddenBonusDice = true;
			}
		}
	}
}
