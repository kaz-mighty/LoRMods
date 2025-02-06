namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_BuyTime : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] { "Burn_Keyword", "Bleeding_Keyword", "bstart_Keyword" };

		public override void OnStartBattle()
		{
			foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				DecreaseStack(unit.bufListDetail.GetActivatedBuf(KeywordBuf.Burn));
				DecreaseStack(unit.bufListDetail.GetActivatedBuf(KeywordBuf.Bleeding));
			}
		}

		private static void DecreaseStack(BattleUnitBuf buf)
		{
			if (buf == null) { return; }
			buf.stack = buf.stack * 2 / 3;
			if (buf.stack <= 0)
			{
				buf.Destroy();
			}
		}
	}
}
