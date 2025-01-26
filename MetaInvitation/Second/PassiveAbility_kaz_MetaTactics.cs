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
			foreach (var cardId in _cards)
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
			// TODO: 各ページの仕様が決まってから
		}


		private static readonly LorId _vsStandard = new LorId(MetaInvitation.packageId, 150);
		private static readonly LorId _vsSmoke = new LorId(MetaInvitation.packageId, 151);
		private static readonly LorId[] _cards = {
			_vsStandard,
			_vsSmoke,
		};
	}
}
