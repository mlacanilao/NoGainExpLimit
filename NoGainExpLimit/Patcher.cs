using HarmonyLib;

namespace NoGainExpLimit;

internal static class Patcher
{
    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(ElementContainer), methodName: nameof(ElementContainer.ModExp))]
    internal static bool ElementContainerModExpPrefix(ElementContainer __instance, int ele, ref float a, bool chain, out ElementContainerPatch.ModExpState __state)
    {
        return ElementContainerPatch.ModExpPrefix(__instance, ele, ref a, chain, out __state);
    }

    [HarmonyPostfix]
    [HarmonyPatch(declaringType: typeof(ElementContainer), methodName: nameof(ElementContainer.ModExp))]
    internal static void ElementContainerModExpPostfix(ElementContainer __instance, int ele, float a, bool chain, ElementContainerPatch.ModExpState __state)
    {
        ElementContainerPatch.ModExpPostfix(__instance: __instance, ele: ele, a: a, chain: chain, __state: __state);
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(ElementContainerCard), methodName: nameof(ElementContainerCard.OnLevelUp))]
    internal static bool ElementContainerCardOnLevelUpPrefix(out bool __state)
    {
        return ElementContainerPatch.OnLevelUpPrefix(out __state);
    }

    [HarmonyPostfix]
    [HarmonyPatch(declaringType: typeof(ElementContainerCard), methodName: nameof(ElementContainerCard.OnLevelUp))]
    internal static void ElementContainerCardOnLevelUpPostfix(bool __state)
    {
        ElementContainerPatch.OnLevelUpPostfix(enteredScope: __state);
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(ElementContainerZone), methodName: nameof(ElementContainerZone.OnLevelUp))]
    internal static bool ElementContainerZoneOnLevelUpPrefix(out bool __state)
    {
        return ElementContainerPatch.OnLevelUpPrefix(out __state);
    }

    [HarmonyPostfix]
    [HarmonyPatch(declaringType: typeof(ElementContainerZone), methodName: nameof(ElementContainerZone.OnLevelUp))]
    internal static void ElementContainerZoneOnLevelUpPostfix(bool __state)
    {
        ElementContainerPatch.OnLevelUpPostfix(enteredScope: __state);
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(SE), methodName: nameof(SE.DingSkill2))]
    internal static bool SEDingSkill2Prefix()
    {
        return ElementContainerPatch.ShouldSuppressOnLevelUpPresentation() == false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(Point), methodName: nameof(Point.TalkWitnesses))]
    internal static bool PointTalkWitnessesPrefix(string idTalk)
    {
        if (ElementContainerPatch.ShouldSuppressOnLevelUpPresentation() == false)
        {
            return true;
        }

        return idTalk != "ding_other";
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(WidgetPopText), methodName: nameof(WidgetPopText.Say))]
    internal static bool WidgetPopTextSayPrefix()
    {
        return ElementContainerPatch.ShouldSuppressOnLevelUpPresentation() == false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(Msg), methodName: nameof(Msg.SayRaw))]
    internal static bool MsgSayRawPrefix(ref string __result)
    {
        if (ElementContainerPatch.ShouldSuppressOnLevelUpPresentation() == false)
        {
            return true;
        }

        Msg.currentColor = Msg.colors.Default;
        Msg.alwaysVisible = false;
        __result = string.Empty;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(WidgetFeed), methodName: nameof(WidgetFeed.System))]
    internal static bool WidgetFeedSystemPrefix()
    {
        return ElementContainerPatch.ShouldSuppressOnLevelUpPresentation() == false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(WidgetMainText), methodName: nameof(WidgetMainText.Append), argumentTypes: new[] { typeof(string), typeof(UnityEngine.Color), typeof(Point) })]
    internal static bool WidgetMainTextAppendPrefix()
    {
        return ElementContainerPatch.ShouldSuppressOnLevelUpPresentation() == false;
    }
}