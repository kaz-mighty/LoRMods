using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;
using HarmonyLib;
using Mod;

namespace SimpleModListSorter
{
	[HarmonyPatch]
	class PatchClass
	{
		[HarmonyPatch(typeof(ModContentManager), "SaveSelectionData")]
		[HarmonyPrefix]
		static void SaveSelectionDataPrefix()
		{
			Initializer.OnAllModInitialized();
		}
	}
}
