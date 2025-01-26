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
	}
}
