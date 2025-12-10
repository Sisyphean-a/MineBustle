using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MineBustle;

/// <summary>
/// MineShaft 类的 Harmony 补丁
/// 使用 IL Transpiler 修改怪物生成概率
/// </summary>
[HarmonyPatch(typeof(MineShaft), "populateLevel")]
public class MineShaftPatches
{
    /// <summary>
    /// 获取当前的怪物生成倍率
    /// 这个方法会被 IL 代码调用
    /// </summary>
    /// <returns>当前倍率（1.0 - 10.0）</returns>
    public static double GetSpawnMultiplier()
    {
        // 确保倍率至少为 1.0，防止出现 0 倍率导致不出怪
        double multiplier = ModEntry.Config.CurrentMultiplier;
        return multiplier > 0 ? multiplier : 1.0;
    }

    /// <summary>
    /// IL Transpiler 方法
    /// 在 adjustLevelChances 调用后，将 monsterChance 乘以我们的倍率
    /// </summary>
    /// <param name="instructions">原始 IL 指令</param>
    /// <returns>修改后的 IL 指令</returns>
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var adjustLevelChancesMethod = AccessTools.Method(typeof(MineShaft), "adjustLevelChances");

        try
        {
            // 1. 找到 adjustLevelChances 的调用位置
            int callIndex = -1;
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand?.ToString()?.Contains("adjustLevelChances") == true)
                {
                    callIndex = i;
                    break;
                }
            }

            if (callIndex == -1)
            {
                ModEntry.ModMonitor.Log("无法定位 adjustLevelChances 方法调用，Transpiler 失败！", LogLevel.Error);
                return instructions;
            }

            // 2. 自动获取 monsterChance 变量的索引
            // adjustLevelChances 的签名是 (ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance)
            // 调用前的 IL 栈是：
            // ldloca.s stoneChance
            // ldloca.s monsterChance  <-- 我们要找这个（倒数第3个指令）
            // ldloca.s itemChance
            // ldloca.s gemStoneChance
            // call adjustLevelChances

            // 回退 3 步，找到加载 monsterChance 地址的指令
            int monsterChanceLoadIndex = callIndex - 3;
            var monsterChanceInstruction = codes[monsterChanceLoadIndex];

            // 验证指令类型
            if (monsterChanceInstruction.opcode != OpCodes.Ldloca_S && monsterChanceInstruction.opcode != OpCodes.Ldloca)
            {
                ModEntry.ModMonitor.Log($"意外的指令类型: {monsterChanceInstruction.opcode}，期望 Ldloca_S 或 Ldloca", LogLevel.Error);
                return instructions;
            }

            // 获取 monsterChance 变量的索引（操作数）
            object monsterVariableIndex = monsterChanceInstruction.operand;

            // 3. 在 call 指令之后插入我们的乘法逻辑
            var newInstructions = new List<CodeInstruction>
            {
                // 加载 monsterChance 变量的值
                new CodeInstruction(OpCodes.Ldloc_S, monsterVariableIndex),
                // 调用我们的方法获取倍率
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MineShaftPatches), nameof(GetSpawnMultiplier))),
                // 相乘：monsterChance * multiplier
                new CodeInstruction(OpCodes.Mul),
                // 将结果存回 monsterChance 变量
                new CodeInstruction(OpCodes.Stloc_S, monsterVariableIndex)
            };

            // 在 call 指令后插入
            codes.InsertRange(callIndex + 1, newInstructions);

            ModEntry.ModMonitor.Log($"成功注入怪物生成倍率修改代码！变量索引: {monsterVariableIndex}", LogLevel.Debug);
        }
        catch (System.Exception ex)
        {
            ModEntry.ModMonitor.Log($"Transpiler 执行失败: {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
            return instructions;
        }

        return codes;
    }
}

