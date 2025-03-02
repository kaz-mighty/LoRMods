using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;
using HarmonyLib;
using Hat_Harmony;

namespace HatPatch
{
	[HarmonyPatch]
	class ReInitializer
	{
		[HarmonyPatch(typeof(TextDataModel), "InitTextData")]
		[HarmonyPostfix]
		static void InittextDataPostFix()
		{
			ReInitialize();
		}

		[HarmonyPatch(typeof(UICardListDetailFilterPopup), "Open")]
		[HarmonyPrefix]
		static void ReInitializeBeforeOpen()
		{
			// Some translations are loaded by BetterFiltersInternals.TryPatchLocalize, so Init must be called afterwards.
			// If there is a LoRLocalizationManager, TryPatchLocalize is called via a delegate from its Update().
			if (_isNeedFilterInit)
			{
				_isNeedFilterInit = false;
				UICardListDetailFilterPopup.Instance.Init();
			}
		}

		static void ReInitialize()
		{
			HatInitializer.HatOriginText.Clear();
			HatInitializer.HatExtraText.Clear();

			HatInitializer.language = TextDataModel.CurrentLanguage;
			AccessTools.Method(typeof(HatInitializer.HatMethod), "OriginalTextAdjust").Invoke(null, null);
			HatInitializer.ExtraLoad.AddOriginKeyword();
			HatInitializer.ExtraLoad.AddEtc();
			HatInitializer.ExtraLoad.AddEffectText();
			XmlPatch.PowerUpFilterFix();
			_isNeedFilterInit = true;
		}

		static bool _isNeedFilterInit;
	}
}
