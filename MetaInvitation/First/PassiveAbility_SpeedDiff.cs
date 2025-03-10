using UnityEngine;

namespace MetaInvitation.First
{
	public class PassiveAbility_SpeedDiff : PassiveAbilityBase
	{
		public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
		{
			int mySpeed = curCard.speedDiceResultValue;
			int targetSlot = curCard.targetSlotOrder;
			int targetSpeed = curCard.target.GetSpeedDiceResult(targetSlot).value;
			if (mySpeed > targetSpeed)
			{
				int num = Mathf.Min(mySpeed - targetSpeed, 5);
				curCard.ApplyDiceStatBonus(DiceMatch.AllDice, new DiceStatBonus { max = num });
			}
		}
	}
}
