using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Reflection;
using UnityEngine;
using Mod;
using HarmonyLib;
using Hat_Harmony;

namespace HatPatch
{
	[HarmonyPatch]
	class XmlPatch
	{
		[HarmonyPatch(typeof(HatInitializer.ExtraLoad), "GetAllResources")]
		[HarmonyPostfix]
		internal static void ReplaceResourcesPatch()
		{
			ReplaceResources();
		}

		internal static void ReplaceResources()
		{
			var assembly = Assembly.GetExecutingAssembly();
			foreach (var resourceName in assembly.GetManifestResourceNames())
			{
				var splitName = resourceName.Split('.');
				var nameBody = splitName[splitName.Count() - 2];
				var extension = splitName[splitName.Count() - 1];
				try
				{
					switch (extension)
					{
						case "xml":
							var xml = new XmlDocument();
							xml.Load(assembly.GetManifestResourceStream(resourceName));
							HatInitializer.Xmls[nameBody] = xml;
							break;
					}
				} 
				catch (Exception ex)
				{
					Debug.LogException(ex);
					Initializer.AddDisplayLog("Failed to load resource.", LogType.Warning);
				}
			}
		}
	}
}
