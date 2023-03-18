using System.Collections.Generic;
using Tf2CriticalHitsPlugin.Common.Configuration;

namespace Tf2CriticalHitsPlugin.Countdown.Configuration;

public class CountdownConfigZero: BaseConfiguration
{

    public IList<CountdownConfigZeroModule> modules { get; init; } = new List<CountdownConfigZeroModule>();
    
    public CountdownConfigZero()
    {
        Version = 0;
    }
}
