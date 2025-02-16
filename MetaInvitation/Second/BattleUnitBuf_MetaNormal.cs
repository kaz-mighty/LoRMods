namespace MetaInvitation.Second
{
	class BattleUnitBuf_MetaNormal : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_MetaNormal";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";

		public override StatBonus GetStatBonus()
		{
			return new StatBonus
			{
				hpAdder = 10,
				breakGageAdder = 5
			};
		}
	}
}
