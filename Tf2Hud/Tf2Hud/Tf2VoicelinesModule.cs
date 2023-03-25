using System;
using Tf2Hud.Common.Configuration;

namespace Tf2Hud.Tf2Hud;

public class Tf2VoiceLinesModule: IDisposable
{
    private readonly ConfigZero configZero;

    public Tf2VoiceLinesModule(ConfigZero configZero)
    {
        this.configZero = configZero;
    }

    public void Dispose()
    {
        
    }
}
