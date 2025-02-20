namespace MetaInvitation.Second
{
	class BattleUnitBuf_MetaDamageRateAlly : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_MetaDamageRateAlly";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";

		public override void Init(BattleUnitModel owner)
		{
			base.Init(owner);
			stack = 0;
		}
	}
}
