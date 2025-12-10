# Content Patcher 配置说明

## 关于由巴祭坛的贴图

根据你的描述，游戏中皮埃尔杂货店里已经有一个由巴的祭坛。我们需要找到这个祭坛的贴图并复用它。

## 如何找到祭坛贴图

### 方法1: 使用Tiled查看SeedShop地图

1. 安装 [Tiled Map Editor](https://www.mapeditor.org/)
2. 解包游戏内容（使用xnbcli或其他工具）
3. 打开 `Content/Maps/SeedShop.tmx` 或 `SeedShop.tbin`
4. 查看Buildings层，找到坐标 (3,17) 附近的祭坛
5. 记录使用的tileset和tile index

### 方法2: 使用游戏内调试工具

1. 安装 [Debug Mode](https://www.nexusmods.com/stardewvalley/mods/679)
2. 进入皮埃尔杂货店
3. 按F2显示调试信息
4. 查看祭坛的tile信息

### 方法3: 直接查看游戏资源

由巴祭坛通常使用以下资源之一：
- `LooseSprites/Cursors` - 游戏的主要UI和装饰sprite sheet
- `Maps/springobjects` - 物品和装饰物的tileset
- 自定义的SeedShop tileset

## 创建祭坛地图文件

### 需要创建的文件

`[CP] MineBustle/assets/altar_tiles.tmx`

这个文件应该是一个2x2的小地图，包含：

1. **Buildings层**：
   - 祭坛的底座（设置为不可通行）
   - 添加Action属性：`Action: YobaAltar`

2. **Front层**：
   - 祭坛的上层装饰

### 临时解决方案：使用占位符

如果暂时找不到确切的贴图，可以：

1. 创建一个简单的2x2占位符
2. 使用任何明显的贴图（如金色方块）
3. 先测试功能是否正常
4. 之后再替换为正确的由巴祭坛贴图

## 祭坛位置

当前配置将祭坛放置在：
- **地图**: Mountain（山区）
- **坐标**: (47, 6) - 2x2区域
- **位置描述**: 矿井入口左侧

如果这个位置不合适，可以修改 `content.json` 中的坐标。

## 推荐的祭坛位置选项

根据设计文档，以下是几个推荐位置：

1. **矿井入口左侧** (47, 6) - 当前配置
2. **矿井入口右侧** (52, 6)
3. **矮人路障附近** (45, 8)

## 兼容性考虑

### Stardew Valley Expanded (SVE)

如果需要兼容SVE，可以在 `content.json` 中添加条件配置：

```json
{
  "Action": "EditMap",
  "Target": "Maps/Mountain",
  "FromFile": "assets/altar_tiles.tmx",
  "ToArea": { "X": 47, "Y": 6, "Width": 2, "Height": 2 },
  "When": {
    "HasMod": "FlashShifter.StardewValleyExpandedCP |contains=false"
  }
},
{
  "Action": "EditMap",
  "Target": "Maps/Mountain",
  "FromFile": "assets/altar_tiles.tmx",
  "ToArea": { "X": 10, "Y": 25, "Width": 2, "Height": 2 },
  "When": {
    "HasMod": "FlashShifter.StardewValleyExpandedCP"
  }
}
```

## 下一步

1. 找到由巴祭坛的确切贴图位置
2. 创建 `assets/altar_tiles.tmx` 文件
3. 测试祭坛是否正确显示在Mountain地图上
4. 测试点击交互是否正常工作

## 快速测试方法

如果你想快速测试功能而不创建完整的地图文件：

1. 暂时注释掉Content Patcher配置
2. 直接在游戏中找到Mountain地图的 (47, 6) 位置
3. 使用调试模式或记住位置
4. 点击该位置测试菜单是否打开
5. 功能正常后再添加视觉元素

