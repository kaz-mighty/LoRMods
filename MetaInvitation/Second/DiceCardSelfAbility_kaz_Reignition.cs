using UnityEngine;

namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_Reignition : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "Energy_Keyword" };

		public override void OnEnterCardPhase(BattleUnitModel unit, BattleDiceCardModel self)
		{
			var memory = new PlayPointMemory { point = unit.PlayPoint };
			self.AddBuf(memory);
		}

		public override int GetCostLast(BattleUnitModel unit, BattleDiceCardModel self, int oldCost)
		{
			var buf = self.GetBufList().Find(x => x is PlayPointMemory) as PlayPointMemory;
			if (buf == null)
			{
				return oldCost;
			}
			return Mathf.Min(oldCost, buf.point);
		}

		public override void OnUseCard()
		{
			owner.cardSlotDetail.RecoverPlayPointByCard(owner.cardSlotDetail.GetMaxPlayPoint());
		}

		class PlayPointMemory : BattleDiceCardBuf
		{
			public int point;

			public override void OnRoundEnd()
			{
				Destroy();
			}
		}
	}
}
