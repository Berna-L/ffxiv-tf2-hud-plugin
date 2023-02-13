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

    public static readonly ISet<FlyTextKind> AutoDirectCritical =
        new[] { FlyTextKind.CriticalDirectHit, FlyTextKind.CriticalDirectHit2 }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionDirectCritical =
        new[] { FlyTextKind.NamedCriticalDirectHit }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> AutoCritical =
        new[]
        {
            FlyTextKind.CriticalHit, FlyTextKind.CriticalHit2, FlyTextKind.CriticalHit3, FlyTextKind.CriticalHit4
        }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionCritical = new[]
        {
            FlyTextKind.NamedCriticalHit, FlyTextKind.NamedCriticalHit2, FlyTextKind.NamedCriticalHitWithMp,
            FlyTextKind.NamedCriticalHitWithTp
        }
        .ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> AutoDirect =
        new[] { FlyTextKind.DirectHit, FlyTextKind.DirectHit2 }.ToImmutableHashSet();

    public static readonly ISet<FlyTextKind> ActionDirect =
        new[] { FlyTextKind.NamedDirectHit }.ToImmutableHashSet();
}
