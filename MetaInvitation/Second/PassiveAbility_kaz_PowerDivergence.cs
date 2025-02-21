using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_PowerDivergence : PassiveAbilityBase
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
			// Since the display must be toggled on and off,
			// the stack must be manipulated using properties.

			protected override string keywordId => MetaInvitation.packageId + "_PowerDivergence";

			public override string bufActivatedText
			{
				get
				{
					return Singleton<BattleEffectTextsXmlList>.Instance.GetEffectTextDesc(keywordId, stack, remainTurn);
				}
			}

			public int Stack
			{
				get => stack;
				set
				{
					stack = value;
					if (stack == 0)
					{
						hide = true;
						AccessTools.Field(typeof(BattleUnitBuf), "_bufIcon").SetValue(this, null);
					}
					else
					{
						hide = false;
						AccessTools.Field(typeof(BattleUnitBuf), "_bufIcon").SetValue(this, _storedBufIcon);
					}
				}
			}

			public override void Init(BattleUnitModel owner)
			{
				base.Init(owner);
				_storedBufIcon = GetBufIcon();  // Load icons before hiding
				Stack = 0;
			}

			public override void OnUseCard(BattlePlayingCardDataInUnitModel card)
			{
				if (Stack <= 0)
				{
					return;
				}
				var range = card.card.GetSpec().Ranged;
				if (range == CardRange.FarArea)
				{
					card.AddDiceAdder(DiceMatch.AllDice, -Stack * 2);
				}
				else if (range == CardRange.FarAreaEach)
				{
					card.AddDiceAdder(DiceMatch.AllDice, -Stack);
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
						Stack += 1;
					}
				}
			}

			public override void OnRoundEnd()
			{
				if (remainTurn > 0 && Stack > 0)
				{
					remainTurn -= 1;
					if (remainTurn == 0)
					{
						Stack -= 1;
						remainTurn = decreaseTurn;
					}
				}
			}

			private int remainTurn = decreaseTurn;
			private const int decreaseTurn = 4;
			private Sprite _storedBufIcon;
		}
	}
}
