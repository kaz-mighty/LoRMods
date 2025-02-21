namespace MetaInvitation.Second
{
	public class BattleUnitBuf_MetaSmoke : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_MetaSmoke";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";

		public override void Init(BattleUnitModel owner)
		{
			base.Init(owner);
			stack = 0;
		}
	}
}
