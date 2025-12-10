using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;

namespace MineBustle;

public class AltarMenu : IClickableMenu
{
    private const int MenuWidth = 600;
    private const int MenuHeight = 400;
    private const int SliderWidth = 400;
    private const int SliderHeight = 24;
    private const int SliderKnobSize = 32;

    private Rectangle sliderBounds;
    private Rectangle sliderKnobBounds;
    private bool isDraggingSlider = false;
    private float sliderPosition = 0f;

    private ClickableTextureComponent confirmButton;
    private ClickableTextureComponent cancelButton;

    private double currentMultiplier = 1.0;
    private int currentCost = 0;

    public AltarMenu() : base(
        (Game1.uiViewport.Width - MenuWidth) / 2,
        (Game1.uiViewport.Height - MenuHeight) / 2,
        MenuWidth,
        MenuHeight)
    {
        sliderBounds = new Rectangle(
            xPositionOnScreen + (MenuWidth - SliderWidth) / 2,
            yPositionOnScreen + 150,
            SliderWidth,
            SliderHeight
        );

        UpdateSliderKnob();

        confirmButton = new ClickableTextureComponent(
            new Rectangle(xPositionOnScreen + MenuWidth / 2 - 80, yPositionOnScreen + MenuHeight - 100, 64, 64),
            Game1.mouseCursors,
            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46),
            1f
        );

