namespace MetaInvitation.Second
{
	class PassiveAbility_kaz_Target25 : PassiveAbilityBase
	{
		public override void OnUseCard(BattlePlayingCardDataInUnitModel curCard)
		{
			if (curCard.target != null && curCard.target.faction != owner.faction)
			{
				_nextTarget = curCard.target;
			}
		}

		public override void OnRoundEnd()
		{
			if (_nextTarget != null && !_nextTarget.IsDead())
			{
				foreach (BattleUnitModel battleUnitModel in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
				{
					battleUnitModel.bufListDetail.RemoveBufAll(KeywordBuf.NicolaiTarget);
				}
				_nextTarget.bufListDetail.AddBuf(new BattleUnitBuf_Target25());
			}
		}

		private BattleUnitModel _nextTarget;

		class BattleUnitBuf_Target25 : PassiveAbility_250223.BattleUnitBuf_target
		{
			protected override string keywordId => "kaz_Target25";
			protected override string keywordIconId => base.keywordId;

			public override int GetDamageIncreaseRate() => 25;
			public override int GetBreakDamageIncreaseRate() => 25;
		}
	}
}
