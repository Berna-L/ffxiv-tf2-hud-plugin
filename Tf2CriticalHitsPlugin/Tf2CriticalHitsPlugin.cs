using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Tf2CriticalHitsPlugin.Windows;
using static Dalamud.Logging.PluginLog;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => "TF2-ish Critical Hits";
        private const string CommandName = "/critconfig";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("TF2CriticalHitsPlugin");
        
        private readonly FlyTextGui flyTextGui;

        private static readonly ISet<FlyTextKind> AutoDirectCriticalHit = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> ActionDirectCriticalHit = new HashSet<FlyTextKind>();

        private static readonly ISet<FlyTextKind> AutoCriticalHit = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> ActionCriticalHit = new HashSet<FlyTextKind>();

        private static readonly ISet<FlyTextKind> AutoDirectHit = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> ActionDirectHit = new HashSet<FlyTextKind>();

        public Tf2CriticalHitsPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] FlyTextGui flyText)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            
            WindowSystem.AddWindow(new ConfigWindow(this));

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the TF2-ish Critical Hits configuration window"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;
            
            this.flyTextGui = flyText;

            AddDirectCriticalHitKinds();
            AddCriticalHitKinds();
            AddDirectHitKinds();

            this.flyTextGui.FlyTextCreated += this.FlyTextCreate;

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
            if (ActionDirectCriticalHit.Contains(kind) || AutoDirectCriticalHit.Contains(kind))
            {
                LogDebug("Direct critical!");
                if (Configuration.DirectCritical.ShowText)
                {
                    text2 = GenerateDirectCriticalHitText();
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
                    text2 = GenerateCriticalHitText();
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
                    text2 = GenerateDirectHitText();
                }

                if (Configuration.Direct.PlaySound  && (!Configuration.Direct.SoundForActionsOnly || ActionDirectHit.Contains(kind)))
                {
                    SoundEngine.PlaySound(Configuration.Direct.FilePath, volume: Configuration.Direct.Volume * 0.01f);
                }
            }
        }
        
        private SeString GenerateDirectCriticalHitText()
        {
            return new SeStringBuilder().AddUiForeground(60)
                                        .AddUiGlow(100)
                                        .AddItalics(Configuration.DirectCritical.Text)
                                        .AddUiForegroundOff()
                                        .AddUiGlowOff()
                                        .Build();
        }

        
        private SeString GenerateCriticalHitText()
        {
            return new SeStringBuilder().AddUiForeground(60)
                                        .AddUiGlow(100)
                                        .AddItalics(Configuration.Critical.Text)
                                        .AddUiForegroundOff()
                                        .AddUiGlowOff()
                                        .Build();
        }
        
        private SeString GenerateDirectHitText()
        {
            return new SeStringBuilder().AddText(Configuration.Direct.Text).Build();
        }



        public void Dispose()
        {
            flyTextGui.FlyTextCreated -= FlyTextCreate;
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(CommandName);
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
