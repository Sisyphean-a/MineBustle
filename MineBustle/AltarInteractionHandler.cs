using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Microsoft.Xna.Framework;

namespace MineBustle;

/// <summary>
/// 处理祭坛交互的类
/// </summary>
public class AltarInteractionHandler
{
    private readonly IModHelper helper;
    private readonly IMonitor monitor;

    // 祭坛位置配置
    // Mountain: 山区矿井入口
    // UndergroundMine: 矿井内部（未来支持）
    private const string AltarLocationName = "Mountain";
    private static readonly Vector2[] AltarTiles = new[]
    {
        new Vector2(55, 7),  // 祭坛左上（根据玩家点击位置）
        new Vector2(56, 7),  // 祭坛右上
        new Vector2(55, 8),  // 祭坛左下
        new Vector2(56, 8)   // 祭坛右下
    };

    public AltarInteractionHandler(IModHelper helper, IMonitor monitor)
    {
        this.helper = helper;
        this.monitor = monitor;
    }

    /// <summary>
    /// 注册事件处理器
    /// </summary>
    public void Register()
    {
        helper.Events.Input.ButtonPressed += OnButtonPressed;
    }

    /// <summary>
    /// 处理按键事件
    /// </summary>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        // 检查是否在游戏中
        if (!Context.IsWorldReady)
            return;

        // 检查是否是交互按钮
        if (!e.Button.IsActionButton())
            return;

        // 获取当前位置和光标位置
        var location = Game1.currentLocation;
        var tile = e.Cursor.GrabTile;

        // 【调试】显示矿井内部的点击
        if (location.Name.StartsWith("UndergroundMine"))
        {
            monitor.Log($"[调试-矿井] 在 {location.Name} 点击了位置: ({tile.X}, {tile.Y})", LogLevel.Info);
        }

        // 检查是否在祭坛位置
        if (IsAltarLocation(location, tile))
        {
            // 阻止默认交互
            helper.Input.Suppress(e.Button);

            // 打开祭坛菜单
            OpenAltarMenu();
        }
    }

    /// <summary>
    /// 检查是否在祭坛位置
    /// </summary>
    private bool IsAltarLocation(GameLocation location, Vector2 tile)
    {
        // 检查是否在正确的地图
        if (location.Name != AltarLocationName)
            return false;

        // 【调试日志】显示所有在Mountain地图的点击位置
        monitor.Log($"[调试] 在Mountain地图点击了位置: ({tile.X}, {tile.Y})", LogLevel.Info);

        // 检查是否点击了祭坛瓷砖
        foreach (var altarTile in AltarTiles)
        {
            if (tile == altarTile)
            {
                monitor.Log($"[成功] 点击了祭坛位置: ({tile.X}, {tile.Y})，打开菜单！", LogLevel.Info);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 打开祭坛菜单
    /// </summary>
    private void OpenAltarMenu()
    {
        monitor.Log("打开由巴的祭坛菜单", LogLevel.Debug);
        Game1.activeClickableMenu = new AltarMenu();
    }
}

