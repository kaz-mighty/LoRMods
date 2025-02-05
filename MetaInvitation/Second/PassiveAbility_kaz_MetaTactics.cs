using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;


namespace MetaInvitation.Second
{
	class PassiveAbility_kaz_MetaTactics : PassiveAbilityBase
	{

		public override void OnWaveStart()
		{
			if (owner.faction != Faction.Player)
			{
				return;
			}
			foreach (var cardId in cards)
			{
				owner.personalEgoDetail.AddCard(cardId);
			}
		}

		public override void OnRoundStartAfter()
		{
			if (owner.faction != Faction.Enemy)
			{
				return;
			}
			if (owner.RollSpeedDice().FindAll(x => !x.breaked).Count <= 0 || owner.IsBreakLifeZero())
			{
				return;
			}
			if (_usedEnemy)
			{
				return;
			}
			// TODO: 3～4種類のページを実装したら賢くする

			int usePage = RandomUtil.Range(0, 1);
			switch (usePage)
			{
				case 0:
					new DiceCardSelfAbility_MetaSmoke().Activate(owner);
					break;
				case 1:
					new DiceCardSelfAbility_MetaOverPower().Activate(owner);
					break;
			}
			_usedEnemy = true;

		}

		private bool _usedEnemy;
		private static readonly LorId _vsStandard = new LorId(MetaInvitation.packageId, 150);
		private static readonly LorId _vsSmoke = new LorId(MetaInvitation.packageId, 151);
		private static readonly LorId _vsOverPower = new LorId(MetaInvitation.packageId, 152);
		public static readonly LorId[] cards = {
			_vsSmoke,
			_vsOverPower,
		};
	}
}
