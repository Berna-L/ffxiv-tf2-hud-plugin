using System.Collections.Generic;
using Dalamud.Interface.ImGuiFileDialog;
using KamiLib.Interfaces;

namespace Tf2Hud.Common.Windows;

public abstract class SoundConfigurationTab<T> : ISelectionWindowTab
{
    internal readonly T Configuration;
    internal readonly FileDialogManager DialogManager;

    protected SoundConfigurationTab(string id, string tabName, T configuration, FileDialogManager dialogManager)
    {
        Id = id;
        Configuration = configuration;
        DialogManager = dialogManager;
        TabName = tabName;
    }

    public string Id { get; }
    public string TabName { get; }

    public ISelectable? LastSelection { get; set; }
    public abstract IEnumerable<ISelectable> GetTabSelectables();

    public virtual void DrawTabExtras() { }

    public void DrawCustomSoundSection() { }
}
