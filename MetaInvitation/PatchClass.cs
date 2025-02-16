using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using LOR_DiceSystem;

namespace MetaInvitation
{
	class PatchClass
	{
		[HarmonyPatch]
		class TimePassive_Patch
		{
			[HarmonyPatch(typeof(StageModel), "Init")]
			[HarmonyPostfix]
			static void Init()
			{
				Singleton<TimeFieldManager>.Instance.Init();
			}

			[HarmonyPatch(typeof(StageController), "SortUnitPhase")]
			[HarmonyPostfix]
			static void OnAfterRollSpeedDice()
			{
				Singleton<TimeFieldManager>.Instance.OnAfterRollSpeedDice();
			}

			[HarmonyPatch(typeof(BattleUnitModel), "Die")]
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

		[HarmonyPatch]
		class DisableEnemyEmotionCard_Patch
		{
			// 一部ステージでEmotionCardを無効化
			// 付与時に混乱抵抗値が0から1になってしまうため
			[HarmonyPatch(typeof(StageWaveModel), "PickRandomEmotionCard")]
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

		[HarmonyPatch]
		class AddEGO_Patch
		{
			// 共有EGOにページを追加するパッシブのため
			[HarmonyPatch(typeof(EmotionBattleTeamModel), "CanUsingEgo")]
			[HarmonyPostfix]
			static void CanUsingEgoFix(ref bool __result)
			{
				var cards = (Dictionary<SephirahType, List<BattleDiceCardModel>>)AccessTools.Field(typeof(SpecialCardListModel), "_cardSelectedDataByFloor").GetValue(Singleton<SpecialCardListModel>.Instance);
				var sephirah = Singleton<StageController>.Instance.GetCurrentStageFloorModel().Sephirah;
				if (cards[sephirah].Count != 0)
				{
					__result = true;
				}
			}

			[HarmonyPatch(typeof(DiceCardXmlInfo), "Copy")]
			[HarmonyPostfix]
			static void Copy_EgoMaxCooltimeValue(DiceCardXmlInfo __instance, DiceCardXmlInfo __result)
			{
				__result.EgoMaxCooltimeValue = __instance.EgoMaxCooltimeValue;
			}
		}

		[HarmonyPatch]
		class Preparedness_Patch
		{
			[HarmonyPatch(typeof(BattleFarAreaPlayManager), "EndFarAreaPlay")]
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

		[HarmonyPatch]
		class MetaOverPower_Patch
		{
			[HarmonyPatch(typeof(BattleDiceBehavior), "UpdateDiceFinalValue")]
			[HarmonyPrefix]
			static void HalfPower(BattleDiceBehavior __instance, DiceStatBonus ____statBonus)
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

		[HarmonyPatch]
		class MetaDamageRate_Patch
		{
			static bool HasAllyBuf(BattleUnitModel unit)
			{
				return unit?.bufListDetail.GetActivatedBufList().Find(
					x => x is Second.BattleUnitBuf_MetaDamageRateAlly && !x.IsDestroyed()
				) != null;
			}

			static bool HasEnemyBuf(BattleUnitModel unit)
			{
				return unit?.bufListDetail.GetActivatedBufList().Find(
					x => x is Second.BattleUnitBuf_MetaDamageRateEnemy && !x.IsDestroyed()
				) != null;
			}

			[HarmonyPatch(typeof(BattleDiceBehavior), "ApplyDiceStatBonus")]
			[HarmonyPrefix]
			static void DiceStatBonusHalf(BattleDiceBehavior __instance, DiceStatBonus bonus)
			{
				if (!HasEnemyBuf(__instance.card.owner)) { return; }

				if (bonus.dmgRate > 0) { bonus.dmgRate /= 2; }
				if (bonus.breakRate > 0) { bonus.breakRate /= 2; }
			}

			[HarmonyPatch(typeof(BattleUnitBufListDetail), "GetDamageIncreaseRate")]
			[HarmonyPatch(typeof(BattleUnitBufListDetail), "GetBreakDamageIncreaseRate")]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> DamageIncreaseRateHalf(IEnumerable<CodeInstruction> codes, ILGenerator ilGen, MethodBase original)
			{
				var hasAllyBuf = ilGen.DeclareLocal(typeof(bool));
				var x = ilGen.DeclareLocal(typeof(int));
				var targetMethod = AccessTools.Method(typeof(BattleUnitBuf), original.Name);

				// insert: bool hasAllyBuf = HasAllyBuf(this._self);
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BattleUnitBufListDetail), "_self"));
				yield return CodeInstruction.Call(typeof(MetaDamageRate_Patch), "HasAllyBuf");
				yield return new CodeInstruction(OpCodes.Stloc, hasAllyBuf);

