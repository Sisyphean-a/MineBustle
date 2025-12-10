using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MineBustle;

/// <summary>
/// 模组入口类
/// </summary>
public class ModEntry : Mod
{
    /// <summary>
    /// 模组配置
    /// </summary>
    public static ModConfig Config { get; private set; } = null!;

    /// <summary>
    /// 模组监视器（用于日志输出）
    /// </summary>
    public static IMonitor ModMonitor { get; private set; } = null!;

    /// <summary>
    /// 模组辅助工具
    /// </summary>
    public static IModHelper ModHelper { get; private set; } = null!;

    /// <summary>
    /// 祭坛交互处理器
    /// </summary>
    private AltarInteractionHandler? altarHandler;

    /// <summary>
    /// 模组入口方法
    /// </summary>
    public override void Entry(IModHelper helper)
    {
        // 初始化静态引用
        Config = helper.ReadConfig<ModConfig>();
        ModMonitor = Monitor;
        ModHelper = helper;

        // 初始化祭坛交互处理器
        altarHandler = new AltarInteractionHandler(helper, Monitor);
        altarHandler.Register();

        // 注册事件
        helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        helper.Events.GameLoop.DayStarted += OnDayStarted;

        // 应用 Harmony 补丁
        var harmony = new Harmony(ModManifest.UniqueID);
        harmony.PatchAll();

        Monitor.Log("MineBustle - Yoba's Altar 已加载！", LogLevel.Info);
    }

    /// <summary>
    /// 游戏启动时触发
    /// </summary>
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Monitor.Log("游戏已启动，由巴的祭坛已准备就绪。", LogLevel.Debug);

        // 集成 Generic Mod Config Menu
        SetupConfigMenu();
    }

    /// <summary>
    /// 设置 Generic Mod Config Menu
    /// </summary>
    private void SetupConfigMenu()
    {
        // 获取 GMCM API
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
        {
            Monitor.Log("未找到 Generic Mod Config Menu，跳过配置菜单集成。", LogLevel.Debug);
            return;
        }

        // 注册模组
        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );

        // 添加配置选项
        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => "基础费用",
            tooltip: () => "献祭的基础金币费用（默认: 500）",
            getValue: () => (float)Config.BaseFee,
            setValue: value => Config.BaseFee = (int)value,
            min: 0f,
            max: 10000f,
            interval: 50f
        );

        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => "通胀系数",
            tooltip: () => "基于总收入的费用增长系数（默认: 0.0001）",
            getValue: () => (float)Config.InflationCoefficient,
            setValue: value => Config.InflationCoefficient = (double)value,
            min: 0f,
            max: 0.001f,
            interval: 0.00001f,
            formatValue: value => value.ToString("F5")
        );

        configMenu.AddNumberOption(
            mod: ModManifest,
            name: () => "惩罚指数",
            tooltip: () => "倍率增长的惩罚指数（默认: 1.5）",
            getValue: () => (float)Config.PenaltyExponent,
            setValue: value => Config.PenaltyExponent = (double)value,
            min: 1.0f,
            max: 3.0f,
            interval: 0.1f,
            formatValue: value => value.ToString("F1")
        );

        Monitor.Log("Generic Mod Config Menu 集成成功！", LogLevel.Debug);
    }

    /// <summary>
    /// 每天开始时重置倍率
    /// </summary>
    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        Config.CurrentMultiplier = 1.0;
        Monitor.Log("新的一天开始，怪物生成倍率已重置为 1.0x", LogLevel.Debug);
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public static void SaveConfig()
    {
        ModHelper.WriteConfig(Config);
    }
}

