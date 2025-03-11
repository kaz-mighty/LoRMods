using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Mod;

namespace SimpleModListSorter
{
	class ModOrderList
	{
		/// <summary>
		/// Loads a list from a file.
		/// </summary>
		/// <param name="result"></param>
		/// <returns>True if the file was loaded successfully or does not exist; false if an exception occurred.</returns>
		internal static bool LoadFile(out ModOrderList result)
		{
			result = new ModOrderList();
			try
			{
				if (!File.Exists(filePath))
				{
					return true;
				}
				using (var stream = File.OpenText(filePath))
				{
					string line;
					while ((line = stream.ReadLine()) != null)
					{
						if (string.IsNullOrEmpty(line)) { continue; }
						result.list.Add(ModOrderInfo.ParseLine(line));
					}
				}
			}
			catch (Exception e) when (
				e is UnauthorizedAccessException ||
				e is IOException
			)
			{
				Debug.LogException(e);
				Initializer.AddDisplayLog($"Failed to open file. Path:{filePath}", LogType.Warning);
				return false;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				Initializer.AddDisplayLog("Failed to read file.", LogType.Exception);
				return false;
			}
			return true;
		}

		internal bool SaveFile()
		{
			var maxDirectoryLength = 0;
			foreach (var modInfo in list)
			{
				var length = modInfo.directory.Length;
				if (length > maxDirectoryLength)
				{
					maxDirectoryLength = length;
				}
			}

			try
			{
				using (var stream = File.CreateText(filePath))
				{
					foreach (var modInfo in list)
					{
						stream.WriteLine(modInfo.ToLine(maxDirectoryLength));
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				Initializer.AddDisplayLog("Failed to save file.", LogType.Exception);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Update the list with currently existing mods.
		/// 
		/// If the mod isn't in the list, add it to the list.
		/// If the mod is in the list, update the list title.
		/// If the mod doesn't exist but is in the list, leave it as is.
		/// </summary>
		internal void UpdateModList()
		{
			foreach (var modContentInfo in ModContentManager.Instance.GetAllMods())
			{
				var directory = modContentInfo.dirInfo.Name;

				var orderInfo = list.Find(x => x.sourceType == modContentInfo.modType && x.directory == directory);
				if (orderInfo == null)
				{
					list.Add(new ModOrderInfo
					{
						sourceType = modContentInfo.modType,
						directory = directory,
						title = modContentInfo.invInfo.workshopInfo.title,
					});
				}
				else
				{
					orderInfo.title = modContentInfo.invInfo.workshopInfo.title;
				}
			}
		}

		internal void SortModList()
		{
			var allMods = ModContentManager.Instance.GetAllMods();
			var activeMods = new List<ModContentInfo>();
			var unsorted = new List<ModContentInfo>(allMods);
			allMods.Clear();
			foreach (var orderInfo in list)
			{
				var contentInfo = unsorted.Find(x => x.modType == orderInfo.sourceType && x.dirInfo.Name == orderInfo.directory);
				if (contentInfo == null) { continue; }
				unsorted.Remove(contentInfo);
				allMods.Add(contentInfo);
			}
			allMods.AddRange(unsorted);

			foreach (var contentInfo in allMods)
			{
				if (contentInfo.activated)
				{
					activeMods.Add(contentInfo);
				}
			}
			ModContentManager.Instance.SaveSelectionData(allMods, activeMods);
		}

		List<ModOrderInfo> list = new List<ModOrderInfo>();

		internal static readonly string filePath = Path.Combine(Application.dataPath, "Mods", "SimpleModListSorter.txt");

		// Represents information about one Mod, used to sort Mods.
		//
		// The string conversion takes advantage of the fact that
		// directory names cannot end with spaces to improve readability.
		private class ModOrderInfo
		{
			public ModSourceType sourceType = ModSourceType.Error;
			public string directory;
			public string title;

			public static ModOrderInfo ParseLine(string line)
			{
				var fields = line.Split(new char[] { '\t' }, 3);
				var sourceType = ModSourceType.Error;
				switch (fields[0])
				{
					case Steam:
						sourceType = ModSourceType.SteamWorkshop;
						break;
					case Local:
						sourceType = ModSourceType.Local;
						break;
				}
				return new ModOrderInfo
				{
					sourceType = sourceType,
					directory = fields.Count() > 1 ? fields[1].TrimEnd(' ') : "",
					title = fields.Count() > 2 ? fields[2] : "",
				};
			}

			public string ToLine(int maxDirectoryLength)
			{
				var sourceString = "Error";
				switch (sourceType)
				{
					case ModSourceType.SteamWorkshop:
						sourceString = Steam;
						break;
					case ModSourceType.Local:
						sourceString = Local;
						break;
				}
				return string.Join("\t", sourceString, (directory ?? "").PadRight(maxDirectoryLength), title ?? "");
			}

			const string Steam = "Steam";
			const string Local = "Local";
		}
	}
}
