namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_ActingCommander : PassiveAbilityBase
	{
		public override void OnRoundStartAfter()
		{
			foreach (var unitModel in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				var bufList = unitModel.bufListDetail;
				var buf1 = bufList.GetActivatedBuf(KeywordBuf.Strength);
				var buf2 = bufList.GetActivatedBuf(KeywordBuf.Endurance);
				//var buf3 = bufList.GetActivatedBuf(KeywordBuf.Quickness);
				if (buf1 == null || buf1.stack == 0 || RandomUtil.valueForProb < _prob)
				{
					bufList.AddKeywordBufThisRoundByEtc(KeywordBuf.Strength, 1, owner);
				}
				if (buf2 == null || buf2.stack == 0 || RandomUtil.valueForProb < _prob)
				{
					bufList.AddKeywordBufThisRoundByEtc(KeywordBuf.Endurance, 1, owner);
				}
				//if (buf3 == null || buf3.stack == 0 || RandomUtil.valueForProb < _prob)
				//{
				//	bufList.AddKeywordBufThisRoundByEtc(KeywordBuf.Quickness, 1, owner);
				//}
			}
		}

		private const float _prob = 0.4f;
	}
}
