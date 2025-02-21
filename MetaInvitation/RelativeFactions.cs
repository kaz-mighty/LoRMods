using System;

namespace MetaInvitation
{
	[Flags]
	public enum RelativeFactions
	{
		None = 0,
		Ally = 1 << 0,
		Enemy = 1 << 1,
	}
}
