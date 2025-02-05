using System;
using System.Collections.Generic;
using UnityEngine;
using LOR_DiceSystem;

namespace MetaInvitation.Second
{
	class BattleUnitBuf_MetaSmoke : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_MetaSmoke";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";
	}
}
