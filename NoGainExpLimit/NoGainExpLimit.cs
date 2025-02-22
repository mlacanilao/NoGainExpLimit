using BepInEx;
using HarmonyLib;

namespace NoGainExpLimit
{
    internal static class ModInfo
    {
        internal const string Guid = "omegaplatinum.elin.nogainexplimit";
        internal const string Name = "No Gain Exp Limit";
        internal const string Version = "1.0.0.0";
    }

    [BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
    internal class NoGainExpLimit : BaseUnityPlugin
    {
        internal static NoGainExpLimit Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Harmony.CreateAndPatchAll(type: typeof(Patcher), harmonyInstanceId: ModInfo.Guid);
        }

        internal static void Log(object payload)
        {
            Instance?.Logger.LogInfo(data: payload);
        }
    }
}