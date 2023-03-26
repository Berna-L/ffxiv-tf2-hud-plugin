using System.Linq;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;

namespace Tf2Hud.VoiceLines.Windows;

public class VoiceLineConfigPane: ModuleConfigPane<ConfigZero.VoiceLinesConfigZero>
{
    public VoiceLineConfigPane(ConfigZero.VoiceLinesConfigZero configZero) : base("Voice Lines", configZero) { }
    public override void Draw()
    {
        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", Config.Enabled)
            .StartConditional(Config.Enabled)
            .AddConfigCheckbox("Surprise Me!", Config.SurpriseMe,
                               "With this enabled, you won't know what can trigger a category of TF2 voice lines until you the trigger is, well, triggered." +
                               "\n\nCheck the text at the end to know how many triggers are left to find!" +
                               "\nOr disable this checkbox if surprises aren't your thing (no judgement!)")
            .EndConditional()
            .Draw();
        
        foreach (var heardTrigger in Config.Triggers.Values.Where(ShowConfig))
        {
            InfoBox.Instance
                   .AddTitle(heardTrigger.Name)
                   .AddString(heardTrigger.Description)
                   .AddConfigCheckbox("Enabled", heardTrigger.Enabled, additionalID: heardTrigger.Name)
                   .BeginDisabled(true)
                   .AddString(GetTriggeredText(heardTrigger))
                   .EndDisabled()
                   .Draw();
        }
        
        new SimpleDrawList()
            .StartConditional(Config.Enabled && Config.SurpriseMe)
            .AddStringCentered(GetRemainingText())
            .EndConditional()
            .Draw();
    }

    private bool ShowConfig(ConfigZero.VoiceLinesConfigZero.VoiceLineTrigger trigger)
    {
        return  Config.Enabled && (!Config.SurpriseMe || trigger.Heard);
    }

    private string GetRemainingText()
    {
        var count = Config.Triggers.Values.Count(t => !t.Heard);
        return count switch
        {
            0 => "You found them all!",
            1 => "1 trigger remaining to find!",
            _ => $"{count} triggers remaining to find!"
        };
    }

    private string GetTriggeredText(ConfigZero.VoiceLinesConfigZero.VoiceLineTrigger heardTrigger)
    {
        return heardTrigger.Heard ? "You have triggered this in the past." : "You have yet to hear this :)";
    }
}
