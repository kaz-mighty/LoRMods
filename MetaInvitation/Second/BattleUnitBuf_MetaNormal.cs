using UnityEngine;

namespace MetaInvitation.Second
{
	public class BattleUnitBuf_MetaNormal : BattleUnitBuf
	{
		protected override string keywordId => MetaInvitation.packageId + "_MetaNormal";
		protected override string keywordIconId => MetaInvitation.packageId + "_PassiveBuf";

		public override string bufActivatedText =>
			BattleEffectTextsXmlList.Instance.GetEffectTextDesc(keywordId, _hpAdder, _breakGageAdder);

		public override void Init(BattleUnitModel owner)
		{
			base.Init(owner);
			stack = 0;
			_hpAdder = Mathf.Max(owner.UnitData.unitData.MaxHp / 10, 10);
			_breakGageAdder = Mathf.Max(owner.UnitData.unitData.Break / 10, 5);
		}

		public override StatBonus GetStatBonus()
		{
			if (IsDestroyed()) { return null; }

			return new StatBonus
			{
				hpAdder = _hpAdder,
				breakGageAdder = _breakGageAdder
			};
		}

		private int _hpAdder = 10;
		private int _breakGageAdder = 5;
	}
}
