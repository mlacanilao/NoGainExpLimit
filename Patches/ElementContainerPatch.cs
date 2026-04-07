using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoGainExpLimit;

internal static class ElementContainerPatch
{
    internal sealed class ModExpState
    {
        internal float RemainingRaw { get; set; }
        internal bool EnteredPresentationSuppressionScope { get; set; }
    }

    internal sealed class ContinuationLoopContext
    {
        internal ElementContainer Container { get; set; } = null!;
        internal int ElementId { get; set; }
        internal bool Chain { get; set; }
        internal float PendingRaw { get; set; }
        internal bool HasPending { get; set; }
    }

    [ThreadStatic]
    private static int presentationSuppressionScopeDepth;

    [ThreadStatic]
    private static int onLevelUpDepth;

    [ThreadStatic]
    private static Stack<ContinuationLoopContext>? continuationLoopContexts;

    internal static bool ModExpPrefix(ElementContainer __instance, int ele, ref float a, bool chain, out ModExpState __state)
    {
        __state = new ModExpState();
        float originalRaw = a;

        LogState(tag: "ModExpPrefix", __instance: __instance, ele: ele, a: originalRaw, chain: chain);

        if (a <= 0f)
        {
            return true;
        }

        if (__instance.Card != null && __instance.Card.isChara && __instance.Card.Chara.isDead)
        {
            return true;
        }

        Element element = __instance.GetElement(id: ele);
        if (element == null || !element.CanGainExp)
        {
            return true;
        }

        bool useLevelUpPresentationSuppression = NoGainExpLimitConfig.EnableLevelUpPresentationSuppression.Value;

        int remainingExpToNext = Mathf.Max(0, element.ExpToNext - element.vExp);
        int rawExpToNext = GetRawExpNeededForNextLevel(__instance: __instance, element: element, remainingExpToNext: remainingExpToNext, chain: chain);

        if (rawExpToNext > 0 && a > rawExpToNext)
        {
            __state.RemainingRaw = a - rawExpToNext;
            if (useLevelUpPresentationSuppression == true)
            {
                __state.EnteredPresentationSuppressionScope = true;
                EnterPresentationSuppressionScope();
            }

            a = rawExpToNext;
        }

        return true;
    }

    internal static void ModExpPostfix(ElementContainer __instance, int ele, float a, bool chain, ModExpState __state)
    {
        LogState(tag: "ModExpPostfix", __instance: __instance, ele: ele, a: a, chain: chain);

        bool useVanillaOverflowReduction = NoGainExpLimitConfig.EnableVanillaOverflowReduction.Value;
        float remainingRaw = __state.RemainingRaw;

        if (useVanillaOverflowReduction == true)
        {
            remainingRaw /= 2f;
        }

        if (remainingRaw <= 0.0001f)
        {
            CleanupPresentationSuppressionScope(__state: __state);
            return;
        }

        if (IsOptimizationEnabled() == false)
        {
            try
            {
                __instance.ModExp(ele: ele, a: remainingRaw, chain: chain);
            }
            finally
            {
                CleanupPresentationSuppressionScope(__state: __state);
            }

            return;
        }

        if (TryCaptureContinuation(__instance: __instance, ele: ele, chain: chain, remainingRaw: remainingRaw) == true)
        {
            CleanupPresentationSuppressionScope(__state: __state);
            return;
        }

        try
        {
            RunContinuationLoop(__instance: __instance, ele: ele, chain: chain, initialRaw: remainingRaw);
        }
        finally
        {
            CleanupPresentationSuppressionScope(__state: __state);
        }
    }

    internal static bool OnLevelUpPrefix(out bool enteredScope)
    {
        if (presentationSuppressionScopeDepth <= 0)
        {
            enteredScope = false;
            return true;
        }

        onLevelUpDepth++;
        enteredScope = true;
        return true;
    }

    internal static void OnLevelUpPostfix(bool enteredScope)
    {
        if (enteredScope == false)
        {
            return;
        }

        if (onLevelUpDepth > 0)
        {
            onLevelUpDepth--;
        }
    }

    internal static bool ShouldSuppressOnLevelUpPresentation()
    {
        return presentationSuppressionScopeDepth > 0 && onLevelUpDepth > 0;
    }

