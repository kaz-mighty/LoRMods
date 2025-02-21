using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Mod;
using LOR_DiceSystem;


namespace MetaInvitation
{
	// 時の波動(敵)
	public class PassiveAbility_TimeWaveEnemy : PassiveAbilityBase
	{
		public override void OnRoundStart()
		{
			// 条件でカード入手
			if (CheckStatus())
			{
				var card = owner.allyCardDetail.AddTempCard(MetaInvitation.timeWaveEnemyCardId);
				if (card != null)
				{
					card.SetCostToZero(true);
					card.SetPriorityAdder(20);
				}
			}
		}

		public override BattleUnitModel ChangeAttackTarget(BattleDiceCardModel card, int idx)
		{
			if (card.XmlData.id != MetaInvitation.timeWaveEnemyCardId)
			{
				return base.ChangeAttackTarget(card, idx);
			}
			if (_target == null || _target.IsDead() || !_target.IsTargetable(owner))
			{
				return base.ChangeAttackTarget(card, idx);
			}
			return _target;
		}

		private bool CheckStatus()
		{
			int max = 0;

			var timeBuf = owner.bufListDetail.GetActivatedBuf<BattleUnitBuf_Time>();
			if (timeBuf == null || timeBuf.stack < DiceCardSelfAbility_TimeWave.canUse)
			{
				return false;
			}

			foreach (var unit in BattleObjectManager.instance.GetAliveList(Faction.Player)) {
				int point = 0;
				var bufList = unit.bufListDetail.GetActivatedBufList();
				foreach (var buf in bufList)
				{
					switch (buf.bufType)
					{
						case KeywordBuf.AllPowerUp:
							point += 3 * buf.stack;
							break;
						case KeywordBuf.Strength:
						case KeywordBuf.Endurance:
							point += 2 * buf.stack;
							break;
						case KeywordBuf.Protection:
						case KeywordBuf.BreakProtection:
						case KeywordBuf.DmgUp:
						case KeywordBuf.SlashPowerUp:
						case KeywordBuf.PenetratePowerUp:
						case KeywordBuf.HitPowerUp:
						case KeywordBuf.DefensePowerUp:
							point += buf.stack;
							break;
						case KeywordBuf.Weak:
						case KeywordBuf.Disarm:
							point -= 2 * buf.stack;
							break;
						case KeywordBuf.Paralysis:
							point -= Mathf.Min(2 * buf.stack, 6);
							break;
						case KeywordBuf.Vulnerable:
						case KeywordBuf.Vulnerable_break:
							point -= buf.stack;
							break;
						case KeywordBuf.Burn:
						case KeywordBuf.Bleeding:
							point -= buf.stack / 2 + 1;
							break;
						case KeywordBuf.Quickness:
						case KeywordBuf.Binding:
							break;
						default:
							if (buf.positiveType == BufPositiveType.Positive)
							{
								point += 2;
							}
							else if (buf.positiveType == BufPositiveType.Negative)
							{
								point -= 2;
							}
							break;
					}
				}
				if (point > max)
				{
					_target = unit;
					max = point;
				}
			}
			return max >= 6;
		}

		private BattleUnitModel _target;
	}

	// 加速
	public class PassiveAbility_EnemyTime : PassiveAbilityBase
	{
		public override void OnWaveStart()
		{
			owner.allyCardDetail.SetMaxDrawHand(12);
			owner.allyCardDetail.SetMaxHand(12);
			var deck = owner.allyCardDetail.GetAllDeck();
			foreach (var card in deck)
			{
				var newCard = owner.allyCardDetail.AddNewCardToDeck(card.GetID(), false);
				if (newCard != null)
				{
					newCard.SetPriorityAdder(-1);
				}
			}
		}

		public override void OnRoundStart()
		{
			owner.cardSlotDetail.RecoverPlayPoint(2);
			var buf = owner.bufListDetail.GetActivatedBuf<BattleUnitBuf_Time>();
			if (buf != null && buf.stack > 0)
			{
				owner.RecoverHP(Mathf.Min((buf.stack + 4) / 5, 70));
			}
		}

		public override void OnDrawCard()
		{
			owner.allyCardDetail.DrawCards(2);
		}

		public override int SpeedDiceNumAdder()
		{
			int value = 1;
			if (owner != null && owner.emotionDetail != null)
			{
				value += Mathf.Min(owner.emotionDetail.EmotionLevel, 3);
			}
			return value;
		}
	}

	// 素早く捌く
	public class PassiveAbility_QuicklyHandle : PassiveAbilityBase
	{
		public override void OnStartBattle()
		{
			var cardItem = ItemXmlDataList.instance.GetCardItem(MetaInvitation.quicklyHandleCardId);
			var list = new List<BattleDiceBehavior>();
			int num = 0;
			foreach (var dice in cardItem.DiceBehaviourList)
			{
				var battleDice = new BattleDiceBehavior();
				battleDice.behaviourInCard = dice.Copy();
				battleDice.SetIndex(num++);
				list.Add(battleDice);
			}
			owner.cardSlotDetail.keepCard.AddBehaviours(cardItem, list);
		}
	}

	// 謎の装置
	public class PassiveAbility_UnknownMachine : PassiveAbilityBase
	{
		public override bool isActionable => false;

		public override void OnRoundStart()
		{
			owner.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Protection, 3, owner);
			foreach (var unit in BattleObjectManager.instance.GetAliveList(owner.faction))
			{
				unit.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Quickness, 2, owner);
			}
		}

		public override void OnRoundEnd_before()
		{
			TimeFieldManager.Instance.stack += 30;
		}

		public override void OnRoundEndTheLast()
		{
			CheckOthers();
		}

		public override bool OnBreakGageZero()
		{
			return true;
		}

		private void CheckOthers()
		{
			BattleUnitModel battleUnitModel = BattleObjectManager.instance.GetAliveList(owner.faction).Find((BattleUnitModel x) => x != owner);
			if (battleUnitModel == null)
			{
				owner.Die(null, true);
			}
		}
	}
}
