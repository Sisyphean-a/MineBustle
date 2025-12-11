import json
import os
# === 配置区域 ===
START_X = 55
START_Y = 7
WIDTH = 8
HEIGHT = 8
# 如果有不需要显示的格子（比如贴图这块是完全透明的，不想覆盖地图），可以在这里定义 (x, y) 偏移量
# 例如: SKIP_TILES = [(0, 0), (7, 0)]  // 跳过左上角和右上角
SKIP_TILES = [] 
def generate():
    tiles = []
    csharp_code_lines = []
    
    # C# 代码生成头
    csharp_code_lines.append("    static AltarInteractionHandler()")
    csharp_code_lines.append("    {")
    csharp_code_lines.append("        AltarTiles = new List<Vector2>();")
    for y in range(HEIGHT):
        for x in range(WIDTH):
            if (x, y) in SKIP_TILES:
                continue
            # 1. 生成 JSON 数据
            index = y * WIDTH + x
            tiles.append({
                "Position": { "X": START_X + x, "Y": START_Y + y },
                "Layer": "Buildings",
                "SetTilesheet": "z_MineBustle_Altar",
                "SetIndex": index
            })
            # 2. 生成 C# 对应逻辑 (直接展开每行，或者你可以用循环逻辑，这里为了直观直接生成)
            # 不过为了代码整洁，我们还是生成循环逻辑，但如果形状不规则，直接 add 坐标最稳妥
            csharp_code_lines.append(f"        AltarTiles.Add(new Vector2({START_X + x}, {START_Y + y}));")
    csharp_code_lines.append("    }")
    # 生成 JSON 文件
    content = {
        "Format": "2.3.0",
        "ConfigSchema": {
            "EnableAltar": {
                "AllowValues": "true, false",
                "Default": True
            }
        },
        "Changes": [
            {
                "Action": "Load",
                "Target": "Mods/MineBustle/AltarSprite",
                "FromFile": "assets/altar.png"
            },
            {
                "Action": "EditMap",
                "Target": "Maps/Mountain",
                "AddTilesheets": [
                    {
                        "Id": "z_MineBustle_Altar",
                        "Image": "Mods/MineBustle/AltarSprite",
                        "TileSize": { "Width": 16, "Height": 16 }
                    }
                ],
                "MapTiles": tiles,
                "When": { "EnableAltar": "true" }
            }
        ]
    }
    target_file = r"f:\MineBustle\MineBustle\[CP] MineBustle\content.json"
    with open(target_file, "w", encoding="utf-8") as f:
        json.dump(content, f, indent=4)
    print(f"成功生成 content.json！包含 {len(tiles)} 个图块定义。")
    print("\n=== 请确保你的 C# 代码 (AltarInteractionHandler.cs) 匹配以下逻辑 ===")
    print("\n".join(csharp_code_lines))
if __name__ == "__main__":
    generate()