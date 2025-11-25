using System.IO;
using BepInEx.Configuration;

namespace NoGainExpLimit
{
    internal static class NoGainExpLimitConfig
    {
        internal static ConfigEntry<bool> EnableExpScaling;
        internal static ConfigEntry<bool> EnableOptimization;
        public static string XmlPath { get; private set; }
        public static string TranslationXlsxPath { get; private set; }

        internal static void LoadConfig(ConfigFile config)
        {
            EnableExpScaling = config.Bind(
                section: ModInfo.Name,
                key: "Enable Exp Scaling",
                defaultValue: true,
                description: "If enabled, XP scaling follows vanilla behavior, where remaining XP after leveling up is divided by 2. (Remaining XP = (Previous XP - 1000) / 2)\n" +
                             "If disabled, all XP is used fully without reduction, allowing more levels per instance. (Remaining XP = Previous XP - 1000)\n" +
                             "**Restart the game for changes to take effect.**\n" +
                             "有効にすると、レベルアップ後の残り経験値は2で割られます。(残りXP = (前のXP - 1000) / 2)\n" +
                             "無効にすると、全ての経験値がそのまま使用され、より多くのレベルアップが可能になります。(残りXP = 前のXP - 1000)\n" +
                             "変更を適用するにはゲームを再起動してください。\n" +
                             "启用时，升级后剩余经验值会除以2。(剩余XP = (原XP - 1000) / 2)\n" +
                             "禁用时，所有经验值将被完全使用，从而获得更多等级提升。(剩余XP = 原XP - 1000)\n" +
                             "请重启游戏以应用更改。"
            );
            
            EnableOptimization = config.Bind(
                section: ModInfo.Name,
                key: "Enable Optimization (Experimental)",
                defaultValue: false,
                description: "When enabled, extra levels from massive EXP gains are optimized and processed differently from vanilla behavior.\n" +
                             "This can significantly reduce lag when gaining very large EXP amounts, but some leftover EXP will be lost during processing.\n" +
                             "有効にすると、大量の経験値を一度に獲得した際に、追加のレベルアップ処理が最適化され、バニラとは異なる挙動になります。\n" +
                             "ラグを大幅に軽減できますが、一部の残り経験値が処理中に失われる場合があります。\n" +
                             "启用后，大量经验一次性获取时会使用优化的升级处理方式，与原版行为不同。\n" +
                             "可以显著减少卡顿，不过在此过程中可能会损失少量剩余经验值。"
            );

        }
        
        public static void InitializeXmlPath(string xmlPath)
        {
            if (File.Exists(path: xmlPath))
            {
                XmlPath = xmlPath;
            }
            else
            {
                XmlPath = string.Empty;
            }
        }
        
        public static void InitializeTranslationXlsxPath(string xlsxPath)
        {
            if (File.Exists(path: xlsxPath))
            {
                TranslationXlsxPath = xlsxPath;
            }
            else
            {
                TranslationXlsxPath = string.Empty;
            }
        }
    }
}
