using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;
using OverPower = MetaInvitation.Second.BattleUnitBuf_MetaOverPower;


namespace MetaInvitation.Second
{
	public class PassiveAbility_kaz_MetaTactics : PassiveAbilityBase
	{
		public void OnPassiveCardUsed()
		{
			_isAddCardNextRound = true;
		}

		public override void OnWaveStart()
		{
			if (owner.faction != Faction.Player)
			{
				return;
			}
			foreach (var cardId in cardIds)
			{
				owner.personalEgoDetail.AddCard(cardId);
			}
		}

		public override void OnRoundStart()
		{
			if (owner.faction != Faction.Player) { return; }

			if (_isAddCardNextRound)
			{
				_isAddCardNextRound = false;
				foreach (var cardId in cardIds)
				{
					owner.personalEgoDetail.AddCard(cardId);
				}

				var deckCards = owner.personalEgoDetail.GetCardAll();
				foreach (var cardId in cardIds)
				{
					deckCards.Find(x => x.GetID() == cardId)?.SetCurrentCost(3);
				}
			}
		}

		public override void OnRoundStartAfter()
		{
			if (owner.faction != Faction.Enemy)
			{
				return;
			}
			// 評価が難しくなるので1ターン目は使用させない
			if (StageController.Instance.RoundTurn <= 1)
			{
				return;
			}
			if (owner.RollSpeedDice().FindAll(x => !x.breaked).Count <= 0 || owner.IsBreakLifeZero())
			{
				return;
			}
			if (_beforeUseCard != CardType.None && owner.PlayPoint < _EnemyReuseCost + 1)
			{
				return;
			}

			var priorityList = new List<Tuple<CardType, float, float>>
			{
				Tuple.Create(CardType.Normal, CalcPriorityVsNormal(), RandomUtil.RangeFloat(0f, Uncertainty)),
				Tuple.Create(CardType.Smoke, CalcPriorityVsSmoke(), RandomUtil.RangeFloat(0f, Uncertainty)),
				Tuple.Create(CardType.OverPower, CalcPriorityVsOverPower(), RandomUtil.RangeFloat(0f, Uncertainty)),
				Tuple.Create(CardType.DamageRate, CalcPriorityVsDamageRate(), RandomUtil.RangeFloat(0f, Uncertainty)),
			};
			var logs = new List<string> { MetaInvitation.packageId + " MetaTactics:" };
			foreach (var tuple in priorityList)
			{
				logs.Add($"{tuple.Item1, 10}, {tuple.Item2:0.00}, {tuple.Item3:0.00}");
			}
			Debug.Log(string.Join("\n", logs));
			priorityList.Sort((x, y) => (y.Item2 + y.Item3).CompareTo(x.Item2 + x.Item3));

			if (priorityList[0].Item1 == _beforeUseCard) { return; }
			switch (priorityList[0].Item1)
			{
				case CardType.Normal:
					new DiceCardSelfAbility_MetaNormal().Activate(owner);
					break;
				case CardType.Smoke:
					new DiceCardSelfAbility_MetaSmoke().Activate(owner);
					break;
				case CardType.OverPower:
					new DiceCardSelfAbility_MetaOverPower().Activate(owner);
					break;
				case CardType.DamageRate:
					new DiceCardSelfAbility_MetaDamageRate().Activate(owner);
					break;
				default:
					return;
			}

			if (_beforeUseCard != CardType.None)
			{
				owner.cardSlotDetail.LosePlayPoint(_EnemyReuseCost);
			}
			// スキンは変わらないが、目立つようにエフェクトを追加
			owner.view.StartEgoSkinChangeEffect("Character");
			Sound.SoundEffectManager.Instance.PlayClip("Battle/Purple_Change");
			_beforeUseCard = priorityList[0].Item1;
		}

		// 評価値は体力・混乱増加率を基準にする
		private float CalcPriorityVsNormal()
		{
			var priority = 0.1f * 2f;
			if (_beforeUseCard == CardType.Normal) { priority += _NotReusePriority; }
			return priority;
		}

		private float CalcPriorityVsSmoke()
		{
			// 煙スタックはラウンド中に増加することを予想して高めに評価する
			var priorityDamage = 0.0f;
			var priorityPower = 0.0f;

			var enemyUnits = BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player);
			foreach (var unit in enemyUnits)
			{
				var smoke = 0;
				var buf = unit.bufListDetail.GetActivatedBuf(KeywordBuf.Smoke);
				if (buf != null)
				{
					smoke = buf.stack;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_240026>()) // Puffy Brume, もくもく
				{
					priorityDamage += (smoke + 2) * 0.05f;
				}
				else
				{
					priorityDamage -= smoke * 0.05f;
				}
				if (smoke + 2 >= 9) { priorityPower += 0.4f; }
			}
			// 他のダメージ割合増加があるとき、比で比較するとあまり減らないので補正
			priorityDamage *= 0.8f;

			var priority = (priorityDamage + priorityPower) / enemyUnits.Count;
			if (_beforeUseCard == CardType.Smoke) { priority += _NotReusePriority; }
			return priority;
		}

