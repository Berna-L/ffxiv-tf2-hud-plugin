using System.Collections.Generic;
using System.Collections.Immutable;
using Dalamud.Game.Gui.FlyText;

namespace Tf2CriticalHitsPlugin.Configuration;

public class FlyTextType
{
    public ISet<FlyTextKind> AutoAttack { get; }

    public ISet<FlyTextKind> Action { get; }

    public FlyTextType(ISet<FlyTextKind> autoAttack, ISet<FlyTextKind> action)
    {
        AutoAttack = autoAttack;
        Action = action;
    }

    public static readonly ISet<FlyTextKind> AutoDirectCriticalDamage =
        new[] { FlyTextKind.CriticalDirectHit, FlyTextKind.CriticalDirectHit2 }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionDirectCriticalDamage =
        new[] { FlyTextKind.NamedCriticalDirectHit }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> AutoCriticalDamage = new[]
            { FlyTextKind.CriticalHit, FlyTextKind.CriticalHit2, FlyTextKind.CriticalHit3, FlyTextKind.CriticalHit4 }
        .ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionCriticalDamage = new[]
            { FlyTextKind.NamedCriticalHit, FlyTextKind.NamedCriticalHitWithMp, FlyTextKind.NamedCriticalHitWithTp }
        .ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionCriticalHeal =
        new[] { FlyTextKind.NamedCriticalHit2 }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> AutoDirectDamage =
        new[] { FlyTextKind.DirectHit, FlyTextKind.DirectHit2 }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionDirectDamage =
        new[] { FlyTextKind.NamedDirectHit }.ToImmutableHashSet();
}
