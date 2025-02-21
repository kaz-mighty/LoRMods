using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_CarefulPursuit : PassiveAbilityBase
	{
		public override void BeforeRollDice(BattleDiceBehavior behavior)
		{
			if (behavior.TargetDice?.Type == BehaviourType.Standby)
			{
				owner.battleCardResultLog?.SetPassiveAbility(this);
				behavior.ApplyDiceStatBonus(new DiceStatBonus
				{
					power = -1,
				});
				behavior.TargetDice.ApplyDiceStatBonus(new DiceStatBonus
				{
					power = -2,
				});

			}
		}
	}
}
