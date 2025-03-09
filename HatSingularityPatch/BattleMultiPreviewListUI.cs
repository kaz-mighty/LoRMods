using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hat_Harmony;
using HarmonyLib;


namespace HatPatch
{
	public class BattleMultiPreviewListUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public BattleDiceCardUI ParentUI { get; private set; }
		public List<BattleDiceCardModel> PreviewCardList => new List<BattleDiceCardModel>(_previewCardList);

		public List<BattleDiceCardUI> PreviewUiList => new List<BattleDiceCardUI>(_previewUiList);

		public int CurrentPreviewIndex
		{
			get => _currentPreviewIndex;
			set
			{
				var beforePreviewIndex = _currentPreviewIndex;
				_currentPreviewIndex = value;
				if (_currentPreviewIndex < 0)
				{
					_currentPreviewIndex = _previewCardList.Count - 1;
				}
				else if (_currentPreviewIndex >= _previewCardList.Count)
				{
					_currentPreviewIndex = 0;
				}
				if (_currentPreviewIndex != beforePreviewIndex && _isShowDetail)
				{
					_previewUiList[beforePreviewIndex].HideDetail();
					_previewUiList[CurrentPreviewIndex].ShowDetail();
				}
			}
		}

		public bool IsShowDetail
		{
			get => _isShowDetail;
			set
			{
				_isShowDetail = value;
				if (_isShowDetail)
				{
					_previewUiList[_currentPreviewIndex].ShowDetail();
				}
				else
				{
					_previewUiList[_currentPreviewIndex].HideDetail();
				}
			}
		}
		// Note: Must be set before SetPreviewCards if needed.
		public int BaseSortingOrder { get; set; } = RootSortingOrder + (10 + 1) * 2;

		public void Init(BattleDiceCardUI parentUI)
		{
			// To avoid this being copied, do the first UI clone before SetParent.
			ClonePreviewUI(parentUI);

			var parentVibeRect = (RectTransform)AccessTools.Field(typeof(BattleDiceCardUI), "vibeRect").GetValue(parentUI);
			transform.SetParent(parentVibeRect, false);
			ParentUI = parentUI;
		}

		public void SetPreviewCards(List<BattleDiceCardModel> previewCards)
		{
			_previewCardList = previewCards;
			while (_previewUiList.Count < _previewCardList.Count)
			{
				ClonePreviewUI(_previewUiList[0]);
			}

			for (var i = 0; i < _previewCardList.Count; i++)
			{
				var previewUI = _previewUiList[i];
				previewUI.transform.localPosition = new Vector3(200f - 200f * (_previewCardList.Count * 0.5f - i), 1320f, 0f);
				previewUI.gameObject.SetActive(true);
				previewUI.SetCard(_previewCardList[i]);
				previewUI.SetDefault();
				previewUI.ResetSiblingIndex();
			}
			for (var i = _previewCardList.Count; i < _previewUiList.Count; i++)
			{
				_previewUiList[i].gameObject.SetActive(false);
			}
			CurrentPreviewIndex = 0;
		}

		public void ShowPreview()
		{
			gameObject.SetActive(true);
		}

		public void HidePreview()
		{
			IsShowDetail = false;
			gameObject.SetActive(false);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			IsShowDetail = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			IsShowDetail = false;
		}

		public void Update()
		{
			if (Input.GetKeyUp(KeyCode.Tab))
			{
				HidePreview();
				return;
			}
			if (!IsShowDetail)
			{
				return;
			}
			BattleCamManager.Instance.scrollable = false;
			if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetAxis("Mouse ScrollWheel") <= -0.1f)
			{
				CurrentPreviewIndex += 1;
			}
			if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetAxis("Mouse ScrollWheel") >= 0.1f)
			{
				CurrentPreviewIndex -= 1;
			}
		}

		private void ClonePreviewUI(BattleDiceCardUI from)
		{
			// Awake may not be called
			var childPreviewUI = Instantiate(from, transform);

			AccessTools.Method(typeof(CardPreviewPatch), "DisableZoom").Invoke(null, new object[] { childPreviewUI });
			childPreviewUI.name = ExtraPreviewReplace.HatObjectName + "_" + _previewUiList.Count;
			childPreviewUI.scaleOrigin = Vector3.one * 0.85f;
			childPreviewUI.transform.localScale = childPreviewUI.scaleOrigin;
			childPreviewUI.transform.localRotation = Quaternion.Euler(0f, 0f, -1f);

			/* insert SiblingIndex */
			var defaultIdxInfo = AccessTools.Field(typeof(BattleDiceCardUI), "_defaultIdx");
			childPreviewUI.transform.SetSiblingIndex(0);
			defaultIdxInfo.SetValue(childPreviewUI, 0);
			foreach (var previewUi in _previewUiList)
			{
				defaultIdxInfo.SetValue(previewUi, (int)defaultIdxInfo.GetValue(previewUi) + 1);
			}

			_previewUiList.Add(childPreviewUI);
		}

		bool _isShowDetail = false;
		int _currentPreviewIndex;
		List<BattleDiceCardUI> _previewUiList = new List<BattleDiceCardUI>();
		List<BattleDiceCardModel> _previewCardList;

		// BattleUnitCardsInHandUI Canvas Order == 1350
		internal const int RootSortingOrder = 1350 + 1;
	}
}
