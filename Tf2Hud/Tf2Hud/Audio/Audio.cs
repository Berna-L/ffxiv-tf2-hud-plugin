using NAudio.Wave;

namespace Tf2Hud.Tf2Hud.Audio;

public class Audio
{
    public readonly byte[] Data;
    public readonly WaveFormatEncoding Encoding;
    public readonly WaveFormat Metadata;
    public Audio(byte[] data, WaveFormatEncoding encoding, WaveFormat metadata)
    {
        this.Data = data;
        // WaveFormat has type, but it doesn't seem to be that reliable.
        this.Encoding = encoding;
        this.Metadata = metadata;
    }
}
