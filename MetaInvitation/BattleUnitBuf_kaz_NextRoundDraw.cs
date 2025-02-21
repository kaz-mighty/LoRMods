namespace MetaInvitation
{
	public class BattleUnitBuf_kaz_NextRoundDraw : BattleUnitBuf
	{
		public override void OnRoundEndTheLast()
		{
			_owner.allyCardDetail.DrawCards(stack);
			Destroy();
		}
	}
}
