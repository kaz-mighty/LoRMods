using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Mod;
using LOR_DiceSystem;

namespace MetaInvitation
{
	public class TimeFieldManager : Singleton<TimeFieldManager>
	{
		public void Init()
		{
			stack = 0;
		}

		public void OnAfterRollSpeedDice()
		{
			var speedNum = 0;
			BattleObjectManager.instance.GetAliveList(false).ForEach(x =>
			{
				speedNum += x.speedDiceResult.Count;
			});

			stack += speedNum;
			var turn = StageController.Instance.RoundTurn;
			Debug.Log($"{MetaInvitation.packageId}: Trun {turn}, SpeedNum {speedNum}, NowTimeStack {stack}");
			return;
		}

		public int stack = 0;
	}

	// 時の回廊
	public class PassiveAbility_TimeField : PassiveAbilityBase
	{
		public override void OnWaveStart()
		{
			var stack = TimeFieldManager.Instance.stack;
			foreach (var x in BattleObjectManager.instance.GetAliveList((owner.faction == Faction.Enemy) ? Faction.Enemy : Faction.Player))
			{
				if (!x.bufListDetail.HasBuf<BattleUnitBuf_Time>())
				{
					var timeBuf = new BattleUnitBuf_Time();
					x.bufListDetail.AddBuf(timeBuf);
					timeBuf.SetStack(stack);
				}
			}
		}
	}

	// 時の波動
	public class PassiveAbility_TimeWave : PassiveAbilityBase
	{
		public override void OnWaveStart()
		{
			var specialCardList = SpecialCardListModel.Instance;
			var _cardInHand = (List<BattleDiceCardModel>)AccessTools.Field(typeof(SpecialCardListModel), "_cardInHand").GetValue(specialCardList);
			if (!_cardInHand.Exists(x => x.GetID() == MetaInvitation.timeWaveCardId))
			{
				specialCardList.AddCard(MetaInvitation.timeWaveCardId, StageController.Instance.GetCurrentStageFloorModel().Sephirah);
			}
		}
	}

	// 絆
	public class PassiveAbility_Kizuna : PassiveAbilityBase
	{
		public override void OnWaveStart()
		{
			_stack = 0;
		}

		public override void OnRoundStart()
		{
			if (_stack > 0)
			{
				owner.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Strength, _stack, owner);
				owner.bufListDetail.AddKeywordBufThisRoundByEtc(KeywordBuf.Endurance, _stack, owner);
			}
		}

		public override void OnDieOtherUnit(BattleUnitModel unit)
		{
			if (unit.faction == owner.faction && _stack < 2)
			{
				_stack++;
			}
		}

		private int _stack = 0;
	}

	// 混乱耐性を変更
	public class PassiveAbility_TimeSub1 : PassiveAbilityBase
	{
		public override bool isHide { get { return true; } }

		public override bool DontChangeResistByBreak()
		{
			return true;
		}

		public override AtkResist GetResistHP(AtkResist origin, BehaviourDetail detail)
		{
			if (owner.IsBreakLifeZero())
			{
				return (AtkResist)Mathf.Max((int)origin - 1, 1);
			}
			return origin;
		}

		public override AtkResist GetResistBP(AtkResist origin, BehaviourDetail detail)
		{
			if (owner.IsBreakLifeZero())
			{
				return (AtkResist)Mathf.Max((int)origin - 1, 1);
			}
			return origin;
		}
	}

	// ダメージリミッター
	public class PassiveAbility_TimeSub2 : PassiveAbilityBase
	{
		public override void OnRoundStartAfter()
		{
			_minHp = (int)owner.hp - (owner.MaxHp * 3).CeilDiv(4);
		}

		public override bool BeforeTakeDamage(BattleUnitModel attacker, int dmg)
		{
			var overDmg = dmg + owner.history.takeDamageAtOneRound - owner.MaxHp / 4;
			if (overDmg > 0)
			{
				_dmgReduction = Mathf.Min(overDmg, dmg) / 2;
			}
			return false;
		}

		public override int GetDamageReductionAll()
		{
			return _dmgReduction;
		}

		public override void AfterTakeDamage(BattleUnitModel attacker, int dmg)
		{
			_dmgReduction = 0;
		}

		public override int GetMinHp()
		{
			return _minHp;
		}

		public override int GetBreakDamageReductionAll(int dmg, DamageType dmgType, BattleUnitModel attacker)
		{
			var overDmg = dmg + owner.history.takeBreakDamageAtOneRound - owner.breakDetail.GetDefaultBreakGauge() / 4;
			if (overDmg > 0)
			{
				return Mathf.Min(overDmg, dmg) / 2;
			}
			return 0;
		}

		private int _minHp = -999;
		private int _dmgReduction = 0;
	}

	// 時バフ
	public class BattleUnitBuf_Time : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_Time";

		public override void OnRoundStart()
		{
			var v = Mathf.Clamp(stack.RandomRoundDiv(80), 1, 8);
			_owner.RecoverHP(v);
		}

		public override void OnRoundEnd()
		{
			SetStack(TimeFieldManager.Instance.stack);
		}

		public override void BeforeRollDice(BattleDiceBehavior behavior)
		{
			if (stack >= addMax || stack >= addMin)
			{
				behavior.ApplyDiceStatBonus(new DiceStatBonus
				{
					max = Convert.ToInt32(stack >= addMax),
					min = Convert.ToInt32(stack >= addMin)
				});
			}
		}

		public void SetStack(int newStack)
		{
			if (stack < addBreakResistStack && newStack >= addBreakResistStack)
			{
				_owner.passiveDetail.AddPassive(new PassiveAbility_TimeSub1());
			}
			if (stack < addKizuna && newStack >= addKizuna)
			{
				_owner.passiveDetail.AddPassive(MetaInvitation.kizunaPassiveId);
			}
			if (stack < addSafety && newStack >= addSafety)
			{
				_owner.passiveDetail.AddPassive(MetaInvitation.timeSub2PassiveId);
			}
			stack = newStack;
		}

		private const int addBreakResistStack = 150;
		private const int addKizuna = 200;
		private const int addMax = 300;
		private const int addMin = 400;
		private const int addSafety = 500;
	}
}
