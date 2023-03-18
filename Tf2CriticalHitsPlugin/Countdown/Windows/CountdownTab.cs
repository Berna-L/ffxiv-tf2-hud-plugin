using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using ImGuiNET;
using KamiLib;
using KamiLib.Interfaces;
using Tf2CriticalHitsPlugin.Common.Window;
using Tf2CriticalHitsPlugin.Countdown.Configuration;

namespace Tf2CriticalHitsPlugin.Countdown.Windows;

public class CountdownTab: SoundConfigurationTab<CountdownConfigZero>
{
    public CountdownTab(CountdownConfigZero configuration, FileDialogManager dialogManager) : base("countdown", "Countdown Jams", configuration, dialogManager) { }
    
    public override IEnumerable<ISelectable> GetTabSelectables()
    {
        return Configuration.modules
                            .Select(m => new CountdownOption(m, DialogManager));
    }

    public override void DrawTabExtras()
    {
        DrawNewButton();
    }
    
    private void DrawNewButton()
    {
        if (ImGui.Button("Create new configuration", new Vector2(ImGui.GetContentRegionAvail().X, 20.0f)))
        {
            Configuration.modules.Add(CountdownConfigZeroModule.Create("Untitled"));
        }
    }

}
