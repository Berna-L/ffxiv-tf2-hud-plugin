using System.Collections.Generic;
using Dalamud.Interface.ImGuiFileDialog;
using KamiLib.Interfaces;

namespace Tf2CriticalHitsPlugin.Common.Windows;

public abstract class SoundConfigurationTab<T>: ISelectionWindowTab
{
    internal readonly T Configuration;
    internal readonly FileDialogManager DialogManager;

    protected SoundConfigurationTab(string id, string tabName, T configuration, FileDialogManager dialogManager)
    {
        this.Id = id;
        this.Configuration = configuration;
        this.DialogManager = dialogManager;
        TabName = tabName;
    }

    public string Id { get; }
    public string TabName { get; }

    public ISelectable? LastSelection { get; set; }
    public abstract IEnumerable<ISelectable> GetTabSelectables();

    public void DrawCustomSoundSection()
    {
        
    }
    
    public virtual void DrawTabExtras()
    {
    }
}
