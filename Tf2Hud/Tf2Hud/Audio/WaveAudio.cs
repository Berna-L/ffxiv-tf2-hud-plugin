using NAudio.Wave;

namespace Tf2Hud.Tf2Hud.Audio;

public class WaveAudio
{
    public readonly byte[] Data;
    public readonly WaveFormat Format;
    public WaveAudio(byte[] data, WaveFormat format)
    {
        this.Data = data;
        this.Format = format;
    }
}
