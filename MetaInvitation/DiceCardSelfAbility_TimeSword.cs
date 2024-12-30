using UnityEngine;

namespace MetaInvitation
{
	class DiceCardSelfAbility_TimeSword: DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "DrawCard_Keyword" };

		public override void OnUseCard()
		{
			owner.allyCardDetail.DrawCards(1);

			var timeBuf = owner.bufListDetail.GetActivatedBufList().Find(buf => buf is BattleUnitBuf_Time) as BattleUnitBuf_Time;
			if (timeBuf == null || timeBuf.stack < 50)
			{
				return;
			}
			var x = Mathf.Min(timeBuf.stack / 50, 4);
			card.ApplyDiceStatBonus(DiceMatch.AllDice, new DiceStatBonus { min = x });
		}
	}
}
