using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Utility;
using KamiLib.Configuration;
using NAudio.Wave;
using Sledge.Formats.Packages;
using Tf2Hud.Common.Util;

namespace Tf2Hud.Common.Audio;

public class Tf2Sound
{
    public static readonly Tf2Sound Instance = new();

    public Setting<string> Tf2InstallFolder { private get; set; } = new("");

    public Task<Audio?> VictorySound => ReadMiscTf2SoundFile("sound/misc/your_team_won.wav");
    public Task<Audio?> FailSound => ReadMiscTf2SoundFile("sound/misc/your_team_lost.wav");
    public Task<Audio?> ScoredSound => ReadMiscTf2SoundFile("sound/ui/scored.wav");

    public Task<Audio?> RandomMannUpSound => ReadVoiceTf2SoundFile(MannUpSounds.Random());

    private static string[] MannUpSounds => Enumerable.Range(1, 15)
                                                      .Select(i => i.ToString().PadLeft(2, '0'))
                                                      .Select(i => $"sound/vo/mvm_mann_up_mode{i}.mp3")
                                                      .ToArray();

    public Task<Audio?> RandomGoSound => ReadVoiceTf2SoundFile(GoSounds.Random());

    private static IDictionary<int, string[]> CountdownSounds => new[]
    {
        new KeyValuePair<int, string[]>(1, new[] { "sound/vo/compmode/cm_admin_compbegins01.mp3" }),
        new KeyValuePair<int, string[]>(2, new[] { "sound/vo/compmode/cm_admin_compbegins02.mp3" }),
        new KeyValuePair<int, string[]>(3, new[] { "sound/vo/compmode/cm_admin_compbegins03.mp3" }),
        new KeyValuePair<int, string[]>(4, new[] { "sound/vo/compmode/cm_admin_compbegins04.mp3" }),
        new KeyValuePair<int, string[]>(5, new[] { "sound/vo/compmode/cm_admin_compbegins05.mp3" }),
        new KeyValuePair<int, string[]>(10, new[]
        {
            "sound/vo/compmode/cm_admin_compbegins10_01.mp3",
            "sound/vo/compmode/cm_admin_compbegins10_02.mp3",
            "sound/vo/compmode/cm_admin_compbegins10_rare_01.mp3",
            "sound/vo/compmode/cm_admin_compbegins10_rare_02.mp3",
            "sound/vo/compmode/cm_admin_compbegins10_rare_03.mp3",
            "sound/vo/announcer_dec_missionbegins10s01.mp3"
        })
    }.ToImmutableDictionary();

    private static string[] GoSounds => Enumerable.Range(1, 7)
                                                  .Select(i => i.ToString().PadLeft(2, '0'))
                                                  .Select(i => $"sound/vo/compmode/cm_admin_compbeginsstart_{i}.mp3")
                                                  .ToArray();

    public Task<Audio?> FiveMinutesLeftSound => ReadVoiceTf2SoundFile("sound/vo/announcer_ends_5min.mp3");

    public Task<Audio?> RandomCountdownSound(int i)
    {
        return Task.Run(() => !CountdownSounds.ContainsKey(i)
                                  ? null
                                  : ReadVoiceTf2SoundFile(CountdownSounds[i].Random()));
    }


    private Task<Audio?> ReadVoiceTf2SoundFile(string soundFilePath)
    {
        return ReadTf2SoundFile("tf2_sound_vo_english_dir.vpk", soundFilePath);
    }

    private Task<Audio?> ReadMiscTf2SoundFile(string soundFilePath)
    {
        return ReadTf2SoundFile("tf2_sound_misc_dir.vpk", soundFilePath);
    }

    private Task<Audio?> ReadTf2SoundFile(string packagePath, string soundFilePath)
    {
        return Task.Run(() =>
        {
            if (Tf2InstallFolder.Value.IsNullOrWhitespace()) return null;
            var tf2VpkPath = Path.Combine(Tf2InstallFolder.Value, "tf", packagePath);
            if (!Path.Exists(tf2VpkPath)) return null;
            using var package = new VpkPackage(tf2VpkPath);
            return LoadSoundFile(package, soundFilePath);
        });
    }

    private static Audio? LoadSoundFile(IPackage package, string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".wav" => LoadWavFile(package, filePath),
            ".mp3" => LoadMp3File(package, filePath),
            _ => null
        };
    }

    private static Audio LoadWavFile(IPackage package, string filePath)
    {
        var file = package.Entries.First(e => e.Path == filePath);
        using var fileStream = package.Open(file);
        var result = new byte[fileStream.Length];
        fileStream.Read(result, 0, result.Length);

        using var memoryStream = new MemoryStream(result);
        using var waveFileReader = new WaveFileReader(memoryStream);
        var format = waveFileReader.WaveFormat;
        return new Audio(result, WaveFormatEncoding.Pcm, format);
    }

    private static Audio? LoadMp3File(IPackage package, string filePath)
    {
        var file = package.Entries.First(e => e.Path == filePath);
        using var fileStream = package.Open(file);
        var result = new byte[fileStream.Length];
        fileStream.Read(result, 0, result.Length);

        using var memoryStream = new MemoryStream(result);
        using var waveFileReader = new Mp3FileReader(memoryStream);
        var format = waveFileReader.WaveFormat;
        return new Audio(result, WaveFormatEncoding.MpegLayer3, format);
    }
}
