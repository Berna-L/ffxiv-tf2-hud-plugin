using System;
using System.Collections.Generic;
using Lumina.Excel.GeneratedSheets;
using Tf2Hud.Common.Util;

namespace Tf2Hud.Tf2Hud.Model;

public enum Tf2Class
{
    Scout,
    Soldier,
    Pyro,
    Demoman,
    Heavy,
    Engineer,
    Medic,
    Sniper,
    Spy
}

public static class Tf2ClassHelper
{
    
    public static Tf2Class GetTf2ClassFromXivCombatClass(ClassJob classJob)
    {
        switch (classJob.Role)
        {
            case 1: // Tanks
                return Tf2Class.Heavy;
            case 4: // Healers
                return Tf2Class.Medic;
            default:
                switch (classJob.Abbreviation)
                {
                    case "MNK":
                    case "DRG":
                    case "SAM":
                    case "RPR":
                        return Tf2Class.Scout;
                    case "BLM":
                        // Fire for Pyro and Soldier for shooting projectiles and stuff
                        return new[] { Tf2Class.Soldier, Tf2Class.Pyro }.Random();
                    case "SMN":
                        // Summons other things to attack + utility for the team
                        return Tf2Class.Engineer;
                    case "RDM":
                        // Duality between long range and melee
                        return Tf2Class.Demoman;
                    case "BRD":
                    case "MCH":
                        // shoot shoot bang bang stab stab stab
                        return Tf2Class.Sniper;
                    case "NIN":
                        return Tf2Class.Spy;
                }
                // He's just the default dude in TF2.
                return Tf2Class.Soldier;
        }
    }
}