    private static void LogState(string tag, ElementContainer __instance, int ele, float a, bool chain)
    {
        Element element = __instance.GetElement(id: ele);

        if (element == null)
        {
            NoGainExpLimit.LogDebug(message: $"[{tag}] ele={ele}, a={a}, chain={chain}, element=null");
            return;
        }

        int remainingExpToNext = Mathf.Max(0, element.ExpToNext - element.vExp);
        int rawExpToNext = GetRawExpNeededForNextLevel(__instance: __instance, element: element, remainingExpToNext: remainingExpToNext, chain: chain);

        NoGainExpLimit.LogDebug(
            $"[{tag}] ele={ele}, a={a}, chain={chain}, canGainExp={element.CanGainExp}, " +
            $"vExp={element.vExp}, expToNext={element.ExpToNext}, remainingExpToNext={remainingExpToNext}, rawExpToNext={rawExpToNext}, " +
            $"vBase={element.vBase}, valueWithoutLink={element.ValueWithoutLink}, potential={element.Potential}, " +
            $"usePotential={element.UsePotential}, useExpMod={element.UseExpMod}, vTempPotential={element.vTempPotential}");
    }

    private static int GetRawExpNeededForNextLevel(ElementContainer __instance, Element element, int remainingExpToNext, bool chain)
    {
        if (remainingExpToNext <= 0)
        {
            return 0;
        }

        float multiplier = 1f;

        if (!chain && __instance.Card != null && __instance.Card.isChara)
        {
            multiplier *= __instance.Card.Chara.GetDaysTogetherBonus() / 100f;
        }

        if (element.UseExpMod)
        {
            int potential = element.UsePotential ? element.Potential : 100;
            int clampedPotential = Mathf.Clamp(value: potential, min: 10, max: 1000);
            int denominator = 100 + Mathf.Max(a: 0, b: element.ValueWithoutLink) * 25;
            multiplier *= (float)clampedPotential / denominator;
        }

        if (multiplier <= 0f)
        {
            return int.MaxValue;
        }

        return Mathf.CeilToInt(f: remainingExpToNext / multiplier);
    }

    private static bool IsOptimizationEnabled()
    {
        return NoGainExpLimitConfig.EnableOptimization.Value;
    }

    private static void RunContinuationLoop(ElementContainer __instance, int ele, bool chain, float initialRaw)
    {
        ContinuationLoopContext context = new ContinuationLoopContext
        {
            Container = __instance,
            ElementId = ele,
            Chain = chain
        };

        PushContinuationLoopContext(context: context);

        try
        {
            float nextRaw = initialRaw;

            while (nextRaw > 0.0001f)
            {
                context.PendingRaw = 0f;
                context.HasPending = false;

                __instance.ModExp(ele: ele, a: nextRaw, chain: chain);

                if (context.HasPending == false)
                {
                    break;
                }

                nextRaw = context.PendingRaw;
            }
        }
        finally
        {
            PopContinuationLoopContext();
        }
    }

    private static bool TryCaptureContinuation(ElementContainer __instance, int ele, bool chain, float remainingRaw)
    {
        ContinuationLoopContext? context = FindContinuationLoopContext(__instance: __instance, ele: ele, chain: chain);
        if (context == null)
        {
            return false;
        }

        context.PendingRaw += remainingRaw;
        context.HasPending = true;
        return true;
    }

    private static ContinuationLoopContext? FindContinuationLoopContext(ElementContainer __instance, int ele, bool chain)
    {
        if (continuationLoopContexts == null || continuationLoopContexts.Count <= 0)
        {
            return null;
        }

        foreach (ContinuationLoopContext context in continuationLoopContexts)
        {
            if (ReferenceEquals(context.Container, __instance) == false)
            {
                continue;
            }

            if (context.ElementId != ele)
            {
                continue;
            }

            if (context.Chain != chain)
            {
                continue;
            }

            return context;
        }

        return null;
    }

    private static void PushContinuationLoopContext(ContinuationLoopContext context)
    {
        if (continuationLoopContexts == null)
        {
            continuationLoopContexts = new Stack<ContinuationLoopContext>();
        }

        continuationLoopContexts.Push(item: context);
    }

    private static void PopContinuationLoopContext()
    {
        if (continuationLoopContexts == null || continuationLoopContexts.Count <= 0)
        {
            return;
        }

        continuationLoopContexts.Pop();

        if (continuationLoopContexts.Count <= 0)
        {
            continuationLoopContexts = null;
        }
    }

    private static void CleanupPresentationSuppressionScope(ModExpState __state)
    {
        if (__state.EnteredPresentationSuppressionScope == false)
        {
            return;
        }

        ExitPresentationSuppressionScope();
    }

    private static void EnterPresentationSuppressionScope()
    {
        presentationSuppressionScopeDepth++;
    }

    private static void ExitPresentationSuppressionScope()
    {
        if (presentationSuppressionScopeDepth > 0)
        {
            presentationSuppressionScopeDepth--;
        }
    }
}
