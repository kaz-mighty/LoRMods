using UnityEngine;

namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_DeepBreath : PassiveAbilityBase
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
			var useCardNum = owner.cardHistory.GetCurrentRoundCardList(StageController.Instance.RoundTurn).Count;
			var num = _canUseSlotNum - useCardNum;
			// Debug.Log($"Character: {owner.UnitData.unitData.name}, RecoverLight: {_canUseSlotNum} - {useCardNum}");
			if (num > 0)
			{
				owner.cardSlotDetail.RecoverPlayPoint(Mathf.Min(num, 2));
			}
		}

		int _canUseSlotNum = 0;
	}
}
