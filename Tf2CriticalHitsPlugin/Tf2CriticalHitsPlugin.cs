using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Tf2CriticalHitsPlugin.Windows;

namespace Tf2CriticalHitsPlugin
{
    public sealed class Tf2CriticalHitsPlugin : IDalamudPlugin
    {
        public string Name => "TF2-ish Critical Hits";
        private const string CommandName = "/critconfig";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("Tf2CriticalHitsPlugin");
        
        private readonly FlyTextGui flyTextGui;
        
        private static readonly ISet<FlyTextKind> CriticalHitKinds = new HashSet<FlyTextKind>();
        private static readonly ISet<FlyTextKind> DirectHitKinds = new HashSet<FlyTextKind>();

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

            AddCriticalHitKinds();

            DirectHitKinds.Add(FlyTextKind.DirectHit);
            DirectHitKinds.Add(FlyTextKind.DirectHit2);

            DirectHitKinds.Add(FlyTextKind.NamedDirectHit);

            this.flyTextGui.FlyTextCreated += this.FlyTextCreate;

        }

        private static void AddCriticalHitKinds()
        {
            CriticalHitKinds.Add(FlyTextKind.CriticalDirectHit);
            CriticalHitKinds.Add(FlyTextKind.CriticalDirectHit2);

            CriticalHitKinds.Add(FlyTextKind.CriticalHit);
            CriticalHitKinds.Add(FlyTextKind.CriticalHit2);
            CriticalHitKinds.Add(FlyTextKind.CriticalHit3);
            CriticalHitKinds.Add(FlyTextKind.CriticalHit4);

            CriticalHitKinds.Add(FlyTextKind.NamedCriticalHit);
            CriticalHitKinds.Add(FlyTextKind.NamedCriticalHit2);

            CriticalHitKinds.Add(FlyTextKind.NamedCriticalDirectHit);
        }

        public void FlyTextCreate(
            ref FlyTextKind kind,
            ref int val1,
            ref int val2,
            ref SeString text1,
            ref SeString text2,
            ref uint color,
            ref uint icon,
            ref uint third,
            ref float yOffset,
            ref bool handled)
        {
            if (CriticalHitKinds.Contains(kind))
            {
                Dalamud.Logging.PluginLog.LogDebug("Critical!");
                if (Configuration.Critical.ShowText)
                {
                    text2 = GenerateCriticalHitText();
                }

                if (Configuration.Critical.PlaySound)
                {
                    SoundEngine.PlaySound(Configuration.Critical.FilePath, volume: Configuration.Critical.Volume * 0.01f);
                }
            }
            if (DirectHitKinds.Contains(kind))
            {
                Dalamud.Logging.PluginLog.LogDebug("Direct hit!");
                if (Configuration.Direct.ShowText)
                {
                    text2 = GenerateDirectHitText();
                }

                if (Configuration.Direct.PlaySound)
                {
                    SoundEngine.PlaySound(Configuration.Direct.FilePath, volume: Configuration.Direct.Volume * 0.01f);
                }
            }
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
