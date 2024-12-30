using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using HarmonyLib;

namespace MetaInvitation
{
	class PatchChecker
	{
		public static void CheckPatch(MethodInfo targetMethod)
		{
			string log = string.Format("Patch to {0}.{1}:", targetMethod.DeclaringType.Name, targetMethod.Name);
			var patches = Harmony.GetPatchInfo(targetMethod);
			if (patches is null)
			{
				log += "none.";
				Debug.Log(log);
				return;
			}
			log += "\n";
			log += "  Prefixes:\n";
			PatchDetail(patches.Prefixes, ref log);
			log += "  Postfixes:\n";
			PatchDetail(patches.Postfixes, ref log);
			log += "  Transpilers:";
			PatchDetail(patches.Transpilers, ref log);
			Debug.Log(log);
		}

		static void PatchDetail(IReadOnlyCollection<Patch> patches, ref string log)
		{
			foreach (var patch in patches)
			{
				log += ("    index: " + patch.index + "\n");
				log += ("    owner: " + patch.owner + "\n");
				log += ("    patch method: " + patch.PatchMethod + "\n");
				log += ("    priority: " + patch.priority + "\n");
				log += ("    before: " + patch.before + "\n");
				log += ("    after: " + patch.after + "\n\n");
			}
		}
	}
}
