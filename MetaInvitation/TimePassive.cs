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
			remainPenaltyTurnOfPlayer = 2;
			remainPenaltyTurnOfEnemy = 2;
		}

		public void OnWaveStart()
		{
			alreadyPenaltyTurnOfPlayer = 0;
			alreadyPenaltyTurnOfEnemy = 0;
		}

		public void OnRoundStart(Faction faction)
		{
			var turn = Singleton<StageController>.Instance.RoundTurn;

			//Debug.Log(MetaInvitation.packageId + ": OnRoundStart turn " + turn);

			if (faction == Faction.Player)
			{
				if (remainPenaltyTurnOfPlayer <= 0 || turn <= alreadyPenaltyTurnOfPlayer)
				{
					return;
				}
				//Debug.Log(MetaInvitation.packageId + ": Adding a penalty to players.");
				BattleObjectManager.instance.GetAliveList(Faction.Player).ForEach(x =>
				{
					x.bufListDetail.AddBuf(new BattleUnitBuf_TimePenalty());
				});
				remainPenaltyTurnOfPlayer -= 1;
				alreadyPenaltyTurnOfPlayer = turn;
			}
			else
			{
				if (remainPenaltyTurnOfEnemy <= 0 || turn <= alreadyPenaltyTurnOfEnemy)
				{
					return;
				}
				//Debug.Log(MetaInvitation.packageId + ": Adding a penalty to enemies.");
				BattleObjectManager.instance.GetAliveList(Faction.Enemy).ForEach(x =>
				{
					x.bufListDetail.AddBuf(new BattleUnitBuf_TimePenalty());
				});
				remainPenaltyTurnOfEnemy -= 1;
				alreadyPenaltyTurnOfEnemy = turn;
			}
		}

		public void OnAfterRollSpeedDice()
		{
			var speedNum = 0;
			BattleObjectManager.instance.GetAliveList(false).ForEach(x =>
			{
				speedNum += x.speedDiceResult.Count;
			});

			stack += speedNum;
			Debug.Log(string.Format("{0}: AddTimeStack {1}, Now {2}", MetaInvitation.packageId, speedNum, stack));
			return;
		}

		public int stack = 0;
		private int remainPenaltyTurnOfPlayer = 0;
		private int remainPenaltyTurnOfEnemy = 0;
		private int alreadyPenaltyTurnOfPlayer = 0;
		private int alreadyPenaltyTurnOfEnemy = 0;
	}

	// 時の回廊
	public class PassiveAbility_TimeField : PassiveAbilityBase
	{
		public override void OnWaveStart()
		{
			Singleton<TimeFieldManager>.Instance.OnWaveStart();

			var stack = Singleton<TimeFieldManager>.Instance.stack;
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

		public override void OnRoundStart()
		{
			Singleton<TimeFieldManager>.Instance.OnRoundStart(owner.faction);
		}
	}

	// 時の波動
	public class PassiveAbility_TimeWave : PassiveAbilityBase
	{
		public override void OnWaveStart()
		{
			var specialCardList = Singleton<SpecialCardListModel>.Instance;
			var _cardInHand = (List<BattleDiceCardModel>)AccessTools.Field(typeof(SpecialCardListModel), "_cardInHand").GetValue(specialCardList);
			if (!_cardInHand.Exists(x => x.GetID() == MetaInvitation.timeWaveCardId))
			{
				specialCardList.AddCard(MetaInvitation.timeWaveCardId, Singleton<StageController>.Instance.GetCurrentStageFloorModel().Sephirah);
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
	public class PassiveAbility_TimeFieldSub1 : PassiveAbilityBase
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

	// 時バフ
	public class BattleUnitBuf_Time : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_Time";

		public override void OnRoundStart()
		{
			var v = Mathf.Min((stack + 49) / 50, 10);
			if (v > 0)
			{
				_owner.RecoverHP(v);
			}
		}

		public override void OnRoundEnd()
		{
			SetStack(Singleton<TimeFieldManager>.Instance.stack);
		}

		public override void BeforeRollDice(BattleDiceBehavior behavior)
		{
			if (stack >= addPower)
			{
				behavior.ApplyDiceStatBonus(new DiceStatBonus
				{
					power = 1
				});
			}
		}

		public void SetStack(int newStack)
		{
			if (stack < addBreakResistStack && newStack >= addBreakResistStack)
			{
				_owner.passiveDetail.AddPassive(new PassiveAbility_TimeFieldSub1());

			}
			if (stack < addKizuna && newStack >= addKizuna)
			{
				_owner.passiveDetail.AddPassive(MetaInvitation.kizunaPassiveId);
			}
			stack = newStack;
		}

		private const int addBreakResistStack = 125;
		private const int addKizuna = 225;
		private const int addPower = 425;
	}

	public class BattleUnitBuf_TimePenalty : BattleUnitBuf
	{
		public BattleUnitBuf_TimePenalty()
		{
			stack = 0;
		}

		protected override string keywordId => MetaInvitation.packageId + "_TimePenalty";

		public override float DmgFactor(int dmg, DamageType type = DamageType.ETC, KeywordBuf keyword = KeywordBuf.None)
		{
			return dmgFactor;
		}

		public override float BreakDmgFactor(int dmg, DamageType type = DamageType.ETC, KeywordBuf keyword = KeywordBuf.None)
		{
			return dmgFactor;
		}

		public override void OnRoundEnd()
		{
			Destroy();
		}

		private const float dmgFactor = 1.25f;
	}
}
