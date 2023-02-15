using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Utility;
using ImGuiNET;
using KamiLib;
using KamiLib.Configuration;
using KamiLib.Drawing;
using KamiLib.Interfaces;
using KamiLib.Windows;
using Lumina.Excel.GeneratedSheets;
using Tf2CriticalHitsPlugin.Configuration;

namespace Tf2CriticalHitsPlugin.Windows;

public class ConfigWindow : SelectionWindow, IDisposable
{
    private static readonly string PluginVersion = GetVersionText();

    public const String Title = "TF2-ish Critical Hits Configuration";

    private readonly ConfigOne configuration;

    internal static readonly FileDialogManager DialogManager = new()
        { AddedWindowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking };

    public static readonly SortedDictionary<ushort, ColorInfo> ForegroundColors = new();
    public static readonly SortedDictionary<ushort, ColorInfo> GlowColors = new();


    private class Option : ISelectable, IDrawable
    {
        internal readonly ConfigOne.JobConfig JobConfig;
        internal readonly FileDialogManager DialogManager;

        internal Option(ConfigOne.JobConfig jobConfig, FileDialogManager dialogManager)
        {
            this.JobConfig = jobConfig;
            this.DialogManager = dialogManager;
        }


        public IDrawable Contents => this;

        public void DrawLabel()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, GetJobColor(JobConfig.GetClassJob()));
            ImGui.Text(JobConfig.GetClassJob().NameEnglish);
            ImGui.PopStyleColor();
        }

        public string ID => JobConfig.GetClassJob().Abbreviation;

        public void Draw()
        {
            DrawDetailPane(JobConfig, DialogManager);
        }
    }

    public ConfigWindow(Tf2CriticalHitsPlugin tf2CriticalHitsPlugin) : base(Title, 0.2f, 23.0f)
    {
        this.configuration = tf2CriticalHitsPlugin.Configuration;
    }

    protected override IEnumerable<ISelectable> GetSelectables()
    {
        return configuration.JobConfigurations
                            .Values
                            .ToList()
                            .ConvertAll(jobConfig => new Option(jobConfig, DialogManager))
                            .OrderBy(o => o.JobConfig.GetClassJob().NameEnglish.RawString)
                            .OrderBy(o => o.JobConfig.GetClassJob().Role);
    }

    public override void Draw()
    {
        base.Draw();
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        DialogManager.Draw();
    }

    protected override void DrawExtras()
    {
        DrawVersionText();
    }

    private static void DrawDetailPane(ConfigOne.JobConfig jobConfig, FileDialogManager dialogManager)
    {
        ImGui.Text($"Configuration for {jobConfig.GetClassJob().NameEnglish}");
        foreach (var module in jobConfig)
        {
            DrawConfigModule(module, dialogManager);
        }
    }


    private static void DrawConfigModule(ConfigOne.ConfigModule config, FileDialogManager dialogManager)
    {
        void UpdatePath(bool success, List<string> paths)
        {
            if (success && paths.Count > 0)
            {
                config.FilePath = new Setting<string>(paths[0]);
            }
        }

        InfoBox.Instance.AddTitle(config.GetModuleDefaults().SectionLabel)
               .AddConfigCheckbox("Play sound", config.PlaySound, additionalID: $"{config.GetId()}PlaySound")
               .StartConditional(config.PlaySound)
               .AddIndent(2)
               .AddConfigCheckbox("Play sound only for actions (ignore auto-attacks)", config.SoundForActionsOnly)
               .AddInputString(string.Empty, config.FilePath, 512, ImGuiInputTextFlags.ReadOnly)
               .SameLine()
               .AddIconButton($"{config.GetId()}browse", FontAwesomeIcon.Folder, () => dialogManager.OpenFileDialog(
                                  "Select the file", "Audio files{.wav,.mp3}", UpdatePath, 1,
                                  config.FilePath.Value.IsNullOrEmpty()
                                      ? Environment.ExpandEnvironmentVariables("%USERPROFILE%")
                                      : config.FilePath.Value), "Open file browser...")
               .AddSliderInt("Volume", config.Volume, 0, 100)
               .AddIndent(-2)
               .EndConditional()
               .AddConfigCheckbox("Show flavor text with floating value", config.ShowText)
               .StartConditional(config.ShowText)
               .AddIndent(2)
               .AddInputString("Text", config.Text, Constants.MaxTextLength)
               .AddAction(() => ImGui.Text("Color: "))
               .SameLine()
               .AddAction(() =>
               {
                   if (ColorComponent.SelectorButton(ForegroundColors, $"{config.GetId()}Foreground",
                                                     ref config.TextColor.Value,
                                                     config.GetModuleDefaults().FlyTextParameters.ColorKey.Value))
                   {
                       KamiCommon.SaveConfiguration();
                   }
               })
               .SameLine()
               .AddAction(() => ImGui.Text("Glow: "))
               .SameLine()
               .AddAction(() =>
               {
                   if (ColorComponent.SelectorButton(GlowColors, $"{config.GetId()}Glow",
                                                     ref config.TextGlowColor.Value,
                                                     config.GetModuleDefaults().FlyTextParameters.GlowColorKey.Value))
                   {
                       KamiCommon.SaveConfiguration();
                   }
               })
               .SameLine()
               .AddConfigCheckbox("Italics", config.TextItalics)
               .AddIndent(-2)
               .EndConditional()
               .AddButton("Test configuration", () => Tf2CriticalHitsPlugin.GenerateTestFlyText(config))
               .Draw();
    }

    private static Vector4 GetJobColor(ClassJob classJob) => classJob.Role switch
    {
        1 => Colors.Blue,
        4 => Colors.HealerGreen,
        _ => Colors.DPSRed
    };

    private static string GetVersionText()
    {
        var assemblyInformation = Assembly.GetExecutingAssembly().FullName!.Split(',');

        var versionString = assemblyInformation[1].Replace('=', ' ');

        var commitInfo = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                 ?.InformationalVersion ?? "Unknown";
        return $"{versionString} - {commitInfo}";
    }

    private static void DrawVersionText()
    {
        var region = ImGui.GetContentRegionAvail();

        var versionTextSize = ImGui.CalcTextSize(PluginVersion) / 2.0f;
        var cursorStart = ImGui.GetCursorPos();
        cursorStart.X += region.X / 2.0f - versionTextSize.X;

        ImGui.SetCursorPos(cursorStart);
        ImGui.TextColored(Colors.Grey, PluginVersion);
    }


    public override void OnClose()
    {
        DialogManager.Reset();
    }


    public void Dispose()
    {
        DialogManager.Reset();
    }
}
