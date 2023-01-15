using System;
using System.Collections.Generic;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using CritPlugin.Windows;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;

namespace CritPlugin
{
    public sealed class CritPlugin : IDalamudPlugin
    {
        public string Name => "Crit Plugin";
        private const string CommandName = "/critconfig";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("CritPlugin");
        
        private readonly FlyTextGui flyTextGui;
        private static readonly ISet<FlyTextKind> CriticalKinds = new HashSet<FlyTextKind>();
        private static readonly string CritString = "CRITICAL HIT!";
        private static readonly SeString critText = new SeStringBuilder().AddUiForeground(60).AddUiGlow(100).AddItalics(CritString).AddUiForegroundOff().AddUiGlowOff().Build();

        private static readonly ISet<FlyTextKind> MiniCritKinds = new HashSet<FlyTextKind>();
        private static readonly string MiniCritString = "Mini crit!";
        private static readonly SeString MiniCritText = new SeStringBuilder().AddText(MiniCritString).Build();


        public CritPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] FlyTextGui flyText)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            WindowSystem.AddWindow(new ConfigWindow(this));

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Opens the Crit Plugin configuration window."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUserInterface;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigWindow;
            
            this.flyTextGui = flyText;

            CriticalKinds.Add(FlyTextKind.CriticalDirectHit);
            CriticalKinds.Add(FlyTextKind.CriticalDirectHit2);

            CriticalKinds.Add(FlyTextKind.CriticalHit);
            CriticalKinds.Add(FlyTextKind.CriticalHit2);
            CriticalKinds.Add(FlyTextKind.CriticalHit3);
            CriticalKinds.Add(FlyTextKind.CriticalHit4);

            CriticalKinds.Add(FlyTextKind.NamedCriticalHit);
            CriticalKinds.Add(FlyTextKind.NamedCriticalHit2);

            CriticalKinds.Add(FlyTextKind.NamedCriticalDirectHit);

            MiniCritKinds.Add(FlyTextKind.DirectHit);
            MiniCritKinds.Add(FlyTextKind.DirectHit2);

            MiniCritKinds.Add(FlyTextKind.NamedDirectHit);

            this.flyTextGui.FlyTextCreated += this.FlyTextCreate;

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
            if (CriticalKinds.Contains(kind))
            {
                Dalamud.Logging.PluginLog.LogDebug("Critical!");
                if (Configuration.Critical.ShowText)
                {
                    text2 = critText;
                }

                if (Configuration.Critical.PlaySound)
                {
                    SoundEngine.PlaySound(Configuration.Critical.FilePath, volume: Configuration.Critical.Volume * 0.01f);
                }
            }
            if (MiniCritKinds.Contains(kind))
            {
                Dalamud.Logging.PluginLog.LogDebug("Direct hit!");
                if (Configuration.Direct.ShowText)
                {
                    text2 = MiniCritText;
                }

                if (Configuration.Direct.PlaySound)
                {
                    SoundEngine.PlaySound(Configuration.Direct.FilePath, volume: Configuration.Direct.Volume * 0.01f);
                }
            }
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
