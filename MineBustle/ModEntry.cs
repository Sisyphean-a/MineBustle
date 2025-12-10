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

