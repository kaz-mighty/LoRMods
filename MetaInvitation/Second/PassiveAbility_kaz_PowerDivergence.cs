using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class PassiveAbility_kaz_PowerDivergence : PassiveAbilityBase
	{
		public override void OnRoundStart()
		{
			foreach (var unit in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player))
			{
				if (!unit.bufListDetail.HasBuf<BattleUnitBuf_kaz_PowerDivergence>())
				{
					unit.bufListDetail.AddBuf(new BattleUnitBuf_kaz_PowerDivergence());
				}
			}
		}

		class BattleUnitBuf_kaz_PowerDivergence : BattleUnitBuf
		{
			protected override string keywordId => MetaInvitation.packageId + "_PowerDivergence";

			public override string bufActivatedText
			{
				get
				{
					return Singleton<BattleEffectTextsXmlList>.Instance.GetEffectTextDesc(keywordId, stack, remainTurn);
				}
			}

			public override bool Hide
			{
				get
				{
					return stack > 0;
				}
			}

			public override void OnUseCard(BattlePlayingCardDataInUnitModel card)
			{
				if (stack <= 0)
				{
					return;
				}
				var range = card.card.GetSpec().Ranged;
				if (range == CardRange.FarArea)
				{
					card.AddDiceAdder(DiceMatch.AllDice, -stack * 2);
				}
				else if (range == CardRange.FarAreaEach)
				{
					card.AddDiceAdder(DiceMatch.AllDice, -stack);
				}
			}

			public override void OnEndBattle(BattlePlayingCardDataInUnitModel curCard)
			{
				var range = curCard.card.GetSpec().Ranged;
				if (range == CardRange.FarArea || range == CardRange.FarAreaEach)
				{
					bool flag = false;
					if (curCard.target.passiveDetail.HasPassive<PassiveAbility_kaz_PowerDivergence>())
					{
						flag = true;
					}
					else if (curCard.subTargets.Find(target => target.target.passiveDetail.HasPassive<PassiveAbility_kaz_PowerDivergence>()) != null)
					{
						flag = true;
					}
					if (flag)
					{
						stack += 1;
					}
				}
			}

			public override void OnRoundEnd()
			{
				if (remainTurn > 0 && stack > 0)
				{
					remainTurn -= 1;
					if (remainTurn == 0)
					{
						stack -= 1;
						remainTurn = decreaseTurn;
					}
				}
			}

			private int remainTurn = decreaseTurn;
			private const int decreaseTurn = 4;
		}
	}
}
