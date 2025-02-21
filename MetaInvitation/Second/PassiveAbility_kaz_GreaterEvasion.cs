using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_GreaterEvasion : PassiveAbilityBase
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
