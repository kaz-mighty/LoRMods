using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Hat_Harmony;
using Hat_Method;

namespace HatPatch
{
	// Since there are many modifications and it is difficult, I replaced the original method completely.
	// 修正箇所が多くて大変なので、元のメソッドを完全に置き換えた。

	// When I tried to reuse a single GameObject, SetParent was too heavy,
	// so I ended up creating a separate GameObject for each card in my hand, just like in the original.
	[HarmonyPatch]
	class ExtraPreviewReplace
	{
		[HarmonyPatch(typeof(HatInitializer), "OnInitializeMod")]
		[HarmonyPostfix]
		static void CancelPatch()
		{
			Initializer.harmony.Unpatch(typeof(BattleDiceCardUI).GetMethod("ShowDetail"), HarmonyPatchType.Postfix, "LOR.HatSingularity");
			Initializer.harmony.Unpatch(typeof(BattleDiceCardUI).GetMethod("HideDetail"), HarmonyPatchType.Postfix, "LOR.HatSingularity");
			Initializer.harmony.Unpatch(typeof(BattleDiceCardUI).GetMethod("OnClick"), HarmonyPatchType.Postfix, "LOR.HatSingularity");

			// It will work without Canvas, but other mods may be using Canvas, so leave it as is.
			// Initializer.harmony.Unpatch(typeof(BattleUnitCardsInHandUI).GetMethod("UpdateCardList"), HarmonyPatchType.Postfix, "LOR.HatSingularity");
		}

		static List<BattleDiceCardModel> GetBattlePreviewCardModels(BattleDiceCardModel cardModel)
		{
			var previewCards = new List<BattleDiceCardModel>();
			if (CardPreviewPatch.PreviewCardList.TryGetValue(cardModel.GetID(), out var value))
			{
				value.ForEach(x => previewCards.Add(BattleDiceCardModel.CreatePlayingCard(ItemXmlDataList.instance.GetCardItem(x))));
			}
			var diceCardSelfAbilityBase = cardModel.CreateDiceCardSelfAbilityScript();
			if (diceCardSelfAbilityBase is IHat_DiceCardSelfAbility hat_DiceCardSelfAbility)
			{
				if (cardModel.owner != null)
				{
					var hatProxy = hat_DiceCardSelfAbility.GetHatProxy();
					if (hatProxy.DontShowOriginalPreviewCard(cardModel.owner))
					{
						previewCards.Clear();
					}
					previewCards.AddRange(hatProxy.ShowPreviewCard(cardModel.owner));
					hatProxy.ShowPreviewCardXml(cardModel.owner).ForEach(
						x => previewCards.Add(BattleDiceCardModel.CreatePlayingCard(x))
					);
					hatProxy.ShowPreviewCardId(cardModel.owner).ForEach(
						x => previewCards.Add(BattleDiceCardModel.CreatePlayingCard(ItemXmlDataList.instance.GetCardItem(x)))
					);
				}
			}
			return previewCards;
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "ShowDetail")]
		[HarmonyPostfix]
		static void UpdateMultiPreviewList(BattleDiceCardUI __instance)
		{
			if (__instance.isProfileCard)
			{
				return;
			}
			if (BattleManagerUI.Instance.ui_unitCardsInHand.IsCardSelected())
			{
				return;
			}
			if (__instance.CardModel == null)
			{
				return;
			}
			if (__instance.gameObject.name.Contains(HatObjectName))
			{
				return;
			}

			var previewCardList = GetBattlePreviewCardModels(__instance.CardModel);
			if (previewCardList.Count <= 0)
			{
				return;
			}
			if (!previewListUIDict.TryGetValue(__instance, out var previewListUI))
			{
				var gameObject = new GameObject("PreviewListUI", typeof(RectTransform));
				previewListUI = gameObject.AddComponent<BattleMultiPreviewListUI>();
				previewListUI.Init(__instance);
				previewListUIDict.Add(__instance, previewListUI);
			}
			previewListUI.SetPreviewCards(previewCardList);
			previewListUI.BaseSortingOrder = __instance.transform.parent.childCount * 2 + CardUIOrderPatch.BaseSortingOrder + 2;
			previewListUI.ShowPreview();
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "HideDetail")]
		[HarmonyPostfix]
		static void HideDetail_Patch(BattleDiceCardUI __instance)
		{
			if (__instance.isProfileCard)
			{
				return;
			}
			if (BattleManagerUI.Instance.ui_unitCardsInHand.IsCardSelected())
			{
				return;
			}
			if (previewListUIDict.TryGetValue(__instance, out var previewListUI))
			{
				previewListUI.HidePreview();
			}
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "OnPointerEnter")]
		[HarmonyPatch(typeof(BattleDiceCardUI), "OnPointerExit")]
		[HarmonyPatch(typeof(BattleDiceCardUI), "OnPdSubmit")]
		[HarmonyPrefix]
		static bool CancelEvent(BattleDiceCardUI __instance)
		{
			// Disable events to the preview BattleDiceCardUI and process it all in BattleMultiPreviewListUI.
			if (__instance.gameObject.name.Contains(HatObjectName))
			{
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(BattleDiceCardUI), "OnClick")]
		[HarmonyPatch(typeof(BattleDiceCardUI), "SetDefault")]
		[HarmonyPostfix]
		public static void EndPreview(BattleDiceCardUI __instance)
		{
			if (previewListUIDict.TryGetValue(__instance, out var previewListUI))
			{
				previewListUI.HidePreview();
			}
		}

		public static Dictionary<BattleDiceCardUI, BattleMultiPreviewListUI> previewListUIDict = 
			new Dictionary<BattleDiceCardUI, BattleMultiPreviewListUI>();

		public const string HatObjectName = "HatPatch_MultiPreviewCard";
	}
}
