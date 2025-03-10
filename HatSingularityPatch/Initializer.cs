using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Mod;
using Hat_Harmony;
using ErrorLogCleaner;

namespace HatPatch
{
	public class Initializer : ModInitializer
	{
		// This must be called after HatSingularity is loaded and before it is initialized.
		// Uses the fact that "1FrameworkLoader" loads and adds initialization list in order of file name.
		public override void OnInitializeMod()
		{
			var hatVersion = Assembly.GetAssembly(typeof(HatInitializer)).GetName().Version;
			if (hatVersion != patchTargetVersion)
			{
				switch (GlobalGameManager.Instance.CurrentOption.language)
				{
					case "jp":
						AddDisplayLog("HatSingularityのバージョンが1.0.0.9ではありません。パッチがうまくいかない可能性があります。", LogType.Log);
						break;
					default:
						AddDisplayLog("Your HatSingularity version is not 1.0.0.9, so the patch may not work.", LogType.Log);
						break;
				}
			}

			harmony.PatchAll(Assembly.GetExecutingAssembly());
			LogCleaner.AddCommonAssembly(Assembly.GetExecutingAssembly());
			LogCleaner.CleanUp();
		}

		internal static void AddDisplayLog(string msg, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
				default:
					msg = $"(mod: {packageName}) {msg}";
					break;
				case LogType.Warning:
					msg = $"<color=yellow>(mod: {packageName}) {msg}</color>";
					break;
				case LogType.Log:
					msg = $"<color=green>(mod: {packageName}) {msg}</color>";
					break;
			}
			Debug.unityLogger.Log(type, msg);
			(AccessTools.Field(typeof(ModContentManager), "_logs").GetValue(ModContentManager.Instance) as List<string>).Add(msg);
		}

		public static readonly string packageName = "HatSingularityPatch";
		public static readonly string packageId = "kazmighty.HatSingularityPatch";
		public static Harmony harmony = new Harmony(packageId);

		public static Version patchTargetVersion = new Version(1, 0, 0, 9);
	}
}
