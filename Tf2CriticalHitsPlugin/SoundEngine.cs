using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Dalamud.Logging;
using Dalamud.Utility;
using NAudio.Wave;
using Tf2CriticalHitsPlugin.Common;

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
    public static void PlaySound(string? path, bool useGameSfxVolume, int volume = 100, string? id = null)
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
                Volume = GetVolume(volume, useGameSfxVolume),
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

    private static float GetVolume(int baseVolume, bool applyGameSfxVolume)
    {
        return (Math.Min(baseVolume, 100) * (applyGameSfxVolume ? GameSettings.GetEffectiveSfxVolume() : 1)) / 100f;
    }
}
