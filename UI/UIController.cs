using System.IO;
using System.Reflection;
using BepInEx;
using EvilMask.Elin.ModOptions;
using EvilMask.Elin.ModOptions.UI;

namespace NoGainExpLimit
{
    public class UIController
    {
        public static void RegisterUI()
        {
            foreach (var obj in ModManager.ListPluginObject)
            {
                if (obj is BaseUnityPlugin plugin && plugin.Info.Metadata.GUID == ModInfo.ModOptionsGuid)
                {
                    var controller = ModOptionController.Register(guid: ModInfo.Guid, tooptipId: "mod.tooltip");
                    
                    var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location);
                    var xmlPath = Path.Combine(path1: assemblyLocation, path2: "NoGainExpLimitConfig.xml");
                    NoGainExpLimitConfig.InitializeXmlPath(xmlPath: xmlPath);
            
                    var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");
                    NoGainExpLimitConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);
                    
                    if (File.Exists(path: NoGainExpLimitConfig.XmlPath))
                    {
                        using (StreamReader sr = new StreamReader(path: NoGainExpLimitConfig.XmlPath))
                            controller.SetPreBuildWithXml(xml: sr.ReadToEnd());
                    }
                    
                    if (File.Exists(path: NoGainExpLimitConfig.TranslationXlsxPath))
                    {
                        controller.SetTranslationsFromXslx(path: NoGainExpLimitConfig.TranslationXlsxPath);
                    }
                    
                    RegisterEvents(controller: controller);
                }
            }
        }

        private static void RegisterEvents(ModOptionController controller)
        {
            controller.OnBuildUI += builder =>
            {
                var enableExpScalingToggle = builder.GetPreBuild<OptToggle>(id: "enableExpScalingToggle");
                enableExpScalingToggle.Checked = NoGainExpLimitConfig.EnableExpScaling.Value;
                enableExpScalingToggle.OnValueChanged += isChecked =>
                {
                    NoGainExpLimitConfig.EnableExpScaling.Value = isChecked;
                };
            };
        }
    }
}