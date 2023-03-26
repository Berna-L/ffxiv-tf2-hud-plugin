using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using NAudio.Wave;
using Tf2Hud.Common;
using Tf2Hud.Common.Audio;

namespace Tf2Hud;

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

    public static void PlaySoundAsync(
        Task<Audio?> audioTask, bool useGameSfxVolume, int volume = 100, string? id = null)
    {
        audioTask.ContinueWith(past =>
        {
            if (past.IsCompletedSuccessfully) PlaySound(past.Result, useGameSfxVolume, volume, id);
        });
    }

    // Copied from PeepingTom plugin, by ascclemens:
    // https://git.anna.lgbt/ascclemens/PeepingTom/src/commit/3749a6b42154a51397733abb2d3b06a47915bdcc/Peeping%20Tom/TargetWatcher.cs#L162
    public static void PlaySound(Audio? waveAudio, bool useGameSfxVolume, int volume = 100, string? id = null)
    {
        if (waveAudio is null) return;
        var soundDevice = DirectSoundOut.DSDEVID_DefaultPlayback;
        new Thread(() =>
        {
            var wave = GetReader(waveAudio);
            using var channel = new WaveChannel32(wave)
            {
                Volume = GetVolume(volume, useGameSfxVolume),
                PadWithZeroes = false
            };

            using (wave)
            {
                using var output = new DirectSoundOut(soundDevice);

                try
                {
                    output.Init(channel);
                    output.Play();
                    if (id is not null) SoundState[id] = 1;

                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        if (id is not null && !SoundState.ContainsKey(id)) output.Stop();

                        Thread.Sleep(500);
                    }

                    if (id is not null) SoundState.Remove(id);
                }
                catch (Exception ex)
                {
                    PluginLog.LogError(ex, "Exception playing sound");
                }
            }
        }).Start();
    }

    private static WaveStream GetReader(Audio audio)
    {
        if (audio.Encoding == WaveFormatEncoding.MpegLayer3) return new Mp3FileReader(new MemoryStream(audio.Data));
        return new RawSourceWaveStream(audio.Data, 0, audio.Data.Length, audio.Metadata);
    }


    private static float GetVolume(int baseVolume, bool applyGameSfxVolume)
    {
        return Math.Min(baseVolume, 100) * (applyGameSfxVolume ? GameSettings.GetEffectiveSfxVolume() : 1) / 100f;
    }
}
