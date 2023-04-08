using System.Collections.Generic;
using System.Linq;
using KamiLib.Drawing;
using Tf2Hud.Common.Configuration;
using Tf2Hud.Common.Windows;
using static Tf2Hud.Common.Configuration.ConfigZero.VoiceLinesConfigZero;

namespace Tf2Hud.VoiceLines.Windows;

public class VoiceLineConfigPane : ModuleConfigPane<ConfigZero.VoiceLinesConfigZero>
{
    public VoiceLineConfigPane(ConfigZero.VoiceLinesConfigZero configZero) : base("Voice Lines", configZero) { }

    private IEnumerable<VoiceLineTrigger> Triggers => new[]
    {
        Config.MannUp, Config.AdministratorCountdown, Config.FiveMinutesLeft, Config.AugmentationToken,
        Config.Revive
    }.OrderBy(t => t.Name);

    public override void Draw()
    {
        new SimpleDrawList()
            .AddConfigCheckbox("Enabled", Config.Enabled)
            .StartConditional(Config.Enabled)
            .AddConfigCheckbox("Surprise Me!", Config.SurpriseMe,
                               "With this enabled, you won't know what can trigger a category" +
                               "\nof TF2 voice lines until it happens." +
                               "\n\nCheck the text at the end to know how many triggers are left to find!" +
                               "\nOr disable this checkbox if surprises aren't your thing (no judgement!)")
            .EndConditional()
            .Draw();

        foreach (var heardTrigger in Triggers.Where(ShowConfig))
            InfoBox.Instance
                   .AddTitle(heardTrigger.Name)
                   .AddString(heardTrigger.Description)
                   .AddConfigCheckbox("Enabled", heardTrigger.Enabled, additionalID: heardTrigger.Name)
                   .BeginDisabled(true)
                   .AddString(GetTriggeredText(heardTrigger))
                   .SameLine()
                   .EndDisabled()
                   .AddButton(GetHeardSwapText(heardTrigger),
                              () => heardTrigger.Heard.Value = !heardTrigger.Heard.Value)
                   .Draw();

        new SimpleDrawList()
            .StartConditional(Config.Enabled && Config.SurpriseMe)
            .AddStringCentered(GetRemainingText())
            .EndConditional()
            .Draw();
    }

    private bool ShowConfig(VoiceLineTrigger trigger)
    {
        return Config.Enabled && (!Config.SurpriseMe || trigger.Heard);
    }

    private string GetRemainingText()
    {
        var count = Triggers.Count(t => !t.Heard);
        return count switch
        {
            0 => "You found them all!",
            1 => "1 trigger remaining to find!",
            _ => $"{count} triggers remaining to find!"
        };
    }

    private string GetTriggeredText(VoiceLineTrigger heardTrigger)
    {
        return heardTrigger.Heard ? "You have triggered this in the past." : "You have yet to hear this :)";
    }

    private string GetHeardSwapText(VoiceLineTrigger heardTrigger)
    {
        return heardTrigger.Heard ? "No I haven't" : "I heard it though";
    }
}