		private float CalcPriorityVsOverPower()
		{
			// 威力をすべて考慮するのは不可能なので、汎用的なものと強すぎなもののみ
			var priorityEnemy = 0.0f;
			var priorityAlly = 0.0f;

			var voidEgo = SpecialCardListModel.Instance.GetHand().Find(x => x.GetID() == 910030);
			bool canVoidEgo = voidEgo != null ? SpecialCardListModel.Instance.ExistEgoCardHand(voidEgo) : false;
			int voidEgoEval = 0;

			var enemyUnits = BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player);
			foreach (var unit in enemyUnits)
			{
				bool isStance = unit.passiveDetail.HasPassive<PassiveAbility_250127>(); // Changing Stances, 体勢変更

				var power = CalcAttackPower(unit) + (isStance ? 2 : 0);
				if (power > OverPower._enemyUpper)
				{
					priorityEnemy += 0.15f * (power - OverPower._enemyUpper);
				}
				power = CalcDefencePower(unit) + (isStance ? 2 : 0);
				if (power > OverPower._enemyUpper)
				{
					priorityEnemy += 0.12f * (power - OverPower._enemyUpper);
				}

				if (canVoidEgo && unit.allyCardDetail.GetHand().Count <= 3)
				{
					// 使用が確定したわけではないため、少し小さめに評価
					voidEgoEval = Mathf.Max(voidEgoEval, isStance ? 6 : 3);
				}
			}

			var allyUnits = BattleObjectManager.instance.GetAliveList(owner.faction);
			foreach (var unit in allyUnits)
			{
				var power = CalcAttackPower(unit) - voidEgoEval;
				if (power < OverPower._allyLower)
				{
					priorityAlly += -0.15f * (power - OverPower._allyLower);
				}
				power = CalcDefencePower(unit) - voidEgoEval;
				if (power < OverPower._allyLower)
				{
					priorityAlly += -0.12f * (power - OverPower._allyLower);
				}
			}

			priorityEnemy /= enemyUnits.Count;
			priorityAlly /= allyUnits.Count;
			var priority = priorityEnemy + priorityAlly;
			if (_beforeUseCard == CardType.OverPower) { priority += _NotReusePriority; }
			return priority;
		}

		private float CalcPriorityVsDamageRate()
		{
			var priorityEnemy = 0f;
			var priorityAlly = 0f;
			var nicolaiTarget = false;

			var enemyUnits = BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Player) ? Faction.Enemy : Faction.Player);
			foreach (var unit in enemyUnits)
			{
				var smoke = 0;
				var buf = unit.bufListDetail.GetActivatedBuf(KeywordBuf.Smoke);
				if (buf != null)
				{
					smoke = buf.stack;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_240026>()) // Puffy Brume, もくもく
				{
					priorityEnemy += (smoke + 2) * 0.05f;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_240023>()) // Sooty Thwack, 煙払い
				{
					priorityEnemy += (smoke + 2) * 0.1f;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_250223>()) // Finishing Touch, 始末
				{
					nicolaiTarget = true;
				}
				if (unit.passiveDetail.HasPassive<PassiveAbility_250025>()) // Maximum Crash, マキシマムクラッシュ
				{
					priorityEnemy += 0.5f * 2f;
				}
				if (unit.bufListDetail.GetActivatedBuf(KeywordBuf.PurpleSlash) != null)
				{
					priorityEnemy += 0.5f;
				}
				if (unit.bufListDetail.GetActivatedBuf(KeywordBuf.PurpleHit) != null)
				{
					priorityEnemy += 0.5f;
				}
			}

			var allyUnits = BattleObjectManager.instance.GetAliveList(owner.faction);
			foreach (var unit in allyUnits)
			{
				var buf = unit.bufListDetail.GetActivatedBuf(KeywordBuf.Smoke);
				if (buf != null)
				{
					priorityAlly += buf.stack * 0.05f;
				}
			}

			priorityEnemy /= enemyUnits.Count;
			priorityAlly /= allyUnits.Count;
			if (nicolaiTarget) {
				priorityAlly += 0.5f * 2f * 2f / (allyUnits.Count + 1);
			}
			// ダメージ増加率 + 混乱ダメージ増加率を
			// 体力増加率 + 混乱抵抗値増加率 に変換する
			// (単純化のため2種の増加率は等しいとする)
			var priority = priorityEnemy + priorityAlly;
			priority = (2f * priority) / (4f + priority);

			if (_beforeUseCard == CardType.DamageRate) { priority += _NotReusePriority; }
			return priority;
		}

		private int CalcAttackPower(BattleUnitModel unit)
		{
			var power = unit.bufListDetail.GetActivatedBuf(KeywordBuf.Strength)?.stack ?? 0;
			power += unit.bufListDetail.GetActivatedBuf(KeywordBuf.AllPowerUp)?.stack ?? 0;
			power -= unit.bufListDetail.GetActivatedBuf(KeywordBuf.Weak)?.stack ?? 0;
			return power;
		}

		private int CalcDefencePower(BattleUnitModel unit)
		{
			var power = unit.bufListDetail.GetActivatedBuf(KeywordBuf.Endurance)?.stack ?? 0;
			power += unit.bufListDetail.GetActivatedBuf(KeywordBuf.AllPowerUp)?.stack ?? 0;
			power -= unit.bufListDetail.GetActivatedBuf(KeywordBuf.Disarm)?.stack ?? 0;
			return power;
		}

		private bool _isAddCardNextRound;
		private CardType _beforeUseCard; // enemy only

		private static readonly LorId _vsNormal = new LorId(MetaInvitation.packageId, 150);
		private static readonly LorId _vsSmoke = new LorId(MetaInvitation.packageId, 151);
		private static readonly LorId _vsOverPower = new LorId(MetaInvitation.packageId, 152);
		private static readonly LorId _vsDamageRate = new LorId(MetaInvitation.packageId, 153);
		public static readonly LorId[] cardIds = {
			_vsNormal,
			_vsSmoke,
			_vsOverPower,
			_vsDamageRate,
		};
		private const int _EnemyReuseCost = 2;

		private const float Uncertainty = 0.15f;
		private const float _NotReusePriority = 0.25f;

		enum CardType
		{
			None,
			Normal,
			Smoke,
			OverPower,
			DamageRate,
		}
	}
}
