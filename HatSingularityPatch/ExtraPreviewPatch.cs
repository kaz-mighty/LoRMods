using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;
using Hat_Effect;
using Hat_Harmony;


namespace HatPatch
{
	[HarmonyPatch]
	class ExtraPreviewPatch
	{
		[HarmonyPatch(typeof(BattleDiceCardUI), "HideDetail")]
		[HarmonyPatch(typeof(BattleDiceCardUI), "ResetSiblingIndex")]
		[HarmonyPatch(typeof(BattleDiceCardUI), "OnClick")]
		[HarmonyPatch(typeof(BattleDiceCardUI), "ShowDetail")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> SetOrderInsert(IEnumerable<CodeInstruction> codes, MethodBase original)
		{
			var targetMethod = AccessTools.Method(typeof(Transform), "SetSiblingIndex");
			MethodInfo insertMethod;
			switch (original.Name)
			{
				case "HideDetail":
				case "ResetSiblingIndex":
					insertMethod = AccessTools.Method(typeof(ExtraPreviewPatch), "ResetOrder");
					break;
				case "OnClick":
				case "ShowDetail":
					insertMethod = AccessTools.Method(typeof(ExtraPreviewPatch), "SetOrder");
					break;
				default:
					throw new ArgumentException("The patch attribute has an invalid value.");
			}
			foreach (var code in codes)
			{
				yield return code;
				if (code.Calls(targetMethod))
				{
					// diff (when inserting ResetOrder):
					//   base.transform.SetSiblingIndex(/* any */);
					// + ResetOrder(this);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, insertMethod);
				}
			}
			yield break;
		}

		static void ResetOrder(BattleDiceCardUI instance)
		{
			var canvas = instance.gameObject.GetComponent<Canvas>();
			if (canvas != null)
			{
				canvas.sortingOrder = instance.transform.GetSiblingIndex() * 2 + 1351;

				// Also fix the Renderer Order of the Book Hunter Mod
				foreach (var renderer in instance.gameObject.GetComponentsInChildren<Renderer>())
				{
					renderer.sortingOrder = canvas.sortingOrder + 1;
				}
			}
		}

		static void SetOrder(BattleDiceCardUI instance)
		{
			var canvas = instance.gameObject.GetComponent<Canvas>();
			if (canvas != null)
			{
				canvas.sortingOrder = instance.transform.parent.childCount * 2 + 1351;

				// Also fix the Renderer Order of the Book Hunter Mod
				foreach (var renderer in instance.gameObject.GetComponentsInChildren<Renderer>())
				{
					renderer.sortingOrder = canvas.sortingOrder + 1;
				}
			}
		}

		[HarmonyPatch(typeof(UI_BattleExtraCardPreview), "Reset")]
		[HarmonyPatch(typeof(UI_BattleExtraCardPreview), "Update")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> FixSetOrder(IEnumerable<CodeInstruction> codes, MethodBase original)
		{
			// Modify the order after substituting into SortingOrder.
			// (Replacement is difficult, so only insert)
			var targetMethod = AccessTools.Method(typeof(Canvas), "set_sortingOrder");
			var isFirst = true;
			int lastLocalUI = -1;
			var localInfo = original.GetMethodBody().LocalVariables;
			foreach (var code in codes)
			{
				yield return code;

				// Get local variables to support changes to build configurations
				if (code.IsLdloc())
				{
					var index = code.LocalIndex();
					if (index < localInfo.Count && localInfo[index].LocalType == typeof(BattleDiceCardUI))
					{
						lastLocalUI = index;
					}
				}
				if (code.Calls(targetMethod))
				{
					if (lastLocalUI == -1)
					{
						Debug.Log($"(Mod: {Initializer.packageName}) Patch ExtraPreviewPatch.FixSetOrder failed.");
						continue;
					}
					if (isFirst)
					{
						// diff:
						//   battleDiceCardUI.transform.gameObject.GetComponent<Canvas>().sortingOrder = /* any */
						// + battleDiceCardUI.ResetSiblingIndex();
						// + ResetOrder(battleDiceCardUI);
						yield return new CodeInstruction(OpCodes.Ldloc, lastLocalUI);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BattleDiceCardUI), "ResetSiblingIndex"));
						yield return new CodeInstruction(OpCodes.Ldloc, lastLocalUI);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraPreviewPatch), "ResetOrder"));
						isFirst = false;
					}
					else
					{
						// diff:
						//   battleDiceCardUI.transform.gameObject.GetComponent<Canvas>().sortingOrder = /* any */
						// + SetMaxSiblingIndex(battleDiceCardUI);
						// + SetOrder(battleDiceCardUI);
						yield return new CodeInstruction(OpCodes.Ldloc, lastLocalUI);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraPreviewPatch), "SetMaxSiblingIndex"));
						yield return new CodeInstruction(OpCodes.Ldloc, lastLocalUI);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraPreviewPatch), "SetOrder"));
					}
				}
			}

			yield break;
		}

		static void SetMaxSiblingIndex(BattleDiceCardUI __instance)
		{
			__instance.transform.SetSiblingIndex(__instance.transform.parent.childCount - 1);
		}

		[HarmonyPatch(typeof(CardPreviewPatch), "BattleDiceCardUI_ShowDetail_Post")]
		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> FixShowDetailPatch(IEnumerable<CodeInstruction> codes, ILGenerator ilGen)
		{
			// insert at top:
			// if (IsCardSelected()) { return; }
			var skipLabel = ilGen.DefineLabel();
			var nop = new CodeInstruction(OpCodes.Nop);
			nop.labels.Add(skipLabel);
			yield return CodeInstruction.Call(typeof(ExtraPreviewPatch), "IsCardSelected");
			yield return new CodeInstruction(OpCodes.Brfalse_S, skipLabel);
			yield return new CodeInstruction(OpCodes.Ret);
			yield return nop;

			var targetMethod = AccessTools.Method(typeof(CardPreviewPatch), "DisableZoom");
			foreach (var code in codes)
			{
				if (code.Calls(targetMethod))
				{
					// diff:
					// + ResetDefaultIndex(battleDiceCardUI);
					//   CardPreviewPatch.DisableZoom(battleDiceCardUI);
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExtraPreviewPatch), "ResetDefaultIndex"));

				}
				yield return code;
			}

			yield break;
		}

		static bool IsCardSelected()
		{
			return BattleManagerUI.Instance.ui_unitCardsInHand.IsCardSelected();
		}

		static void ResetDefaultIndex(BattleDiceCardUI instantiateUI)
		{
			var defaultIdxInfo = AccessTools.Field(typeof(BattleDiceCardUI), "_defaultIdx");

			instantiateUI.transform.SetSiblingIndex(0);
			defaultIdxInfo.SetValue(instantiateUI, 0);

			foreach (Transform child in instantiateUI.transform.parent)
			{
				if (child.gameObject.name.Contains("Hat_BattleExtraPreviewCard"))
				{
					var childUI = child.GetComponent<BattleDiceCardUI>();
					defaultIdxInfo.SetValue(childUI, (int)defaultIdxInfo.GetValue(childUI) + 1);
				}
			}
		}

		[HarmonyPatch(typeof(UI_BattleExtraCardPreview), "ShowDetails")]
		[HarmonyPostfix]
		static void ShowDetails_Patch(UI_BattleExtraCardPreview __instance)
		{
			__instance.enabled = true;
		}

		[HarmonyPatch(typeof(UI_BattleExtraCardPreview), "HideDetails")]
		[HarmonyPostfix]
		static void HideDetails_Patch(UI_BattleExtraCardPreview __instance)
		{
			__instance.enabled = false;
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "HideDetail")]
		[HarmonyPostfix]
		static void CancelExtraDetail(BattleDiceCardUI __instance)
		{
			if (__instance.gameObject.name.Contains("Hat_BattleExtraPreviewCard")) {
				AccessTools.Field(typeof(CardPreviewPatch), "isPreviewBattleCard").SetValue(null, false);
			}
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "OnPdSubmit")]
		[HarmonyPrefix]
		static bool CancelClick(BattleDiceCardUI __instance)
		{
			// The flashing when vibrating issue causes further problems,
			// so it would be simpler to just disable the click altogether.
			if (__instance.gameObject.name.Contains("Hat_BattleExtraPreviewCard"))
			{
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "SetDefault")]
		[HarmonyPostfix]
		static void SetDefaultExtra(BattleDiceCardUI __instance)
		{
			var component = __instance.gameObject.GetComponent<UI_BattleExtraCardPreview>();
			if (component != null)
			{
				component.HideDetails();
			}
		}
	}
}
