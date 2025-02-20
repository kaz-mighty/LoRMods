using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class BattleUnitBuf_MetaOverPower : BattleUnitBuf
	{
		public BattleUnitBuf_MetaOverPower(RelativeFactions faction)
		{
			stack = 0;
			mode = faction;
			Update();
		}

		protected override string keywordId => MetaInvitation.packageId + "_MetaOverPower";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";
		public override string bufActivatedText =>
			Singleton<BattleEffectTextsXmlList>.Instance.GetEffectTextDesc(keywordId, Lower, Upper);
		public int Lower { get; private set; } = -999;
		public int Upper { get; private set; } = 999;

		public RelativeFactions Mode
		{
			get => mode;
			set
			{
				mode = value;
				Update();
			}
		}

		private void Update()
		{
			switch (mode)
			{
				case RelativeFactions.Ally | RelativeFactions.Enemy:
					Lower = _allyLower;
					Upper = _enemyUpper;
					break;
				case RelativeFactions.Ally:
					Lower = _allyLower;
					Upper = _allyUpper;
					break;
				case RelativeFactions.Enemy:
					Lower = _enemyLower;
					Upper = _enemyUpper;
					break;
				case RelativeFactions.None:
					Destroy();
					break;
			}
		}

		private RelativeFactions mode;

		internal const int _allyLower = 0;
		internal const int _allyUpper = 4;
		internal const int _enemyLower = -2;
		internal const int _enemyUpper = 2;
	}
}
