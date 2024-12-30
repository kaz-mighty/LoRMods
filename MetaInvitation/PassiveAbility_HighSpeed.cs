namespace MetaInvitation
{
	class PassiveAbility_HighSpeed: PassiveAbilityBase
	{
		public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
		{
			int mySpeed = curCard.speedDiceResultValue;
			int targetSlot = curCard.targetSlotOrder;
			int targetSpeed = curCard.target.GetSpeedDiceResult(targetSlot).value;
			if (mySpeed > targetSpeed)
			{
				curCard.ApplyDiceStatBonus(DiceMatch.AllDice, new DiceStatBonus { power = 1 });
			}
		}
	}
}
