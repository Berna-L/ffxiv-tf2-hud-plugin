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
        public ImmutableSortedDictionary<string, ClassJob> CombatJobs;

        public ConfigOne Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");

        public Tf2CriticalHitsPlugin(DalamudPluginInterface pluginInterface)
        {
            
            pluginInterface.Create<Service>();
            KamiCommon.Initialize(pluginInterface, Name, () => Configuration?.Save());
            
            CombatJobs = InitCombatJobs();
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

        private static ImmutableSortedDictionary<string, ClassJob> InitCombatJobs()
        {
            // A combat job is one that has a JobIndex.
            if (Service.DataManager == null) throw new ApplicationException("DataManager not initialized!");
        
            var classJobSheet = Service.DataManager.GetExcelSheet<ClassJob>();
            if (classJobSheet == null) throw new ApplicationException("ClassJob sheet unavailable!");
            var jobList = new List<ClassJob>();
            for (var i = 0u; i < classJobSheet.RowCount; i++)
            {
                var classJob = classJobSheet.GetRow(i);
                if (classJob is null || classJob.JobIndex is 0) continue;
                jobList.Add(classJob);
            }

            return jobList.ToImmutableSortedDictionary(j => j.Abbreviation.ToString(), j => j);
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
            foreach (var config in Configuration.SubConfigurations.Values)
            {
                if (config.FlyTextColor == color &&
                    (config.ActionFlyTextKinds.Contains(kind) || config.AutoFlyTextKinds.Contains(kind)))
                {
                    LogDebug($"{config.Id} registered!");
                    if (config.ShowText)
                    {
                        text2 = GenerateText(config);
                    }

                    if (config.PlaySound && (!config.SoundForActionsOnly || config.ActionFlyTextKinds.Contains(kind)))
                    {
                        SoundEngine.PlaySound(config.FilePath, config.Volume * 0.01f);
                    }
                }
            }
        }

        public static void GenerateTestFlyText(ConfigOne.SubConfiguration config)
        {
            var kind = config.ActionFlyTextKinds.FirstOrDefault();
            LogDebug($"Kind: {kind}, Config ID: {config.Id}");
            var text = Constants.TestFlavorText[
                (int)Math.Floor(Random.Shared.NextSingle() * Constants.TestFlavorText.Length)];
            Service.FlyTextGui.AddFlyText(kind, 1, 3333, 0, new SeStringBuilder().AddText(text).Build(),
                                          GenerateText(config), config.FlyTextColor, 0, 60012);
        }

        private static SeString GenerateText(ConfigOne.SubConfiguration config)
        {
            LogDebug(
                $"Generating text with colorKey {config.TextParameters.ColorKey} and glowColorKey {config.TextParameters.GlowColorKey}");
            var stringBuilder = new SeStringBuilder()
                                .AddUiForeground(config.TextParameters.ColorKey)
                                .AddUiGlow(config.TextParameters.GlowColorKey);
            if (config.Italics)
            {
                stringBuilder.AddItalicsOn();
            }

            return stringBuilder
                   .AddText(config.Text)
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
