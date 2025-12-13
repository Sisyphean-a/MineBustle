# MineBustle - 由巴的祭坛 (Yoba's Altar)

一个星露谷物语SMAPI模组，允许玩家通过献祭金币来动态调整矿井怪物生成倍率（1x-10x）。

## 功能特性

- **动态怪物生成倍率**：通过献祭金币，将今日矿井的怪物生成概率提升至1-10倍
- **经济平衡系统**：献祭费用基于玩家总收入和选择的倍率动态计算
- **每日重置**：倍率效果仅持续当天，第二天自动重置为1.0x
- **高性能实现**：使用IL Transpiler技术，运行时开销几乎为零
- **完全兼容**：与其他矿井相关模组兼容

## 技术亮点

### 1. IL Transpiler 实现
本模组采用了Harmony的IL Transpiler技术，直接修改`MineShaft.populateLevel`方法中的`monsterChance`变量：

- **动态变量索引获取**：自动识别`monsterChance`变量的索引，无需硬编码
- **最佳注入点**：在`adjustLevelChances`调用后注入，确保与其他模组兼容
- **零性能开销**：仅增加3条IL指令（加载、乘法、存储）

### 2. 经济模型
献祭费用计算公式：
```
Cost = (BaseFee + α × TotalEarnings) × (Multiplier - 1)^β
```

参数说明：
- `BaseFee`：基础费用（默认500g）
- `TotalEarnings`：玩家总收入
- `α`：通胀系数（默认0.001）
- `β`：惩罚指数（默认1.5）

这确保了：
- 早期玩家难以负担高倍率
- 后期玩家可以使用高倍率作为金币回收机制
- 倍率越高，费用增长越快

## 安装方法

1. 安装 [SMAPI](https://smapi.io/)
2. 下载本模组的最新版本
3. 解压到 `Stardew Valley/Mods` 文件夹
4. 启动游戏

## 使用方法

1. 前往**山区（Mountain）**的矿井入口
2. 找到**由巴的祭坛**（位于矿井入口左侧，坐标约47,6）
3. 右键点击祭坛打开献祭菜单
4. 使用滑动条选择想要的怪物生成倍率（1.0x - 10.0x）
5. 查看所需金币数量
6. 点击"献祭"按钮完成献祭
7. 进入矿井享受更多的怪物战斗！

**注意**：祭坛的视觉元素需要配合Content Patcher模组。如果你只安装了主模组，祭坛是"隐形"的，但仍然可以在正确的位置点击交互。

## 配置选项

编辑 `config.json` 文件可以调整以下参数：

```json
{
  "CurrentMultiplier": 1.0,        // 当前倍率（自动管理，无需手动修改）
  "BaseFee": 500,                  // 基础献祭费用
  "InflationCoefficient": 0.001,   // 通胀系数
  "PenaltyExponent": 1.0           // 惩罚指数
}
```

## 兼容性

- **SMAPI版本**：4.0.0+
- **游戏版本**：Stardew Valley 1.6+
- **多人游戏**：支持（倍率由主机玩家控制）

## 已知问题

- 祭坛位置目前硬编码在山区矿井入口附近（坐标47,6），未来版本将支持自定义位置
- 需要Content Patcher模组来添加祭坛的视觉元素（当前版本功能完整，但祭坛是隐形的）
- 祭坛贴图需要从游戏中的皮埃尔杂货店复制（参见 `[CP] MineBustle` 文件夹中的说明）

## 开发计划

- [ ] 添加Content Patcher支持，在地图上显示祭坛
- [ ] 添加祭坛动画和特效
- [ ] 支持自定义祭坛位置
- [ ] 添加更多配置选项
- [ ] 支持其他地点（如骷髅洞穴）

## 技术细节

### 核心代码结构

```
MineBustle/
├── ModEntry.cs                  # 模组入口
├── ModConfig.cs                 # 配置类
├── MineShaftPatches.cs          # Harmony Transpiler补丁
├── AltarMenu.cs                 # 祭坛UI菜单
├── AltarInteractionHandler.cs   # 祭坛交互处理
└── manifest.json                # 模组清单
```

### Transpiler工作原理

1. 定位`adjustLevelChances`方法调用
2. 回退3步找到`monsterChance`变量的加载指令
3. 自动获取变量索引
4. 在调用后插入乘法逻辑：
   ```
   ldloc.s <index>      // 加载 monsterChance
   call GetMultiplier   // 获取倍率
   mul                  // 相乘
   stloc.s <index>      // 存回 monsterChance
   ```

## 致谢

- 感谢SMAPI团队提供的优秀模组框架
- 感谢Harmony库提供的IL修改能力
- 设计灵感来源于社区讨论

## 许可证

MIT License

## 更新日志

### v1.0.0 (2024-12-10)
- 初始版本发布
- 实现核心功能：动态怪物生成倍率
- 实现祭坛UI和交互系统
- 实现经济平衡系统

