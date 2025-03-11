using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Mod;

namespace SimpleModListSorter
{
	public class Initializer : ModInitializer
	{
		public override void OnInitializeMod()
		{
			var isSuccess = ModOrderList.LoadFile(out var modOrder);
			if (isSuccess)
			{
				modOrder.UpdateModList();
				isSuccess = modOrder.SaveFile();
			}
			if (isSuccess)
			{
				var process = new Process();
				process.StartInfo.FileName = ModOrderList.filePath;
				process.StartInfo.UseShellExecute = true;
				process.EnableRaisingEvents = true;
				process.Exited += OnExitedTextEdit;
				process.Start();
			}
		}

		void OnExitedTextEdit(object _sender, EventArgs _e)
		{
			if (ModOrderList.LoadFile(out var modOrder))
			{
				modOrder.SortModList();
				Debug.Log("Mod List Saved.");
			}

			var process = _sender as Process;
			process?.Dispose();
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

		public static readonly string packageName = "Simple Mod List Sorter";
		public static readonly string packageId = "kazmighty.SimpleModListSorter";
	}
}
