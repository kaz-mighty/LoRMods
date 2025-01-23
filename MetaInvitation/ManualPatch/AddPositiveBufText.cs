using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using LOR_DiceSystem;

namespace MetaInvitation.ManualPatch
{
	class AddPositiveBufText
	{
		[HarmonyPatch(typeof(BattleUnitBuf), "GetAddTextData")]
		[HarmonyPostfix]
		static void Patch(BattleUnitBuf __instance, ref string __result)
		{
			if (__instance.positiveType == BufPositiveType.Positive)
			{
				__result += " \n<color=#33CCFF>(" + TextDataModel.GetText("BattleUI_buf", Array.Empty<object>()) + ")</color>";
			}
		}
	}
}
