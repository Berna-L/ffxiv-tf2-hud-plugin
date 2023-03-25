using System.IO;
using System.Linq;
using Dalamud.Utility;
using KamiLib.Configuration;
using NAudio.Wave;
using Sledge.Formats.Packages;

namespace Tf2Hud.Tf2Hud.Audio;

public class Tf2Sound
{

    public Setting<string> Tf2InstallFolder { private get; set; } = new("");

    public static readonly Tf2Sound Instance = new();
    
    public WaveAudio? VictorySound => ReadTf2SoundFile("sound/misc/your_team_won.wav");
    public WaveAudio? FailSound => ReadTf2SoundFile( "sound/misc/your_team_lost.wav");
    public WaveAudio? ScoredSound => ReadTf2SoundFile("sound/ui/scored.wav");
    
    public WaveAudio? ReadTf2SoundFile(string soundFilePath)
    {
        if (Tf2InstallFolder.Value.IsNullOrWhitespace()) return null;
        var tf2VpkPath = Path.Combine(Tf2InstallFolder.Value, "tf", "tf2_sound_misc_dir.vpk");
        if (!Path.Exists(tf2VpkPath)) return null;
        using var package = new VpkPackage(tf2VpkPath);
        return LoadSoundFile(package, soundFilePath);
    }

    private static WaveAudio LoadSoundFile(IPackage package, string filePath)
    {
        var file = package.Entries.First(e => e.Path == filePath);
        using var fileStream = package.Open(file);
        var result = new byte[fileStream.Length];
        fileStream.Read(result, 0, result.Length);
        
        using var memoryStream = new MemoryStream(result);
        using var waveFileReader = new WaveFileReader(memoryStream);
        var format = waveFileReader.WaveFormat;
        return new WaveAudio(result, format);
    }

}