        cancelButton = new ClickableTextureComponent(
            new Rectangle(xPositionOnScreen + MenuWidth / 2 + 16, yPositionOnScreen + MenuHeight - 100, 64, 64),
            Game1.mouseCursors,
            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47),
            1f
        );

        UpdateMultiplierAndCost();
    }

    private void UpdateSliderKnob()
    {
        int knobX = sliderBounds.X + (int)(sliderPosition * (SliderWidth - SliderKnobSize));
        sliderKnobBounds = new Rectangle(knobX, sliderBounds.Y - 4, SliderKnobSize, SliderKnobSize);
    }

    private void UpdateMultiplierAndCost()
    {
        currentMultiplier = 1.0 + (sliderPosition * 9.0);
        currentCost = CalculateCost(currentMultiplier);
    }

    private int CalculateCost(double multiplier)
    {
        var config = ModEntry.Config;
        double baseFee = config.BaseFee;
        double totalEarnings = Game1.player.totalMoneyEarned;
        double alpha = config.InflationCoefficient;
        double beta = config.PenaltyExponent;

        double cost = (baseFee + alpha * totalEarnings) * Math.Pow(multiplier - 1.0, beta);
        return (int)Math.Round(cost);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (sliderKnobBounds.Contains(x, y))
        {
            isDraggingSlider = true;
            Game1.playSound("drumkit6");
        }
        else if (confirmButton.containsPoint(x, y))
        {
            ConfirmOffering();
        }
        else if (cancelButton.containsPoint(x, y))
        {
            exitThisMenu();
            Game1.playSound("bigDeSelect");
        }
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        isDraggingSlider = false;
    }

    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

        if (isDraggingSlider)
        {
            float newPosition = (float)(x - sliderBounds.X) / (SliderWidth - SliderKnobSize);
            sliderPosition = Math.Clamp(newPosition, 0f, 1f);
            UpdateSliderKnob();
            UpdateMultiplierAndCost();
        }
    }

    private void ConfirmOffering()
    {
        if (Game1.player.Money < currentCost)
        {
            Game1.playSound("cancel");
            Game1.showRedMessage("金币不足！");
            return;
        }

        Game1.player.Money -= currentCost;
        ModEntry.Config.CurrentMultiplier = currentMultiplier;
        ModEntry.SaveConfig();

        Game1.playSound("yoba");
        // 创建金色闪光效果（使用临时精灵）
        Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
            "LooseSprites\\Cursors",
            new Rectangle(432, 1664, 16, 16),
            80f,
            8,
            0,
            Game1.player.Position,
            false,
            false,
            1f,
            0f,
            Color.Gold,
            4f,
            0f,
            0f,
            0f
        ));

        string message = $"献祭成功！今日矿井怪物生成倍率：{currentMultiplier:F1}x";
        Game1.addHUDMessage(new HUDMessage(message, HUDMessage.achievement_type));

        ModEntry.ModMonitor.Log($"玩家献祭 {currentCost}g，设置倍率为 {currentMultiplier:F1}x", StardewModdingAPI.LogLevel.Info);

        exitThisMenu();
    }

    public override void draw(SpriteBatch b)
    {
        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
        Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, MenuWidth, MenuHeight, false, true);

        string title = "由巴的祭坛";
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title);
        Utility.drawTextWithShadow(b, title, Game1.dialogueFont,
            new Vector2(xPositionOnScreen + (MenuWidth - titleSize.X) / 2, yPositionOnScreen + 32), Game1.textColor);

        string description = "献祭金币以增强今日矿井的怪物生成";
        Vector2 descSize = Game1.smallFont.MeasureString(description);
        Utility.drawTextWithShadow(b, description, Game1.smallFont,
            new Vector2(xPositionOnScreen + (MenuWidth - descSize.X) / 2, yPositionOnScreen + 80), Color.Gray);

        DrawSliderTrack(b);
        DrawSliderKnob(b);

        string multiplierText = $"怪物生成倍率: {currentMultiplier:F1}x";
        Vector2 multiplierSize = Game1.dialogueFont.MeasureString(multiplierText);
        Utility.drawTextWithShadow(b, multiplierText, Game1.dialogueFont,
            new Vector2(xPositionOnScreen + (MenuWidth - multiplierSize.X) / 2, yPositionOnScreen + 200), Game1.textColor);

        bool canAfford = Game1.player.Money >= currentCost;
        Color costColor = canAfford ? Color.Green : Color.Red;
        string costText = $"所需金币: {currentCost}g";
        Vector2 costSize = Game1.dialogueFont.MeasureString(costText);
        Utility.drawTextWithShadow(b, costText, Game1.dialogueFont,
            new Vector2(xPositionOnScreen + (MenuWidth - costSize.X) / 2, yPositionOnScreen + 250), costColor);

        confirmButton.draw(b);
        cancelButton.draw(b);

        string confirmText = "献祭";
        Vector2 confirmTextSize = Game1.smallFont.MeasureString(confirmText);
        Utility.drawTextWithShadow(b, confirmText, Game1.smallFont,
            new Vector2(confirmButton.bounds.X + (confirmButton.bounds.Width - confirmTextSize.X) / 2,
                       confirmButton.bounds.Y - 30), Game1.textColor);

        string cancelText = "取消";
        Vector2 cancelTextSize = Game1.smallFont.MeasureString(cancelText);
        Utility.drawTextWithShadow(b, cancelText, Game1.smallFont,
            new Vector2(cancelButton.bounds.X + (cancelButton.bounds.Width - cancelTextSize.X) / 2,
                       cancelButton.bounds.Y - 30), Game1.textColor);

        drawMouse(b);
    }

    private void DrawSliderTrack(SpriteBatch b)
    {
        b.Draw(Game1.staminaRect,
            new Rectangle(sliderBounds.X, sliderBounds.Y + SliderHeight / 2 - 2, SliderWidth, 4), Color.DarkGray);

        int highlightWidth = (int)(sliderPosition * SliderWidth);
        b.Draw(Game1.staminaRect,
            new Rectangle(sliderBounds.X, sliderBounds.Y + SliderHeight / 2 - 2, highlightWidth, 4), Color.Gold);
    }

    private void DrawSliderKnob(SpriteBatch b)
    {
        Rectangle sourceRect = new Rectangle(0, 256, 64, 64);
        b.Draw(Game1.mouseCursors, sliderKnobBounds, sourceRect, Color.White);
    }
}
