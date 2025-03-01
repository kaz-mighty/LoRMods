using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using Mod;

namespace HatPatch
{
    public class Initializer : ModInitializer
    {
		// This must be called after HatSingularity is loaded and before it is initialized.
		// Uses the fact that "1FrameworkLoader" loads and initializes files in order of file name.
		public override void OnInitializeMod()
		{
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		internal static void AddDisplayLog(string msg, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
				default:
					msg = $"(pid: {packageId}) {msg}";
					break;
				case LogType.Warning:
					msg = $"<color=yellow>(pid: {packageId}) {msg}</color>";
					break;
			}
			Debug.unityLogger.Log(type, msg);
			(AccessTools.Field(typeof(ModContentManager), "_logs").GetValue(ModContentManager.Instance) as List<string>).Add(msg);
		}

		public static readonly string packageId = "kazmighty.HatSingularityPatch";
		public static Harmony harmony = new Harmony(packageId);
	}
}
