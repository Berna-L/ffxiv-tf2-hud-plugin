using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private static readonly IDictionary<string, bool> SoundState = new ConcurrentDictionary<string, bool>();

    public static void StopSound(string id)
    {
        SoundState[id] = false;
    }
    
    // Copied from PeepingTom plugin, by ascclemens:
    // https://git.anna.lgbt/ascclemens/PeepingTom/src/commit/3749a6b42154a51397733abb2d3b06a47915bdcc/Peeping%20Tom/TargetWatcher.cs#L162
    public static void PlaySound(string? path, float volume = 1.0f, string? id = null)
    {
        if (path.IsNullOrEmpty() || !File.Exists(path))
        {
            PluginLog.Error($"Could not find file: {path}");
        }

        var soundDevice = DirectSoundOut.DSDEVID_DefaultPlayback;
        new Thread(() =>
        {
            WaveStream reader;
            try
            {
                reader = new MediaFoundationReader(path);
            }
            catch (Exception e)
            {
                PluginLog.LogError(e.Message);
                return;
            }
            PluginLog.LogDebug(reader.TotalTime.ToString());
            using var channel = new WaveChannel32(reader)
            {
                // prevent the user from bursting their eardrums if they decide to put an absurd value in the JSON
                Volume = Math.Min(volume, 1.0f),
                PadWithZeroes = false,
            };

            using (reader)
            {
                using var output = new DirectSoundOut(soundDevice);

                try
                {
                    output.Init(channel);
                    output.Play();
                    if (id is not null)
                    {
                        SoundState[id] = true;
                    }

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        if (id is not null && SoundState[id] == false)
                        {
                            output.Stop();
                        }

                        Thread.Sleep(500);
                    }

                    if (id is not null)
                    {
                        SoundState[id] = false;
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex, "Exception playing sound");
                }
            }
        }).Start();
    }
}
