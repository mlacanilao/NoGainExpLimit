using System;
using System.Linq;
using BepInEx;
using HarmonyLib;

namespace NoGainExpLimit
{
    internal static class ModInfo
    {
        internal const string Guid = "omegaplatinum.elin.nogainexplimit";
        internal const string Name = "No Gain Exp Limit";
        internal const string Version = "1.1.1.0";
        internal const string ModOptionsGuid = "evilmask.elinplugins.modoptions";
        internal const string ModOptionsAssemblyName = "ModOptions";
    }

    [BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
    internal class NoGainExpLimit : BaseUnityPlugin
    {
        internal static NoGainExpLimit Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            NoGainExpLimitConfig.LoadConfig(config: Config);
            Harmony.CreateAndPatchAll(type: typeof(Patcher), harmonyInstanceId: ModInfo.Guid);
            
            if (IsModOptionsInstalled())
            {
                try
                {
                    UIController.RegisterUI();
                }
                catch (Exception ex)
                {
                    Log(payload: $"An error occurred during UI registration: {ex.Message}");
                }
            }
            else
            {
                Log(payload: "Mod Options is not installed. Skipping UI registration.");
            }
        }

        internal static void Log(object payload)
        {
            Instance?.Logger.LogInfo(data: payload);
        }
        
        private bool IsModOptionsInstalled()
        {
            try
            {
                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Any(predicate: assembly => assembly.GetName().Name == ModInfo.ModOptionsAssemblyName);
            }
            catch (Exception ex)
            {
                Log(payload: $"Error while checking for Mod Options: {ex.Message}");
                return false;
            }
        }
    }
}