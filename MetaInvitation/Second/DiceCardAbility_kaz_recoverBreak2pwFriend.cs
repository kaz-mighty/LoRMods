namespace MetaInvitation.Second
{
	public class DiceCardAbility_kaz_recoverBreak2pwFriend : DiceCardAbilityBase
	{
		public override string[] Keywords => new string[] { "RecoverBreak_Keyword" };

		public override void OnWinParrying()
		{
			foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				unit.breakDetail.RecoverBreak(2);
			}
		}
	}
}
