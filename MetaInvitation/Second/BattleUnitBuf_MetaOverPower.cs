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
					Lower = 0;
					Upper = 2;
					break;
				case RelativeFactions.Ally:
					Lower = 0;
					Upper = 4;
					break;
				case RelativeFactions.Enemy:
					Lower = -2;
					Upper = 2;
					break;
				case RelativeFactions.None:
					Destroy();
					break;
			}
		}

		private RelativeFactions mode;
	}
}
