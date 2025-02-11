using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class DiceCardSelfAbility_kaz_DiverseTactics : DiceCardSelfAbilityBase
	{
		public override string[] Keywords => new string[] {
			"kaz_DiverseTactics_Desc1",
			"kaz_DiverseTactics_Desc2",
			"kaz_DiverseTactics_Desc3",
			"kaz_DiverseTactics_Desc4"
		};

		public override void OnUseCard()
		{
			owner.bufListDetail.AddBuf(new HandExtend(round));

			var handIdSet = new HashSet<LorId>(owner.allyCardDetail.GetAllDeck().Select(x => x.GetID()));
			var candidates = GetCandidateCards();
			var choice = RandomUtil.SelectOne(candidates);
			var newCard = owner.allyCardDetail.AddNewCard(choice);
			if (newCard != null)
			{
				// xmlData is copied (except for DiceBehaviour) when the card is generated, so it can be rewritten.
				newCard.XmlData.optionList.Add(CardOption.ExhaustOnUse);
				newCard.AddBuf(new DiverseTacticsCard(round));
				Debug.Log(MetaInvitation.packageId + ": DicerseTactics Generate Page " + newCard.GetName());
			}
		}

		List<LorId> GetCandidateCards()
		{
			var handIdSet = new HashSet<LorId>(owner.allyCardDetail.GetAllDeck().Select(x => x.GetID()));
			if (owner.faction == Faction.Player)
			{
				var candidates = InventoryModel.Instance.GetCardList().ConvertAll(x => x.ClassInfo);
				candidates.RemoveAll(x => x.Chapter <= 5 || !IsCandidate(x));
				if (!logAlly)
				{
					candidates.Sort((x, y) => x.id.id - y.id.id);
					var list = candidates.ConvertAll(x =>
						string.Format("{0} ({1})", x.id, x.Name)
					);
					Debug.Log(MetaInvitation.packageId + ": DiverseTactics Card List by Ally");
					Debug.Log(string.Join("\n", list));
					logAlly = true;
				}
				candidates.RemoveAll(x => handIdSet.Contains(x.id));
				return candidates.ConvertAll(x => x.id);
			}
			else
			{
				if (_enemyCache == null)
				{
					var cache = GetEnemyInventory();
					cache.RemoveAll(x => !IsCandidate(x));

					cache.Sort((x, y) => x.id.id - y.id.id);
					var list = cache.ConvertAll(x =>
						string.Format("{0} ({1})", x.id, x.Name)
					);
					Debug.Log(MetaInvitation.packageId + ": DiverseTactics Card List by Emeny");
					Debug.Log(string.Join("\n", list));

					_enemyCache = cache.ConvertAll(x => x.id);
				}
				var candidates = new List<LorId>(_enemyCache);
				candidates.RemoveAll(x => handIdSet.Contains(x));
				return candidates;
			}
		}

		static List<DiceCardXmlInfo> GetEnemyInventory()
		{
			var cardSet = new HashSet<LorId>();
			foreach (var bookId in _StarOfCity1and2Book)
			{
				var book = DropBookXmlList.Instance.GetData(bookId);
				foreach (var drop in book.DropItemList)
				{
					if (drop.itemType == DropItemType.Equip)
					{
						continue;
					}
					cardSet.Add(drop.id);
				}
			}
			return cardSet.Select(x => ItemXmlDataList.instance.GetCardItem(x)).ToList();

		}

		static bool IsCandidate(DiceCardXmlInfo card)
		{
			if (card.id.IsWorkshop())
			{
				return false;
			}
			var range = card.Spec.Ranged;
			if (range == CardRange.FarArea || range == CardRange.FarAreaEach)
			{
				return false;
			}
			if (pageAddPage.Contains(card.id.id))
			{
				return false;
			}
			return true;
		}

		const int round = 3;

		static bool logAlly = false;

		static List<LorId> _enemyCache = null;

		static readonly HashSet<int> pageAddPage = new HashSet<int>() {
			601008, // Forming Storm, 押し寄せる
			602001, // Reload, 装填
			602002, // Bayonet Combat, 銃剣術
			605008, // Unlock-Ⅰ, 解禁-Ⅰ
			608016, // Clone, 複製
			608004, // Disposal, 処分
			608017, // Savage Mode, 狂暴化
			616006, // Ready Position, 準備姿勢
			705011, // Crescendo, クレッシェンド
			705021, // Flames of Despair, 絶望の炎
			705013, // Deluge of Brachial Quietuses, 押し寄せる安息の腕
			705014, // Unending Thirst, 永遠なる渇き
			705015, // Straining Strings, 拘束の糸
			705017, // CLIMAX~!!!!, クライマックス～！！！
			705019, // Chorus at the Climax, 絶頂の合唱
			705020, // Thought Gear: Indoctrinate, 考えの歯車：洗礼
		};

		static readonly int[] _StarOfCity1and2Book =
		{
			243005,
			250001,
			250002,
			250003,
			250004,
			250005,
			250006,
			250007,
			250008,
			250009,
			250010,
			250011,
			250012,
			250013,
			250014,
			250015,
			250016,
			250017,
			250018,
			250019,
			250020,
			250021,
			250022,
			250023,
			250024,
			250025,
			250026,
			250027,
			250028,
			250029,
			252001,
			252002,
			253001,
			254001,
			254002,
			255001,
			255002,
			256001,
			256002,
		};

		class DiverseTacticsCard : BattleDiceCardBuf
		{
			public DiverseTacticsCard(int round)
			{
				_round = round;
			}

			public override int GetCost(int oldCost)
			{
				return oldCost - 1;
			}

			public override void OnRoundEnd()
			{
				_round--;
				if (_round <= 0)
				{
					// exhaust
					_card.owner.allyCardDetail.ExhaustACardAnywhere(_card);
				}
			}

			int _round;
		}

		class HandExtend : BattleUnitBuf
		{
			public HandExtend(int round)
			{
				_round = round;
			}

			public override bool Hide => true;

			public override void Init(BattleUnitModel owner)
			{
				base.Init(owner);
				if (!_isInit)
				{
					var maxDrawHand = (int)AccessTools.Field(typeof(BattleAllyCardDetail), "_maxDrawHand").GetValue(_owner.allyCardDetail);
					_owner.allyCardDetail.SetMaxDrawHand(maxDrawHand + stack);
					_owner.allyCardDetail.SetMaxHand(_owner.allyCardDetail.maxHandCount + stack);
					_isInit = true;
				}
			}

			public override void OnRoundEnd()
			{
				_round--;
				if (_round <= 0)
				{
					Destroy();
				}
			}
			public override void Destroy()
			{
				var maxDrawHand = (int)AccessTools.Field(typeof(BattleAllyCardDetail), "_maxDrawHand").GetValue(_owner.allyCardDetail);
				_owner.allyCardDetail.SetMaxDrawHand(maxDrawHand - stack);
				_owner.allyCardDetail.SetMaxHand(_owner.allyCardDetail.maxHandCount - stack);
				base.Destroy();
			}

			int _round;
			bool _isInit;
		}
	}
}
