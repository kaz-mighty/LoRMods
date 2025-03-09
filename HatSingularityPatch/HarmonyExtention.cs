using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;


namespace HatPatch
{
	static class HarmonyExtention
	{
		// Taken from HarmonyX 2.14, adding Localbuilder support
		public static int LocalIndex(this CodeInstruction code)
		{
			if (code.opcode == OpCodes.Ldloc_0 || code.opcode == OpCodes.Stloc_0)
			{
				return 0;
			}
			if (code.opcode == OpCodes.Ldloc_1 || code.opcode == OpCodes.Stloc_1)
			{
				return 1;
			}
			if (code.opcode == OpCodes.Ldloc_2 || code.opcode == OpCodes.Stloc_2)
			{
				return 2;
			}
			if (code.opcode == OpCodes.Ldloc_3 || code.opcode == OpCodes.Stloc_3)
			{
				return 3;
			}
			if (code.opcode == OpCodes.Ldloc_S || code.opcode == OpCodes.Ldloc
				|| code.opcode == OpCodes.Stloc_S || code.opcode == OpCodes.Stloc
				|| code.opcode == OpCodes.Ldloca_S || code.opcode == OpCodes.Ldloca)
			{
				if (code.operand is IConvertible)
				{
					return Convert.ToInt32(code.operand);
				}
				if (code.operand is LocalBuilder)
				{
					return (code.operand as LocalBuilder).LocalIndex;
				}
				throw new ArgumentException("The operand of Instruction is unexpected.", "code");
			}
			throw new ArgumentException("Instruction is not a load or store", "code");
		}
	}
}
