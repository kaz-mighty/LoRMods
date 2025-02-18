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
			// マルチデッキ対応 -> 1ターン目は使用させない事で対応
			if (StageController.Instance.RoundTurn <= 1)
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

			var priorityList = new List<Tuple<int, float, float>>
			{
				Tuple.Create(0, CalcPriorityVsNormal(), RandomUtil.RangeFloat(0f, Uncertainty)),
				Tuple.Create(1, CalcPriorityVsSmoke(), RandomUtil.RangeFloat(0f, Uncertainty)),
				Tuple.Create(2, CalcPriorityVsOverPower(), RandomUtil.RangeFloat(0f, Uncertainty)),
				Tuple.Create(3, CalcPriorityVsDamageRate(), RandomUtil.RangeFloat(0f, Uncertainty)),
			};
			var logs = new List<string> { MetaInvitation.packageId + " MetaTactics:" };
			foreach (var tuple in priorityList)
			{
				logs.Add($"{tuple.Item1}, {tuple.Item2:0.00}, {tuple.Item3:0.00}");
			}
			Debug.Log(string.Join("\n", logs));
			priorityList.Sort((x, y) => (y.Item2 + y.Item3).CompareTo(x.Item2 + x.Item3));
			switch (priorityList[0].Item1)
			{
				case 0:
					new DiceCardSelfAbility_MetaNormal().Activate(owner);
					break;
				case 1:
					new DiceCardSelfAbility_MetaSmoke().Activate(owner);
					break;
				case 2:
					new DiceCardSelfAbility_MetaOverPower().Activate(owner);
					break;
				case 3:
					new DiceCardSelfAbility_MetaDamageRate().Activate(owner);
					break;
			}
			_usedEnemy = true;

		}

		private float CalcPriorityVsNormal() => 1.1f;

		private float CalcPriorityVsSmoke()
		{
			var priority = 0.0f;
			var enemyUnits = BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player);
			foreach (var unit in enemyUnits)
			{
				var smoke = 0;
				var deck = unit.allyCardDetail.GetAllDeck();
				foreach (var card in deck)
				{
					if (GetKeywords(card).Contains("Smoke_Keyword")) { smoke++; }
				}
				priority += 2f * smoke / deck.Count;
				if (unit.passiveDetail.HasPassive<PassiveAbility_240026>())
				{
					priority += 1f;
				}
			}
			return priority / enemyUnits.Count;
		}

		private float CalcPriorityVsOverPower()
		{
			var priority = 0.0f;
			var enemyUnits = BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player);
			foreach (var unit in enemyUnits)
			{
				var deckPriority1 = 0f;
				var deckPriority2 = 0f;
				var deckPriority3 = 0f;
				var deck = unit.allyCardDetail.GetAllDeck();
				foreach (var card in deck)
				{
					var keywords = GetKeywords(card);
					if (keywords.Contains("Strength_Keyword")) { deckPriority1 += 2f; }
					if (keywords.Contains("Endurance_Keyword")) { deckPriority1 += 1.5f; }
					if (keywords.Contains("Weak_keyword")) { deckPriority2 += 2f; }
					if (keywords.Contains("Disarm_Keyword")) { deckPriority2 += 1.5f; }
					if (card.XmlData.Script == "endurance2friend") { deckPriority3 += 1f * enemyUnits.Count; }
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_260002>()) // Hana for All, ハナは皆のために
				{
					deckPriority1 *= 2f;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_250127>()) // Changing Stances, 体勢変更
				{
					deckPriority2 *= 2f;
					if (StageController.Instance.CurrentFloor == SephirahType.Tiphereth)
					{
						priority += 0.5f * enemyUnits.Count;
					}
				}
				priority += (deckPriority1 + deckPriority2 + deckPriority3) / deck.Count;
			}
			return priority / enemyUnits.Count;
		}

		private float CalcPriorityVsDamageRate()
		{
			var enemyUnits = BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player);
			var priority = CalcPriorityVsSmoke() * 0.7f;
			foreach (var unit in enemyUnits)
			{
				if (unit.passiveDetail.HasPassive<PassiveAbility_240023>()) // Sooty Thwack, 煙払い
				{
					priority += 2.0f / enemyUnits.Count;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_250223>()) // Finishing Touch, 始末
				{
					priority += 0.4f;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_250025>()) // Maximum Crash, マキシマムクラッシュ
				{
					priority += 2.0f / enemyUnits.Count;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_250127>()) // Changing Stances, 体勢変更
				{
					priority += 0.8f / enemyUnits.Count;
				}
			}
			return priority;
		}

		private HashSet<string> GetKeywords(BattleDiceCardModel card)
		{
			var keywords = new HashSet<string>();
			var xml = card.XmlData;
			keywords.UnionWith(xml.Keywords);
			keywords.UnionWith(BattleCardAbilityDescXmlList.Instance.GetAbilityKeywords(xml));
			foreach (var dice in xml.DiceBehaviourList)
			{
				keywords.UnionWith(BattleCardAbilityDescXmlList.Instance.GetAbilityKeywords_byScript(dice.Script));
			}
			return keywords;
		}

		private bool _usedEnemy;
		private static readonly LorId _vsNormal = new LorId(MetaInvitation.packageId, 150);
		private static readonly LorId _vsSmoke = new LorId(MetaInvitation.packageId, 151);
		private static readonly LorId _vsOverPower = new LorId(MetaInvitation.packageId, 152);
		private static readonly LorId _vsDamageRate = new LorId(MetaInvitation.packageId, 153);
		public static readonly LorId[] cards = {
			_vsNormal,
			_vsSmoke,
			_vsOverPower,
			_vsDamageRate,
		};
		private const float Uncertainty = 0.3f;
	}
}
