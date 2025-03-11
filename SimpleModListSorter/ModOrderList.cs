using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Mod;

namespace SimpleModListSorter
{
	using ModContentDict = Dictionary<(ModSourceType, string), ModContentInfo>;

	public class ModOrderList
	{
		/// <summary>
		/// Load the list from a file.
		/// </summary>
		/// <param name="result">A new ModOrderList instance</param>
		/// <returns>True if the file was loaded successfully or does not exist; false if an exception occurred.</returns>
		public static bool LoadFile(bool mustExist, out ModOrderList result)
		{
			result = new ModOrderList();
			var modContentDict = GetAllModDictionary();
			try
			{
				if (!File.Exists(filePath))
				{
					if (mustExist)
					{
						Initializer.AddDisplayLog($"File does not exist. Path:{filePath}", LogType.Warning);
					}
					return !mustExist;
				}
				using (var stream = File.OpenText(filePath))
				{
					string line;
					while ((line = stream.ReadLine()) != null)
					{
						if (string.IsNullOrEmpty(line)) { continue; }
						result.list.Add(ModOrderInfo.ParseLine(line, modContentDict));
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
			result.list = new List<ModOrderInfo>(result.list.OrderBy(x => x.priority));
			return true;
		}

		/// <summary>
		/// Save the list to a file.
		/// </summary>
		/// <returns>true if successful, false if failed</returns>
		public bool SaveFile()
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
		/// If the mod is in the list, add a title only if it is blank.
		/// If the mod doesn't exist but is in the list, leave it as is.
		/// If there are multiple mods with the same ID, they will be sorted by mod order in the game.
		/// </summary>
		public void UpdateSelfList()
		{
			var matched = new HashSet<ModOrderInfo>();

			foreach (var modContentInfo in ModContentManager.Instance.GetAllMods())
			{
				var modId = modContentInfo.invInfo.workshopInfo.uniqueId;
				var directory = modContentInfo.dirInfo.Name;

				var idMatchIndex = list.FindIndex(x => x.modId == modId && !matched.Contains(x));
				var orderInfoIndex = list.FindIndex(x => x.sourceType == modContentInfo.modType && x.directory == directory);
				if (orderInfoIndex == -1)
				{
					var newOrderInfo = new ModOrderInfo
					{
						sourceType = modContentInfo.modType,
						directory = directory,
						title = modContentInfo.invInfo.workshopInfo.title,
						modId = modId,
						priority = specialMods.Count(),
					};
					list.Add(newOrderInfo);
					matched.Add(newOrderInfo);
				}
				else
				{
					var orderInfo = list[orderInfoIndex];
					matched.Add(orderInfo);
					if (string.IsNullOrWhiteSpace(orderInfo.title))
					{
						orderInfo.title = modContentInfo.invInfo.workshopInfo.title;
					}
					// Sorting when there are multiple same Mod IDs
					if (idMatchIndex != orderInfoIndex)
					{
						var tmp = list[idMatchIndex];
						list[idMatchIndex] = orderInfo;
						list[orderInfoIndex] = tmp;
					}

				}
			}
		}

		/// <summary>
		/// Sort and save the game's mod list based on this list.
		/// </summary>
		public void SortAndSaveGameList()
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

		private static ModContentDict GetAllModDictionary()
		{
			var allModsDict = new ModContentDict();
			foreach (var contentInfo in ModContentManager.Instance.GetAllMods())
			{
				var directory = contentInfo.dirInfo.Name;
				allModsDict[(contentInfo.modType, directory)] = contentInfo;
			}
			return allModsDict;
		}

		List<ModOrderInfo> list = new List<ModOrderInfo>();

		public static readonly string filePath = Path.Combine(Application.dataPath, "Mods", "SimpleModListSorter.txt");
		private static readonly string[] specialMods = new string[]
		{
			"1FrameworkPriorityLoader",
			"BaseMod"
		};

		// Represents information about one Mod, used to sort Mods.
		//
		// The string conversion takes advantage of the fact that
		// directory names cannot end with spaces to improve readability.
		private class ModOrderInfo
		{
			public ModSourceType sourceType = ModSourceType.Error;
			public string directory;
			public string title;
			public string modId;
			public int priority;

			public static ModOrderInfo ParseLine(string line, ModContentDict modContentDict)
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
				var orderInfo = new ModOrderInfo
				{
					sourceType = sourceType,
					directory = fields.Count() > 1 ? fields[1].TrimEnd(' ') : "",
					title = fields.Count() > 2 ? fields[2] : "",
					priority = specialMods.Count(),
				};

				if (modContentDict == null)
				{
					return orderInfo;
				}
				if (modContentDict.TryGetValue((orderInfo.sourceType, orderInfo.directory), out var contentInfo))
				{
					var modId = contentInfo.invInfo.workshopInfo.uniqueId;
					var priority = Array.IndexOf(specialMods, modId);
					orderInfo.modId = modId;
					if (priority != -1) { orderInfo.priority = priority; }
				}
				return orderInfo;
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
