using NAudio.Wave;

namespace Tf2Hud.Common.Audio;

public class Audio
{
    public readonly byte[] Data;
    public readonly WaveFormatEncoding Encoding;
    public readonly WaveFormat Metadata;

    public Audio(byte[] data, WaveFormatEncoding encoding, WaveFormat metadata)
    {
        Data = data;
        // WaveFormat has type, but it doesn't seem to be that reliable.
        Encoding = encoding;
        Metadata = metadata;
    }
}
