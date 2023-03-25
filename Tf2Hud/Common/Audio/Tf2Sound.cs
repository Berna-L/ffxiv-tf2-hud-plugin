using System.IO;
using System.Linq;
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

    public Audio? VictorySound => ReadMiscTf2SoundFile("sound/misc/your_team_won.wav");
    public Audio? FailSound => ReadMiscTf2SoundFile("sound/misc/your_team_lost.wav");
    public Audio? ScoredSound => ReadMiscTf2SoundFile("sound/ui/scored.wav");

    public Audio? RandomMannUpSound => ReadVoiceTf2SoundFile(MannUpSounds.Random());

    private static string[] MannUpSounds => Enumerable.Range(1, 15)
                                                      .Select(i => i.ToString().PadLeft(2, '0'))
                                                      .Select(i => $"sound/vo/mvm_mann_up_mode{i}.mp3")
                                                      .ToArray();

    private Audio? ReadVoiceTf2SoundFile(string soundFilePath)
    {
        return ReadTf2SoundFile("tf2_sound_vo_english_dir.vpk", soundFilePath);
    }

    private Audio? ReadMiscTf2SoundFile(string soundFilePath)
    {
        return ReadTf2SoundFile("tf2_sound_misc_dir.vpk", soundFilePath);
    }

    private Audio? ReadTf2SoundFile(string packagePath, string soundFilePath)
    {
        if (Tf2InstallFolder.Value.IsNullOrWhitespace()) return null;
        var tf2VpkPath = Path.Combine(Tf2InstallFolder.Value, "tf", packagePath);
        if (!Path.Exists(tf2VpkPath)) return null;
        using var package = new VpkPackage(tf2VpkPath);
        return LoadSoundFile(package, soundFilePath);
    }

    private static Audio? LoadSoundFile(IPackage package, string filePath)
    {
        switch (Path.GetExtension(filePath).ToLower())
        {
            case ".wav":
                return LoadWavFile(package, filePath);
            case ".mp3":
                return LoadMp3File(package, filePath);
        }

        return null;
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
