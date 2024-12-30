using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Mod;
using LOR_DiceSystem;

namespace MetaInvitation
{
	public class MetaInvitation : ModInitializer
	{
		public override void OnInitializeMod()
		{
			foreach (Type type in typeof(PatchClass).GetNestedTypes(AccessTools.all))
			{
				Debug.Log(MetaInvitation.packageId + ": HarmonyPatch " + type.Name);
				harmony.CreateClassProcessor(type).Patch();
			}

			// tmp
			var cardXml = ItemXmlDataList.instance.GetCardItem(timeWaveCardId, false);
			Debug.Log(
				string.Format(
					"TimeWave id: {0}, xml cooltime: {1}",
					cardXml.id,
					cardXml.EgoMaxCooltimeValue
				)
			);

			RemoveError();
		}

		public static void RemoveError()
		{
			Singleton<ModContentManager>.Instance.GetErrorLogs().RemoveAll(
				(string errorLog) => errorLog.Contains(packageId) && errorLog.Contains("The same assembly name already exists")
			);
		}

		public static readonly string packageId = "kazmighty.meta";
		public static Harmony harmony = new Harmony(packageId);

		public static readonly LorId timeWaveCardId = new LorId(packageId, 3);
		public static readonly LorId timeWaveEnemyCardId = new LorId(packageId, 4);
		public static readonly LorId quicklyHandleCardId = new LorId(packageId, 5);
		public static readonly LorId kizunaPassiveId = new LorId(packageId, 4);
		public static readonly LorId disabledEmotionCardStageId = new LorId(packageId, 1);

	}

}
