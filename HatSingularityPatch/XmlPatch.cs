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
using Hat_Xml;
using LOR_XML;

namespace HatPatch
{
	[HarmonyPatch]
	class XmlPatch
	{
		[HarmonyPatch(typeof(HatInitializer.ExtraLoad), "GetAllResources")]
		[HarmonyPostfix]
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

		[HarmonyPatch(typeof(HatInitializer.ExtraLoad), "AddXmls")]
		[HarmonyPostfix]
		internal static void PowerUpFilterFix()
		{
			// To replace text with the Hat prefix, define BattleEffectTextExtra.
			// Copy it to HatOriginText in order to display the filter correctly
			// (with replacement turned off).

			string[] textIds = { "SlashPowerUp", "PenetratePowerUp", "HitPowerUp" };

			foreach (var textId in textIds)
			{
				var extraTextId = "Hat_" + textId;
				var effectTextExtra = BattleEffectTextsXmlList.Instance.GetEffectText(extraTextId) as BattleEffectTextExtra;
				if (effectTextExtra == null)
				{
					continue;
				}
				var effectText = BattleEffectTextsXmlList.Instance.GetEffectText(textId);
				HatInitializer.HatOriginText.Add(new OriginKeywordText
				{
					ID = textId,
					keywordName = effectText.Name,
					keywordDesc = effectText.Desc,
					iconName = effectTextExtra.iconName,
					color = effectTextExtra.iconName,
					hasBracket = effectTextExtra.hasBracket,
					isUnderline = effectTextExtra.isUnderline,
				});
			}
		}
	}
}
