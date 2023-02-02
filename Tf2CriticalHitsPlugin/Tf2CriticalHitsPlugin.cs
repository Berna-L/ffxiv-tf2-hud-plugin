using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Lumina.Excel.GeneratedSheets;
using Tf2CriticalHitsPlugin.Windows;
using static Dalamud.Logging.PluginLog;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => "TF2-ish Critical Hits";
        private const string CommandName = "/critconfig";

        
        [PluginService]
        [RequiredVersion("1.0")]
        public static DataManager? DataManager => null;

        public Configuration Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");
        
        private static readonly ISet<FlyTextKind> AutoDirectCriticalHit = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> ActionDirectCriticalHit = new HashSet<FlyTextKind>();

        private static readonly ISet<FlyTextKind> AutoCriticalHit = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> ActionCriticalHit = new HashSet<FlyTextKind>();

        private static readonly ISet<FlyTextKind> AutoDirectHit = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> ActionDirectHit = new HashSet<FlyTextKind>();

        public Tf2CriticalHitsPlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<Service>();
            ConfigWindow.ForegroundColors.Clear();
            ConfigWindow.GlowColors.Clear();

            InitColors();

            
            this.Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(Service.PluginInterface);
            
            WindowSystem.AddWindow(new ConfigWindow(this));

            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the TF2-ish Critical Hits configuration window"
            });

            Service.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            Service.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;
            
            AddDirectCriticalHitKinds();
            AddCriticalHitKinds();
            AddDirectHitKinds();

            Service.FlyTextGui.FlyTextCreated += this.FlyTextCreate;

        }

        private static void InitColors()
        {
            if (DataManager != null)
            {
                var colorSheet = DataManager.GetExcelSheet<UIColor>();
                if (colorSheet != null)
                {
                    for (var i = 0u; i < colorSheet.RowCount; i++)
                    {
                        var row = colorSheet.GetRow(i);
                        if (row != null)
                        {
                            ConfigWindow.ForegroundColors.Add((ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIForeground));
                            ConfigWindow.GlowColors.Add((ushort)i, ColorInfo.FromUiColor((ushort)i, row.UIGlow));
                        }
                    }
                }
            }
        }


        private static void AddDirectCriticalHitKinds()
        {
            AutoDirectCriticalHit.Add(FlyTextKind.CriticalDirectHit);
            AutoDirectCriticalHit.Add(FlyTextKind.CriticalDirectHit2);

            ActionDirectCriticalHit.Add(FlyTextKind.NamedCriticalDirectHit);
        }
        
        private static void AddCriticalHitKinds()
        {
            AutoCriticalHit.Add(FlyTextKind.CriticalHit);
            AutoCriticalHit.Add(FlyTextKind.CriticalHit2);
            AutoCriticalHit.Add(FlyTextKind.CriticalHit3);
            AutoCriticalHit.Add(FlyTextKind.CriticalHit4);

            ActionCriticalHit.Add(FlyTextKind.NamedCriticalHit);
            ActionCriticalHit.Add(FlyTextKind.NamedCriticalHit2);
        }

        private static void AddDirectHitKinds()
        {
            AutoDirectHit.Add(FlyTextKind.DirectHit);
            AutoDirectHit.Add(FlyTextKind.DirectHit2);

            ActionDirectHit.Add(FlyTextKind.NamedDirectHit);
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
            if (ActionDirectCriticalHit.Contains(kind) || AutoDirectCriticalHit.Contains(kind))
            {
                LogDebug("Direct critical!");
                if (Configuration.DirectCritical.ShowText)
                {
                    text2 = GenerateText(Configuration.DirectCritical);
                }

                if (Configuration.DirectCritical.PlaySound && (!Configuration.DirectCritical.SoundForActionsOnly || ActionDirectCriticalHit.Contains(kind)))
                {
                    SoundEngine.PlaySound(Configuration.DirectCritical.FilePath, volume: Configuration.DirectCritical.Volume * 0.01f);
                }
                
            }
            if (ActionCriticalHit.Contains(kind) || AutoCriticalHit.Contains(kind))
            {
                LogDebug("Critical!");
                if (Configuration.Critical.ShowText)
                {
                    text2 = GenerateText(Configuration.Critical);
                }

                if (Configuration.Critical.PlaySound  && (!Configuration.Critical.SoundForActionsOnly || ActionCriticalHit.Contains(kind)))
                {
                    SoundEngine.PlaySound(Configuration.Critical.FilePath, volume: Configuration.Critical.Volume * 0.01f);
                }
            }
            if (ActionDirectHit.Contains(kind) || AutoDirectHit.Contains(kind))
            {
                LogDebug("Direct hit!");
                if (Configuration.Direct.ShowText)
                {
                    text2 = GenerateText(Configuration.Direct);
                }

                if (Configuration.Direct.PlaySound  && (!Configuration.Direct.SoundForActionsOnly || ActionDirectHit.Contains(kind)))
                {
                    SoundEngine.PlaySound(Configuration.Direct.FilePath, volume: Configuration.Direct.Volume * 0.01f);
                }
            }
        }

        public static void GenerateTestFlyText(Configuration.SubConfiguration config)
        {
            var kind = config.Id switch
            {
                "directCritical" => FlyTextKind.NamedCriticalDirectHit,
                "critical" => FlyTextKind.NamedCriticalHit,
                "direct" => FlyTextKind.NamedDirectHit,
                _ => throw new ArgumentOutOfRangeException("Type of configuration not found")
            };
            LogDebug($"Kind: {kind}, Config ID: {config.Id}");
            Service.FlyTextGui.AddFlyText(kind, 1, 3333, 0, new SeStringBuilder().AddText("Stickybomb").Build(), GenerateText(config), 4278215139, 0, 60012);
        }

        private static SeString GenerateText(Configuration.SubConfiguration config)
        {
            LogDebug($"Generating text with colorKey {config.TextColor.ColorKey} and glowColorKey {config.TextColor.GlowColorKey}");
            var stringBuilder = new SeStringBuilder()
                                .AddUiForeground(config.TextColor.ColorKey)
                                .AddUiGlow(config.TextColor.GlowColorKey);
            if (config.Italics)
            {
                stringBuilder.AddItalics(config.Text);
            }
            else
            {
                stringBuilder.AddText(config.Text);
            }

            return stringBuilder.AddUiForegroundOff()
                                .AddUiGlowOff()
                                .Build();
        }


        public void Dispose()
        {
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
