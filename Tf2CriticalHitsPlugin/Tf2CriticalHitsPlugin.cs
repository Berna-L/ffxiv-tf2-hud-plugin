using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using KamiLib;
using Lumina.Excel.GeneratedSheets;
using Tf2CriticalHitsPlugin.Configuration;
using Tf2CriticalHitsPlugin.Windows;
using static Dalamud.Logging.PluginLog;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => "TF2-ish Critical Hits";
        private const string CommandName = "/critconfig";

        public ConfigOne Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");

        public Tf2CriticalHitsPlugin(DalamudPluginInterface pluginInterface)
        {
            
            pluginInterface.Create<Service>();
            KamiCommon.Initialize(pluginInterface, Name, () => Configuration?.Save());
            
            InitColors();

            this.Configuration = InitConfig();
            this.Configuration.Initialize(Service.PluginInterface);
            Configuration.Save();
            
            WindowSystem.AddWindow(new ConfigWindow(this));

            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the TF2-ish Critical Hits configuration window"
            });

            Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;

            Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;
        }

        private static ConfigOne InitConfig()
        {
            var configFile = Service.PluginInterface.ConfigFile.FullName;
            if (!File.Exists(configFile))
            {
                return new ConfigOne();
            }
            var configText = File.ReadAllText(configFile);
            var versionCheck = JsonSerializer.Deserialize<VersionCheck>(configText);
            if (versionCheck is null)
            {
                return new ConfigOne();
            }
            var version = versionCheck.Version;
            var config = version switch
            {
                0 => JsonSerializer.Deserialize<ConfigZero>(configText)?.MigrateToOne() ?? new ConfigOne(),
                1 => Service.PluginInterface.GetPluginConfig() as ConfigOne ?? new ConfigOne(),
                _ => new ConfigOne()
            };

            return config;
        }


        private static void InitColors()
        {

            
            ConfigWindow.ForegroundColors.Clear();
            ConfigWindow.GlowColors.Clear();

            if (Service.DataManager != null)
            {
                var colorSheet = Service.DataManager.GetExcelSheet<UIColor>();
                if (colorSheet != null)
                {
                    for (var i = 0u; i < colorSheet.RowCount; i++)
                    {
                        var row = colorSheet.GetRow(i);
                        if (row != null)
                        {
                            ConfigWindow.ForegroundColors.Add(
                                (ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIForeground));
                            ConfigWindow.GlowColors.Add((ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIGlow));
                        }
                    }
                }
            }
        }


        public void FlyTextCreate(
            ref FlyTextKind kind,
            ref int val1,
            ref int val2,
            ref SeString text1,
            ref SeString text2,
            ref uint color,
            ref uint icon,
            ref uint damageTypeIcon,
            ref float yOffset,
            ref bool handled)
        {
            LogDebug($"Color: {color}");
            var currentClassJobId = GetCurrentClassJobId();
            if (currentClassJobId is null) return;
            
            foreach (var config in Configuration.JobConfigurations[currentClassJobId.Value])
            {
                if (config.ModuleDefaults.FlyTextColor == color &&
                    (config.ModuleDefaults.FlyTextType.AutoAttack.Contains(kind) || config.ModuleDefaults.FlyTextType.Action.Contains(kind)))
                {
                    LogDebug($"{config.GetId()} registered!");
                    if (config.ShowText)
                    {
                        text2 = GenerateText(config);
                    }

                    if (config.PlaySound && (!config.SoundForActionsOnly || config.ModuleDefaults.FlyTextType.Action.Contains(kind)))
                    {
                        SoundEngine.PlaySound(config.FilePath.Value, config.Volume.Value * 0.01f);
                    }
                }
            }
        }

        private static unsafe byte? GetCurrentClassJobId()
        {
            var classJobId = PlayerState.Instance()->CurrentClassJobId;
            return Constants.CombatJobs.ContainsKey(classJobId) ? classJobId : null;
        } 

        public static void GenerateTestFlyText(ConfigOne.ConfigModule config)
        {
            var kind = config.ModuleDefaults.FlyTextType.Action.FirstOrDefault();
            LogDebug($"Kind: {kind}, Config ID: {config.GetId()}");
            var text = Constants.TestFlavorText[
                (int)Math.Floor(Random.Shared.NextSingle() * Constants.TestFlavorText.Length)];
            Service.FlyTextGui.AddFlyText(kind, 1, 3333, 0, new SeStringBuilder().AddText(text).Build(),
                                          GenerateText(config), config.ModuleDefaults.FlyTextColor, 0, 60012);
        }

        private static SeString GenerateText(ConfigOne.ConfigModule config)
        {
            LogDebug(
                $"Generating text with color {config.TextColor} and glow {config.TextGlowColor}");
            var stringBuilder = new SeStringBuilder()
                                .AddUiForeground(config.TextColor.Value)
                                .AddUiGlow(config.TextGlowColor.Value);
            if (config.TextItalics)
            {
                stringBuilder.AddItalicsOn();
            }

            return stringBuilder
                   .AddText(config.Text.Value)
                   .AddItalicsOff()
                   .AddUiForegroundOff()
                   .AddUiGlowOff()
                   .Build();
        }


        public void Dispose()
        {
            KamiCommon.Dispose();
            Service.FlyTextGui.FlyTextCreated -= FlyTextCreate;
            this.WindowSystem.RemoveAllWindows();
            Service.CommandManager.RemoveHandler(CommandName);
        }

        private void OnConfigCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            WindowSystem.GetWindow(ConfigWindow.Title)!.IsOpen = true;
        }

        private void DrawUserInterface()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigWindow()
        {
            WindowSystem.GetWindow(ConfigWindow.Title)!.IsOpen = true;
        }
    }
}
