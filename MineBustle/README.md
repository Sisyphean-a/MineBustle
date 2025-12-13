# MineBustle - Yoba's Altar (单一模组版)

## 简介
MineBustle 是一个星露谷物语模组，它添加了一个“由巴的祭坛”，允许玩家通过献祭金币来动态调整矿井怪物的生成倍率。

## 架构说明 (Architecture)
本模组已重构为**单一 SMAPI 模组** (Single Mod Architecture)，不再依赖 Content Patcher。所有资产加载逻辑均由 C# 代码原生处理。

### 关键技术：虚拟路径 (Virtual Paths)
为了在内存中正确链接 TMX 地图文件和 PNG 贴图文件，本模组引入了“虚拟路径”的概念。

- **虚拟路径**: `Mods/MineBustle/AltarTilesheet`
- **对应文件**: `assets/altar4.png`
- **原理**: 
  1. `ModEntry.cs` 中的 `OnAssetRequested` 方法监听该虚拟路径的请求，并提供 `altar4.png` 的数据。
  2. 当 `altar2.tmx` 地图补丁被应用到 `Maps/Mine` 时，代码会自动将其中对 `altar4.png` 的引用重定向到上述虚拟路径。
  
> [!IMPORTANT]
> 如果你需要修改贴图文件名，请务必同时更新 `ModEntry.cs` 中的 `TilesheetVirtualPath` 常量以及 `LocalTexturePath` 常量。

## 安装指南
1. 将 `MineBustle` 文件夹放入 `Stardew Valley/Mods` 目录。
2. 确保 `manifest.json` 中配置正确。
3. 启动游戏。

## 许可证
MIT License
