using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using HarmonyLib;


namespace HatPatch
{
	[HarmonyPatch]
	class CardUIOrderPatch
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
					insertMethod = AccessTools.Method(typeof(CardUIOrderPatch), "ResetOrder");
					break;
				case "OnClick":
				case "ShowDetail":
					insertMethod = AccessTools.Method(typeof(CardUIOrderPatch), "SetAsLastOrder");
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

		internal static void ResetOrder(BattleDiceCardUI instance)
		{
			if (instance.gameObject.TryGetComponent<Canvas>(out var canvas))
			{
				if (instance.transform.parent.TryGetComponent<BattleMultiPreviewListUI>(out var multiUI))
				{
					canvas.sortingOrder = instance.transform.GetSiblingIndex() * 2 + multiUI.BaseSortingOrder;
				}
				else
				{
					canvas.sortingOrder = instance.transform.GetSiblingIndex() * 2 + BattleMultiPreviewListUI.RootSortingOrder;
				}

				// Also fix the Renderer Order of the Book Hunter Mod
				foreach (var renderer in instance.gameObject.GetComponentsInChildren<Renderer>())
				{
					renderer.sortingOrder = canvas.sortingOrder + 1;
				}
			}
		}

		internal static void SetAsLastOrder(BattleDiceCardUI instance)
		{
			if (instance.gameObject.TryGetComponent<Canvas>(out var canvas))
			{
				if (instance.transform.parent.TryGetComponent<BattleMultiPreviewListUI>(out var multiUI))
				{
					canvas.sortingOrder = instance.transform.parent.childCount * 2 + multiUI.BaseSortingOrder;
				}
				else
				{
					canvas.sortingOrder = instance.transform.parent.childCount * 2 + BattleMultiPreviewListUI.RootSortingOrder;
				}

				// Also fix the Renderer Order of the Book Hunter Mod
				foreach (var renderer in instance.gameObject.GetComponentsInChildren<Renderer>())
				{
					renderer.sortingOrder = canvas.sortingOrder + 1;
				}
			}
		}
	}
}
