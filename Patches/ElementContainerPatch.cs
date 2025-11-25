using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace NoGainExpLimit
{
    internal static class ElementContainerPatch
    {
        internal static IEnumerable<CodeInstruction> ModExpTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codeMatcher = new CodeMatcher(instructions: instructions);

            bool enableExpScaling = NoGainExpLimitConfig.EnableExpScaling?.Value ?? true;
            
            if (enableExpScaling == true)
            {
                codeMatcher.MatchStartForward(matches: new[]
                {
                    new CodeMatch(opcode: OpCodes.Ldc_I4_0),       // Load constant 0
                    new CodeMatch(opcode: OpCodes.Ldloc_0),        // Load element
                    new CodeMatch(opcode: OpCodes.Callvirt, operand: typeof(Element).GetMethod(name: "get_ExpToNext")),  // Get ExpToNext
                    new CodeMatch(opcode: OpCodes.Ldc_I4_2),       // Load constant 2
                    new CodeMatch(opcode: OpCodes.Div),            // Divide ExpToNext by 2
                    new CodeMatch(opcode: OpCodes.Call, operand: typeof(Mathf).GetMethod(name: "Clamp", types: new[] { typeof(int), typeof(int), typeof(int) })), // Mathf.Clamp call
                });
                
                if (codeMatcher.IsValid)
                {
                    codeMatcher.RemoveInstructions(count: 6);
                }
            }

            if (enableExpScaling == false)
            {
                codeMatcher.MatchStartForward(matches: new[]
                {
                    new CodeMatch(opcode: OpCodes.Ldc_I4_2),  // Load constant 2
                    new CodeMatch(opcode: OpCodes.Div),       // Divide operation
                    new CodeMatch(opcode: OpCodes.Ldc_I4_0),  // Load constant 0
                    new CodeMatch(opcode: OpCodes.Ldloc_0),   // Load local variable 0
                    new CodeMatch(opcode: OpCodes.Callvirt, operand: typeof(Element).GetMethod(name: "get_ExpToNext")),  // Call get_ExpToNext()
                    new CodeMatch(opcode: OpCodes.Ldc_I4_2),  // Load constant 2
                    new CodeMatch(opcode: OpCodes.Div),       // Divide operation
                    new CodeMatch(opcode: OpCodes.Call, operand: typeof(Mathf).GetMethod(name: "Clamp", types: new[] { typeof(int), typeof(int), typeof(int) })) // Mathf.Clamp()
                });
                
                if (codeMatcher.IsValid)
                {
                    codeMatcher.RemoveInstructions(count: 8);
                }
            }
            
            return codeMatcher.Instructions();
        }
        
        internal static void ModExpPostfix(ElementContainer __instance, int ele, float a, bool chain)
        {
            Element element = __instance.GetElement(id: ele);

            if (element == null || !element.CanGainExp)
            {
                return;
            }

            bool enableOptimization = NoGainExpLimitConfig.EnableOptimization?.Value ?? false;

            if (enableOptimization == false)
            {
                if (element.vExp >= element.ExpToNext)
                {
                    __instance.ModExp(ele: element.id, a: 1, chain: chain);
                }

                return;
            }

            bool enableExpScaling = NoGainExpLimitConfig.EnableExpScaling?.Value ?? true;

            int originalBase = element.vBase;
            int totalLevelsGained = 0;

            while (element.vExp >= element.ExpToNext)
            {
                int leftover = element.vExp - element.ExpToNext;

                __instance.ModBase(ele: ele, v: 1);
                totalLevelsGained += 1;

                if (enableExpScaling == true)
                {
                    int scaled = leftover / 2;
                    int max = element.ExpToNext / 2;
                    element.vExp = Mathf.Clamp(value: scaled, min: 0, max: max);
                }
                else
                {
                    element.vExp = leftover;
                }

                if (element.vTempPotential > 0)
                {
                    element.vTempPotential -= element.vTempPotential / 4 + EClass.rnd(a: 5) + 5;
                    if (element.vTempPotential < 0)
                    {
                        element.vTempPotential = 0;
                        break;
                    }
                }
                else if (element.vTempPotential < 0)
                {
                    element.vTempPotential += -element.vTempPotential / 4 + EClass.rnd(a: 5) + 5;
                    if (element.vTempPotential > 0)
                    {
                        element.vTempPotential = 0;
                        break;
                    }
                }
            }

            if (totalLevelsGained > 0)
            {
                __instance.OnLevelUp(e: element, lastValue: originalBase);
            }
        }
    }
}