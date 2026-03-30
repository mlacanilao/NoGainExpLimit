using System.IO;
using BepInEx.Configuration;

namespace NoGainExpLimit;

internal static class NoGainExpLimitConfig
{
    internal static ConfigEntry<bool> EnableVanillaOverflowReduction = null!;
    internal static ConfigEntry<bool> EnableOptimization = null!;
    internal static ConfigEntry<bool> EnableLevelUpPresentationSuppression = null!;
    public static string XmlPath { get; private set; } = string.Empty;
    public static string TranslationXlsxPath { get; private set; } = string.Empty;

    internal static void LoadConfig(ConfigFile config)
    {
        EnableVanillaOverflowReduction = config.Bind(
            section: ModInfo.Name,
            key: "Enable Vanilla Overflow Reduction",
            defaultValue: true,
            description: "Controls whether the current continuation step halves remaining raw EXP after each level-up.\n" +
                         "If enabled, the remaining raw EXP is divided by 2 before the next continuation step.\n" +
                         "This greatly reduces repeated multi-level gains from very large EXP gains in a single action and usually feels fairer and closer to vanilla pacing.\n" +
                         "If disabled, the remaining raw EXP is kept in full for the next continuation step, allowing much larger chained level gains per action.\n" +
                         "This is the mod's current raw-overflow behavior and is not the same as vanilla's actual adjusted-overflow handling.\n" +
                         "各レベルアップ後の継続処理で、残っている生EXPを半分にするかどうかを設定します。\n" +
                         "有効にすると、次の継続ステップへ進む前に残りの生EXPが2で割られます。\n" +
                         "これにより、1回の行動で非常に大きなEXPを得た際の連続レベルアップ数を大きく減らし、体感としてはより公平でバニラ寄りになりやすいです。\n" +
                         "無効にすると、次の継続ステップへ残りの生EXPがそのまま引き継がれ、1回の行動でより大きな連続レベルアップが発生します。\n" +
                         "これは現在のMOD側の生EXPベース挙動であり、バニラ本来の調整済みオーバーフロー処理とは同一ではありません。\n" +
                         "控制每次升级后的当前续算步骤是否将剩余的原始EXP减半。\n" +
                         "启用后，在进入下一次续算步骤前，剩余的原始EXP会除以2。\n" +
                         "这会大幅减少单次行动获得超大量EXP时产生的连续多级提升，体感上通常会更公平，也更接近原版节奏。\n" +
                         "禁用后，剩余的原始EXP会完整保留到下一次续算步骤，因此单次行动可能出现更多连续升级。\n" +
                         "这是本MOD当前基于原始EXP的溢出处理方式，并不等同于原版真正的调整后溢出处理。"
        );

        EnableOptimization = config.Bind(
            section: ModInfo.Name,
            key: "Enable Optimization (Experimental)",
            defaultValue: false,
            description: "When enabled, very large EXP gains are handled in a more stable way to help avoid crashes during massive chained level-ups.\n" +
                         "For very large EXP gains in a single action, it is strongly recommended to enable this together with Enable Level-Up Presentation Suppression.\n" +
                         "有効にすると、非常に大きなEXP獲得時の連続レベルアップ処理をより安定させ、極端なケースでのクラッシュ回避に役立ちます。\n" +
                         "1回の行動で非常に大きなEXPを得る場合は、Enable Level-Up Presentation Suppression と一緒に有効にすることを強く推奨します。\n" +
                         "启用后，超大量EXP带来的连续升级会以更稳定的方式处理，有助于避免极端情况下的崩溃。\n" +
                         "对于单次行动获得非常大量EXP的情况，强烈建议与 Enable Level-Up Presentation Suppression 一起启用。"
        );

        EnableLevelUpPresentationSuppression = config.Bind(
            section: ModInfo.Name,
            key: "Enable Level-Up Presentation Suppression",
            defaultValue: false,
            description: "When enabled, extra level-ups from large EXP gains hide level-up sounds and messages to reduce spam and lag.\n" +
                         "This currently suppresses the skill ding sound, ally ding chatter, and pop-up level-up text during chained level-ups.\n" +
                         "Actual level-up effects still run normally.\n" +
                         "For very large EXP gains in a single action, it is strongly recommended to enable this together with Enable Optimization (Experimental).\n" +
                         "有効にすると、大量のEXP獲得で発生する追加のレベルアップ時に、レベルアップ音やメッセージを非表示にして、表示負荷やラグを軽減します。\n" +
                         "現在は、連続したレベルアップ中のスキル上昇音、仲間の反応メッセージ、ポップアップのレベルアップ表示を抑制します。\n" +
                         "実際のレベルアップ効果は通常どおり適用されます。\n" +
                         "1回の行動で非常に大きなEXPを得る場合は、Enable Optimization (Experimental) と一緒に有効にすることを強く推奨します。\n" +
                         "启用后，对于大量EXP带来的额外连续升级，会隐藏升级音效和消息，以减少刷屏和卡顿。\n" +
                         "目前会抑制连续升级时的技能提示音、同伴的升级反应消息，以及弹出的升级文本。\n" +
                         "实际的升级效果仍会正常生效。\n" +
                         "对于单次行动获得非常大量EXP的情况，强烈建议与 Enable Optimization (Experimental) 一起启用。"
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
