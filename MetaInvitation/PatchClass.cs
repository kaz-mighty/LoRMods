using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using LOR_DiceSystem;

namespace MetaInvitation
{
	class PatchClass
	{
		[HarmonyPatch(typeof(StageModel))]
		class StageModel_Patch
		{
			[HarmonyPatch("Init")]
			[HarmonyPostfix]
			static void Init_Postfix()
			{
				Singleton<TimeFieldManager>.Instance.Init();
			}
		}

		[HarmonyPatch(typeof(StageController))]
		class StageController_Patch
		{
			[HarmonyPatch("SortUnitPhase")]
			[HarmonyPostfix]
			static void SortUnitPhase_Postfix()
			{
				Singleton<TimeFieldManager>.Instance.OnAfterRollSpeedDice();
			}
		}

		[HarmonyPatch(typeof(StageWaveModel))]
		class StageWaveModel_Patch
		{
			// 一部ステージでEmotionCardを無効化
			// 付与時に混乱抵抗値が0から1になってしまうため
			[HarmonyPatch("PickRandomEmotionCard")]
			[HarmonyPrefix]
			static bool PickRandomEmotionCard_Prefix(StageWaveModel __instance)
			{
				var id = __instance.team.stage.id;
				if (id == MetaInvitation.disabledEmotionCardStageId)
				{
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(EmotionBattleTeamModel))]
		class EmotionBattleTeamModel_Patch
		{
			// 共有EGOにページを追加するパッシブのため
			[HarmonyPatch("CanUsingEgo")]
			[HarmonyPostfix]
			static void CanUsingEgo_Postfix(ref bool __result)
			{
				var cards = (Dictionary<SephirahType, List<BattleDiceCardModel>>)AccessTools.Field(typeof(SpecialCardListModel), "_cardSelectedDataByFloor").GetValue(Singleton<SpecialCardListModel>.Instance);
				var sephirah = Singleton<StageController>.Instance.GetCurrentStageFloorModel().Sephirah;
				if (cards[sephirah].Count != 0)
				{
					__result = true;
				}
			}
		}

		[HarmonyPatch(typeof(DiceCardXmlInfo))]
		class DiceCardXmlInfo_Patch
		{
			[HarmonyPatch("Copy")]
			[HarmonyPostfix]
			static void Copy_EgoMaxCooltimeValue(DiceCardXmlInfo __instance, DiceCardXmlInfo __result)
			{
				__result.EgoMaxCooltimeValue = __instance.EgoMaxCooltimeValue;
			}
		}

		[HarmonyPatch(typeof(BattleUnitModel))]
		class BattleUnitModel_Patch
		{
			[HarmonyPatch("Die")]
			[HarmonyPrefix]
			[HarmonyPriority(Priority.LowerThanNormal)]
			static bool ForcelyDieCancel(BattleUnitModel __instance, bool __runOriginal)
			{
				if (__runOriginal && __instance.hp > __instance.Book.DeadLine && __instance.passiveDetail.HasPassive<PassiveAbility_TimeSub2>())
				{
					var minHp = __instance.GetMinHp();
					if (minHp > __instance.Book.DeadLine)
					{
						AccessTools.Property(typeof(BattleUnitModel), "hp").SetValue(__instance, minHp);
						SingletonBehavior<BattleManagerUI>.Instance.ui_unitListInfoSummary.UpdateCharacterProfile(__instance, __instance.faction, __instance.hp, __instance.breakDetail.breakGauge, null);
						return false;
					}
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(BattleFarAreaPlayManager))]
		class BattleFarAreaPlayManager_Patch
		{
			[HarmonyPatch("EndFarAreaPlay")]
			[HarmonyPrefix]
			static void CancelDestroyDices(BattleFarAreaPlayManager __instance, float ____endDelay, float ____elapsedEndDelay, float deltaTime)
			{
				if (____elapsedEndDelay + deltaTime < ____endDelay)
				{
					return;
				}
				foreach (var victimInfo in __instance.victims)
				{
					if (victimInfo.unitModel.passiveDetail.HasPassive<Second.PassiveAbility_kaz_Preparedness>())
					{
						victimInfo.cardDestroyed = false;
						victimInfo.destroyedDicesIndex.Clear();
					}
				}
			}
		}

		[HarmonyPatch(typeof(BattleUnitBuf_smoke))]
		[HarmonyPriority(Priority.Normal + 10)]
		class MetaSmoke_Patch
		{
			[HarmonyPatch("bufActivatedText", MethodType.Getter)]
			[HarmonyPrefix]
			static bool bufActivatedText(bool __runOriginal, ref string __result, BattleUnitModel ____owner)
			{
				if (!__runOriginal) { return false; }
				if (____owner.bufListDetail.HasBuf<Second.BattleUnitBuf_MetaSmoke>())
				{
					__result = Singleton<BattleEffectTextsXmlList>.Instance.GetEffectTextDesc(MetaInvitation.packageId + "_Smoke");
					return false;
				}
				return true;
			}

			[HarmonyPatch(typeof(BattleUnitBuf_smoke), "BeforeRollDice")]
			[HarmonyPatch(typeof(BattleUnitBuf_smoke), "BeforeGiveDamage")]
			[HarmonyPrefix]
			static bool BeforeEffect(bool __runOriginal, BattleUnitModel ____owner)
			{
				if (!__runOriginal) { return false; }
				if (____owner.bufListDetail.HasBuf<Second.BattleUnitBuf_MetaSmoke>()) { return false; }
				return true;
			}

			[HarmonyPatch("GetDamageIncreaseRate")]
			[HarmonyPrefix]
			static bool GetDamageIncreaseRate(bool __runOriginal, ref int __result, BattleUnitModel ____owner)
			{
				if (!__runOriginal) { return false; }
				if (____owner.bufListDetail.HasBuf<Second.BattleUnitBuf_MetaSmoke>())
				{
					__result = 0;
					return false;
				}
				return true;
			}
		}

		[HarmonyPatch(typeof(BattleDiceBehavior), "UpdateDiceFinalValue")]
		[HarmonyPrefix]
		static void MetaOverPower_Patch(BattleDiceBehavior __instance, DiceStatBonus ____statBonus)
		{
			var buf = __instance.owner?.bufListDetail.GetActivatedBufList().Find(x => x is Second.BattleUnitBuf_MetaOverPower) as Second.BattleUnitBuf_MetaOverPower;
			if (buf != null)
			{
				if (____statBonus.power < buf.Lower)
				{
					____statBonus.power += (buf.Lower - ____statBonus.power).RandomRoundDiv(2);
				}
				else if (____statBonus.power > buf.Upper)
				{
					____statBonus.power -= (____statBonus.power - buf.Upper).RandomRoundDiv(2);
				}
			}
		}

	}
}
