using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using MaskOfKefka.Capture;

namespace MaskOfKefka.Windows;

public class ConfigWindow : Window
{
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin) : base("Mask of Kefka 設定###MaskOfKefkaConfig")
    {
        this.plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(440, 260),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void Draw()
    {
        var config = plugin.Config;

        var active = plugin.OutputRequested;
        if (ImGui.Checkbox("啟用輸出視窗", ref active))
            plugin.OutputRequested = active;
        ImGui.TextDisabled("請在 OBS 使用「視窗擷取」（Windows 10 方法），並選擇「Mask of Kefka」視窗。");

        ImGui.TextColored(new Vector4(1f, 0.75f, 0.25f, 1f), "效能提示：直播時請將輸出視窗最小化。");
        ImGui.TextWrapped(
            "輸出視窗可見時，Windows 每幀都要合成畫面，可能明顯降低遊戲 FPS。" +
            "完成擷取設定後請將視窗最小化；在目前的 Windows 11 上，OBS／Discord 通常仍可正常擷取，" +
            "同時幾乎不再產生額外負擔。若較舊版本的 Windows 在最小化後停止擷取，請改為縮小視窗並保持可見。");
        if (plugin.OutputActive && ImGui.Button("將輸出視窗最小化"))
            plugin.MinimizeOutput();

        ImGui.Separator();

        var showUi = config.ShowGameUi;
        if (ImGui.Checkbox("在輸出畫面顯示遊戲 UI", ref showUi))
        {
            config.ShowGameUi = showUi;
            config.Save();
        }

        if (!config.ShowGameUi)
        {
            ImGui.TextWrapped(
                "無 UI 模式：請選擇包含遊戲場景的渲染目標索引。此索引可能隨遊戲更新而改變，" +
                "請調整到輸出視窗能顯示不含 UI 的場景。索引無效時，會自動改用包含 UI 的畫面。");

            var index = config.RenderTargetIndex;
            if (ImGui.InputInt("渲染目標索引", ref index))
            {
                config.RenderTargetIndex = Math.Clamp(index, -1, GameSources.RenderTargetSlotCount - 1);
                config.Save();
            }

            ImGui.TextDisabled($"範圍：0 到 {GameSources.RenderTargetSlotCount - 1}；輸入 -1 可停用。");

            if (ImGui.Button("將渲染目標清單寫入記錄"))
                GameSources.DumpRenderTargets(Plugin.Log);
            ImGui.SameLine();
            ImGui.TextDisabled("（使用 /xllog 開啟記錄）");
        }

        ImGui.Separator();

        var borderless = config.Borderless;
        if (ImGui.Checkbox("無邊框視窗（隱藏標題列）", ref borderless))
        {
            config.Borderless = borderless;
            config.Save();
        }
        if (config.Borderless)
            ImGui.TextDisabled("可從視窗內任意位置拖曳，並從邊緣（8px）調整大小。");

        var everyNth = config.RenderEveryNthFrame;
        if (ImGui.SliderInt("每隔幾個遊戲影格輸出 1 幀", ref everyNth, 1, 4))
        {
            config.RenderEveryNthFrame = Math.Clamp(everyNth, 1, 4);
            config.Save();
        }
        ImGui.TextDisabled(config.RenderEveryNthFrame == 1
            ? "輸出幀率與遊戲相同（負擔最高、畫面最流暢）。"
            : $"遊戲為 60 FPS 時，輸出約為 {60 / config.RenderEveryNthFrame} FPS；GPU 負擔會依比例降低。");

        ImGui.Separator();

        var startMinimized = config.StartMinimized;
        if (ImGui.Checkbox("開啟輸出視窗時自動最小化（建議）", ref startMinimized))
        {
            config.StartMinimized = startMinimized;
            config.Save();
        }

        var autoStart = config.AutoStart;
        if (ImGui.Checkbox("外掛載入時自動開啟輸出視窗", ref autoStart))
        {
            config.AutoStart = autoStart;
            config.Save();
        }
    }
}
