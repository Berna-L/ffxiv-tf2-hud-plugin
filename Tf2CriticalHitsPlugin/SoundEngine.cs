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
    private static readonly IDictionary<string, byte> SoundState = new ConcurrentDictionary<string, byte>();

    public static bool IsPlaying(string id)
    {
        return SoundState.ContainsKey(id);
    }
    
    public static void StopSound(string id)
    {
        SoundState.Remove(id);
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
                        SoundState[id] = 1;
                    }

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        if (id is not null && !SoundState.ContainsKey(id))
                        {
                            output.Stop();
                        }

                        Thread.Sleep(500);
                    }

                    if (id is not null)
                    {
                        SoundState.Remove(id);
                    }
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex, "Exception playing sound");
                }
            }
        }).Start();
    }
    
    public static void PlaySound(byte[] sound, bool useGameSfxVolume, int volume = 100, int sampleRate = 44100, int channels = 2, string? id = null)
    {
        var soundDevice = DirectSoundOut.DSDEVID_DefaultPlayback;
        new Thread(() =>
        {
            var wave = new RawSourceWaveStream(sound, 0, sound.Length, new WaveFormat(sampleRate, channels));
            using var channel = new WaveChannel32(wave)
            {
                Volume = GetVolume(volume, useGameSfxVolume),
                PadWithZeroes = false,
            };

            using (wave)
            {
                using var output = new DirectSoundOut(soundDevice);

                try
                {
                    output.Init(channel);
                    output.Play();
                    if (id is not null)
                    {
                        SoundState[id] = 1;
                    }

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        if (id is not null && !SoundState.ContainsKey(id))
                        {
                            output.Stop();
                        }

                        Thread.Sleep(500);
                    }

                    if (id is not null)
                    {
                        SoundState.Remove(id);
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
