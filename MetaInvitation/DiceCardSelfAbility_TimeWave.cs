namespace MetaInvitation
{
	public class DiceCardSelfAbility_TimeWave : DiceCardSelfAbilityBase
	{
		public override string[] Keywords
		{
			get
			{
				return new string[] { "ego_Keyword" };
			}
		}

		public override bool IsTargetableSelf()
		{
			return true;
		}

		public override bool IsTargetableAllUnit()
		{
			return true;
		}

		public override bool OnChooseCard(BattleUnitModel owner)
		{
			var timeBuf = owner.bufListDetail.GetActivatedBufList().Find(buf => buf is BattleUnitBuf_Time) as BattleUnitBuf_Time;
			return timeBuf != null && timeBuf.stack >= canUse;
		}

		public override void OnStartBattle()
		{
			var target = card.target;
			if (target != null)
			{
				target.bufListDetail.RemoveBufAll(BufPositiveType.Positive);
				target.bufListDetail.RemoveBufAll(BufPositiveType.Negative);
			}
		}

		internal const int canUse = 350;
	}
}
