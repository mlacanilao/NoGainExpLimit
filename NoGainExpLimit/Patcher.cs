using System.Collections.Generic;
using HarmonyLib;

namespace NoGainExpLimit
{
    internal class Patcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(declaringType: typeof(ElementContainer), methodName: nameof(ElementContainer.ModExp))]
        internal static void ElementContainerModExpPostfix(ElementContainer __instance, int ele, float a, bool chain)
        {
            ElementContainerPatch.ModExpPostfix(__instance: __instance, ele: ele, a: a, chain: chain);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(declaringType: typeof(ElementContainer), methodName: nameof(ElementContainer.ModExp))]
        internal static IEnumerable<CodeInstruction> ElementContainerModExpTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return ElementContainerPatch.ModExpTranspiler(instructions: instructions);
        }
    }
}