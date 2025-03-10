using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;
using HarmonyLib;
using Hat_Harmony;

namespace HatPatch
{
	[HarmonyPatch]
	class OnlyOneFilterPatch
	{
		[HarmonyPatch(typeof(SpecialTextPatch), "TryGetFilterBufName")]
		[HarmonyPrefix]
		internal static bool TryGetFilterBufName_Prefix(ref bool __result, string keyword, ref string name)
		{
			if (keyword == "OnlyOne_Keyword")
			{
				name = BattleEffectTextsXmlList.Instance.GetEffectTextName(keyword);
				__result = !string.IsNullOrWhiteSpace(name);
				return false;
			}
			return true;
		}

	}
}
