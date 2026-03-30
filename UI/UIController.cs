using System.IO;
using System.Reflection;
using EvilMask.Elin.ModOptions;
using EvilMask.Elin.ModOptions.UI;

namespace NoGainExpLimit;

public static class UIController
{
    public static void RegisterUI()
    {
        var controller = ModOptionController.Register(guid: ModInfo.Guid, tooptipId: "mod.tooltip");
        var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var xmlPath = Path.Combine(path1: assemblyLocation, path2: "NoGainExpLimitConfig.xml");
        var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");

        NoGainExpLimitConfig.InitializeXmlPath(xmlPath: xmlPath);
        NoGainExpLimitConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);

        if (File.Exists(path: NoGainExpLimitConfig.XmlPath))
        {
            controller.SetPreBuildWithXml(xml: File.ReadAllText(path: NoGainExpLimitConfig.XmlPath));
        }

        if (File.Exists(path: NoGainExpLimitConfig.TranslationXlsxPath))
        {
            controller.SetTranslationsFromXslx(path: NoGainExpLimitConfig.TranslationXlsxPath);
        }

        RegisterEvents(controller: controller);
    }

    private static void RegisterEvents(ModOptionController controller)
    {
        controller.OnBuildUI += builder =>
        {
            var enableVanillaOverflowReductionToggle = builder.GetPreBuild<OptToggle>(id: "enableVanillaOverflowReductionToggle");
            enableVanillaOverflowReductionToggle.Checked = NoGainExpLimitConfig.EnableVanillaOverflowReduction.Value;
            enableVanillaOverflowReductionToggle.OnValueChanged += isChecked =>
            {
                NoGainExpLimitConfig.EnableVanillaOverflowReduction.Value = isChecked;
            };
            
            var enableOptimizationToggle = builder.GetPreBuild<OptToggle>(id: "enableOptimizationToggle");
            enableOptimizationToggle.Checked = NoGainExpLimitConfig.EnableOptimization.Value;
            enableOptimizationToggle.OnValueChanged += isChecked =>
            {
                NoGainExpLimitConfig.EnableOptimization.Value = isChecked;
            };
            
            var enableLevelUpPresentationSuppressionToggle = builder.GetPreBuild<OptToggle>(id: "enableLevelUpPresentationSuppressionToggle");
            enableLevelUpPresentationSuppressionToggle.Checked = NoGainExpLimitConfig.EnableLevelUpPresentationSuppression.Value;
            enableLevelUpPresentationSuppressionToggle.OnValueChanged += isChecked =>
            {
                NoGainExpLimitConfig.EnableLevelUpPresentationSuppression.Value = isChecked;
            };
        };
    }
}
