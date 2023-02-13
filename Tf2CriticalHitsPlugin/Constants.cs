using System.Collections.Generic;
using System.Collections.Immutable;
using Dalamud.Game.Gui.FlyText;
using static Dalamud.Game.Gui.FlyText.FlyTextKind;

namespace Tf2CriticalHitsPlugin;

public abstract class Constants
{

    public const uint CurrentConfigVersion = 1;
    
    public const uint DamageColor = 4278215139;
    public const uint HealColor = 4278213930;

    public const int MaxTextLength = 40;

    public static readonly string[] TestFlavorText =
        { "Stickybomb", "Meatshot 8)", "Crocket", "Lucksman", "360 Noscope", "Reflected Rocket", "Random crit lol" };

    public abstract class FlyText
    {
        public static readonly ISet<FlyTextKind> AutoDirectCritical = new[] { CriticalDirectHit, CriticalDirectHit2 }.ToImmutableHashSet();

        public static readonly ISet<FlyTextKind> ActionDirectCritical =
            new[] { NamedCriticalDirectHit }.ToImmutableHashSet();

        public static readonly ISet<FlyTextKind> AutoCritical =
            new[] { CriticalHit, CriticalHit2, CriticalHit3, CriticalHit4 }.ToImmutableHashSet();

        public static readonly ISet<FlyTextKind> ActionCritical = new[]
                { NamedCriticalHit, NamedCriticalHit2, NamedCriticalHitWithMp, NamedCriticalHitWithTp }
            .ToImmutableHashSet();

        public static readonly ISet<FlyTextKind> AutoDirect = new[] { DirectHit, DirectHit2 }.ToImmutableHashSet();

        public static readonly ISet<FlyTextKind> ActionDirect = new[] { NamedDirectHit }.ToImmutableHashSet();
    }
}
