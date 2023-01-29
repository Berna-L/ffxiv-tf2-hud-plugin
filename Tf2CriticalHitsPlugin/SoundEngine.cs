using System;
using System.IO;
using System.Linq;
using System.Threading;
using Dalamud.Logging;
using Dalamud.Utility;
using Lumina.Data;
using NAudio.Wave;

namespace Tf2CriticalHitsPlugin;

public static class SoundEngine
{
    // Copied from PeepingTom plugin, by ascclemens:
    // https://git.anna.lgbt/ascclemens/PeepingTom/src/commit/3749a6b42154a51397733abb2d3b06a47915bdcc/Peeping%20Tom/TargetWatcher.cs#L162
    public static void PlaySound(string? path, float volume = 1.0f)
    {
        if (path.IsNullOrEmpty() || !File.Exists(path))
        {
            PluginLog.Error($"Could not find file: {path}");
        }
        
        var soundDevice = DirectSoundOut.DSDEVID_DefaultPlayback;
        new Thread(() => {
            WaveStream reader;
            try {
                reader = new MediaFoundationReader(path);
            } catch (Exception e) {
                PluginLog.LogError(e.Message);
                return;
            }

            using var channel = new WaveChannel32(reader) {
                // prevent the user from bursting their eardrums if they decide to put an absurd value in the JSON
                Volume = Math.Min(volume, 1.0f),
                PadWithZeroes = false,
            };

            using (reader) {
                using var output = new DirectSoundOut(soundDevice);

                try {
                    output.Init(channel);
                    output.Play();

                    while (output.PlaybackState == PlaybackState.Playing) {
                        Thread.Sleep(500);
                    }
                } catch (Exception ex) {
                    PluginLog.LogError(ex, "Exception playing sound");
                }
            }
        }).Start();
        
    }
}
