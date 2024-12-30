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

		[HarmonyPatch(typeof(BattleUnitBuf))]
		class BattleUnitBuf_Patch
		{
			[HarmonyPatch("GetAddTextData")]
			[HarmonyPostfix]
			static void GetAddTextData_Postfix(BattleUnitBuf __instance, ref string __result)
			{
				if (__instance.positiveType == BufPositiveType.Positive)
				{
					__result += " \n<color=#00FF00>(" + TextDataModel.GetText("BattleUI_buf", Array.Empty<object>()) + ")</color>";
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
	}
}
