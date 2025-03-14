using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Mod;
using HarmonyLib;

namespace SimpleModListSorter
{
	public class Initializer : ModInitializer
	{
		public override void OnInitializeMod()
		{
			harmony.PatchAll(typeof(PatchClass));

			var isSuccess = ModOrderList.LoadFile(false, out var modOrder);
			if (isSuccess)
			{
				modOrder.UpdateSelfList();
				isSuccess = modOrder.SaveFile();
			}
			if (isSuccess)
			{
				textProcess = new Process();
				textProcess.StartInfo.FileName = ModOrderList.filePath;
				textProcess.StartInfo.UseShellExecute = true;
				textProcess.Start();
			}
		}

		internal static void OnAllModInitialized()
		{
			if (textProcess == null) { return; }

			Debug.Log($"(Mod: {packageName}) WaitForExit...");
			textProcess.WaitForExit();

			if (ModOrderList.LoadFile(true, out var modOrder))
			{
				modOrder.SortGameList();
				AddDisplayLog("Sorting completed.", LogType.Log);
			}

			textProcess.Dispose();
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
			ModContentManager.Instance.GetErrorLogs().Add(msg);
		}
		private static Process textProcess = null;

		public static readonly string packageName = "Simple Mod List Sorter";
		public static readonly string packageId = "kazmighty.SimpleModListSorter";
		public static readonly Harmony harmony = new Harmony(packageId);
	}
}
