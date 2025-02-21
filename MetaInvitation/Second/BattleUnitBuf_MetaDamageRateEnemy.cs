namespace MetaInvitation.Second
{
	public class BattleUnitBuf_MetaDamageRateEnemy : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_MetaDamageRateEnemy";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";

		public override void Init(BattleUnitModel owner)
		{
			base.Init(owner);
			stack = 0;
		}
	}
}
