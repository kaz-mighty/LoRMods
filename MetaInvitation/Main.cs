using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
				harmony.PatchAll(type);
			}
			SceneManager.sceneLoaded += LatePatch;

			RemoveError();
		}

		public static void RemoveError()
		{
			ModContentManager.Instance.GetErrorLogs().RemoveAll(
				(string errorLog) => errorLog.Contains(packageId) && errorLog.Contains("The same assembly name already exists")
			);
		}

		private static void LatePatch(Scene scene, LoadSceneMode _)
		{
			if (scene.name == "Stage_Hod_New")
			{
				SceneManager.sceneLoaded -= LatePatch;
				if (!Harmony.HasAnyPatches("LOR.HatSingularity"))
				{
					harmony.PatchAll(typeof(ManualPatch.AddPositiveBufText));
				}
			}
		}

		public static readonly string packageId = "kazmighty.meta";
		public static Harmony harmony = new Harmony(packageId);

		public static readonly LorId timeWaveCardId = new LorId(packageId, 3);
		public static readonly LorId timeWaveEnemyCardId = new LorId(packageId, 4);
		public static readonly LorId quicklyHandleCardId = new LorId(packageId, 5);
		public static readonly LorId kizunaPassiveId = new LorId(packageId, 4);
		public static readonly LorId timeSub2PassiveId = new LorId(packageId, 9);
		public static readonly LorId disabledEmotionCardStageId = new LorId(packageId, 1);
		// Also in MetaTactics
	}

	public static class Util
	{
		// positive only
		public static int CeilDiv(this int a, int b)
		{
			return (a + b - 1) / b;
		}

		// positive only
		public static int RandomRoundDiv(this int a, int b)
		{
			return (a + RandomUtil.Range(0, b - 1)) / b;
		}

		public static T GetActivatedBuf<T>(this BattleUnitBufListDetail self)
			where T : class
		{
			return self.GetActivatedBufList().Find(buf => buf is T && !buf.IsDestroyed()) as T;
		}
	}
}