				var codeList = new List<CodeInstruction>(codes);
				foreach(var code in codes)
				{
					yield return code;
					if (code.Calls(targetMethod))
					{
						// diff (if GetDamageIncreaseRate):
						// - num += battleUnitBuf.GetDamageIncreaseRate();
						// + int x = battleUnitBuf.GetDamageIncreaseRate();
						// + if (hasAllyBuf && x > 0) { x /= 2; }
						// + num += x;
						var label = ilGen.DefineLabel();
						yield return new CodeInstruction(OpCodes.Stloc, x);
						yield return new CodeInstruction(OpCodes.Ldloc, hasAllyBuf);
						yield return new CodeInstruction(OpCodes.Brfalse, label);

						yield return new CodeInstruction(OpCodes.Ldloc, x);
						yield return new CodeInstruction(OpCodes.Ldc_I4_0);
						yield return new CodeInstruction(OpCodes.Ble, label);

						yield return new CodeInstruction(OpCodes.Ldloc, x);
						yield return new CodeInstruction(OpCodes.Ldc_I4_2);
						yield return new CodeInstruction(OpCodes.Div);
						yield return new CodeInstruction(OpCodes.Stloc, x);

						var labelCode = new CodeInstruction(OpCodes.Ldloc, x);
						labelCode.labels.Add(label);
						yield return labelCode;
					}
				}
				yield break;
			}

			[HarmonyPatch(typeof(BattleUnitEmotionDetail), "DmgFactor")]
			[HarmonyPatch(typeof(BattleUnitEmotionDetail), "BreakDmgFactor")]
			[HarmonyPatch(typeof(BattleUnitPassiveDetail), "DmgFactor")]
			[HarmonyPatch(typeof(BattleUnitPassiveDetail), "BreakDmgFactor")]
			[HarmonyPatch(typeof(BattleUnitBufListDetail), "DmgFactor")]
			[HarmonyPatch(typeof(BattleUnitBufListDetail), "BreakDmgFactor")]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> DamageFactorHalf(IEnumerable<CodeInstruction> codes, ILGenerator ilGen, MethodBase original)
			{
				var hasAllyBuf = ilGen.DeclareLocal(typeof(bool));
				var x = ilGen.DeclareLocal(typeof(float));
				Type methodClass;
				if (original.DeclaringType == typeof(BattleUnitEmotionDetail))
				{
					methodClass = typeof(BattleEmotionCardModel);
				}
				else if (original.DeclaringType == typeof(BattleUnitPassiveDetail))
				{
					methodClass = typeof(PassiveAbilityBase);
				}
				else
				{
					methodClass = typeof(BattleUnitBuf);
				}
				var targetMethod = AccessTools.Method(methodClass, original.Name);

				// insert: bool hasAllyBuf = HasAllyBuf(this._self);
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(original.DeclaringType, "_self"));
				yield return CodeInstruction.Call(typeof(MetaDamageRate_Patch), "HasAllyBuf");
				yield return new CodeInstruction(OpCodes.Stloc, hasAllyBuf);

				var codeList = new List<CodeInstruction>(codes);
				foreach(var code in codes)
				{
					yield return code;
					if (code.Calls(targetMethod))
					{
						// diff (example):
						// - num *= battleEmotionCardModel.DmgFactor(dmg, type, keyword);
						// + float x = battleEmotionCardModel.DmgFactor(dmg, type, keyword);
						// + if (hasAllyBuf && x > 1f) { x = x / 2f + 0.5f; }
						// + num *= x;
						var label = ilGen.DefineLabel();
						yield return new CodeInstruction(OpCodes.Stloc, x);
						yield return new CodeInstruction(OpCodes.Ldloc, hasAllyBuf);
						yield return new CodeInstruction(OpCodes.Brfalse, label);

						yield return new CodeInstruction(OpCodes.Ldloc, x);
						yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
						yield return new CodeInstruction(OpCodes.Ble_Un, label);

						yield return new CodeInstruction(OpCodes.Ldloc, x);
						yield return new CodeInstruction(OpCodes.Ldc_R4, 2f);
						yield return new CodeInstruction(OpCodes.Div);
						yield return new CodeInstruction(OpCodes.Ldc_R4, 0.5f);
						yield return new CodeInstruction(OpCodes.Add);
						yield return new CodeInstruction(OpCodes.Stloc, x);
						var labelCode = new CodeInstruction(OpCodes.Ldloc, x);
						labelCode.labels.Add(label);
						yield return labelCode;
					}
				}
				yield break;
			}
		}
	}
}
