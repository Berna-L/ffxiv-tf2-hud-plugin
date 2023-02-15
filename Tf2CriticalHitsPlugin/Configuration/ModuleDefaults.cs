using System;
using System.Collections.Generic;
using Tf2CriticalHitsPlugin.SeFunctions;

namespace Tf2CriticalHitsPlugin.Configuration;

public class ModuleDefaults
{
    public string SectionLabel { get; }
    public Sounds GameSound { get; set; }
    public FlyTextType FlyTextType { get; }
    public uint FlyTextColor { get; }
    public string DefaultText { get; }
    public FlyTextParameters FlyTextParameters { get; }

    private ModuleDefaults(ModuleType moduleType)
    {
        SectionLabel = GetModuleLabel(moduleType);
        GameSound = GetModuleGameSound(moduleType);
        FlyTextType = GetModuleFlyTextType(moduleType);
        FlyTextColor = GetModuleFlyTextColor(moduleType);
        DefaultText = GetModuleDefaultText(moduleType);
        FlyTextParameters = GetModuleDefaultTextParameters(moduleType);
    }

    private static readonly IDictionary<ModuleType, ModuleDefaults> ConstantsMap = new Dictionary<ModuleType, ModuleDefaults>
    {
        [ModuleType.DirectCriticalDamage] = new(ModuleType.DirectCriticalDamage),
        [ModuleType.CriticalDamage] = new(ModuleType.CriticalDamage),
        [ModuleType.CriticalHeal] = new(ModuleType.CriticalHeal),
        [ModuleType.DirectDamage] = new(ModuleType.DirectDamage)
    };

    public static ModuleDefaults GetDefaultsFromType(ModuleType moduleType) => ConstantsMap[moduleType];

    public static string GetModuleLabel(ModuleType moduleType) => moduleType switch
    {
        ModuleType.DirectCriticalDamage => "Direct Critical Damage",
        ModuleType.CriticalDamage => "Critical Damage",
        ModuleType.CriticalHeal => "Critical Heal",
        ModuleType.DirectDamage => "Direct Damage",
        _ => throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null)
    };

    private static Sounds GetModuleGameSound(ModuleType moduleType) => moduleType switch
    {
        ModuleType.DirectCriticalDamage => Sounds.Sound06,
        ModuleType.CriticalDamage => Sounds.Sound04,
        ModuleType.CriticalHeal => Sounds.Sound10,
        ModuleType.DirectDamage => Sounds.Sound16,
        _ => throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null)
    };

    public static FlyTextType GetModuleFlyTextType(ModuleType moduleType)
    {
        switch (moduleType)
        {
            case ModuleType.DirectCriticalDamage:
                return new FlyTextType(FlyTextType.AutoDirectCritical, FlyTextType.ActionDirectCritical);
            case ModuleType.CriticalDamage:
            case ModuleType.CriticalHeal:
                return new FlyTextType(FlyTextType.AutoCritical, FlyTextType.ActionCritical);
            case ModuleType.DirectDamage:
                return new FlyTextType(FlyTextType.AutoDirect, FlyTextType.ActionDirect);
            default:
                throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null);
        }
    }

    public static string GetModuleDefaultText(ModuleType moduleType) => moduleType switch
    {
        ModuleType.DirectCriticalDamage => "DIRECT CRITICAL HIT!",
        ModuleType.CriticalDamage => "CRITICAL HIT!",
        ModuleType.CriticalHeal => "CRITICAL HEAL!",
        ModuleType.DirectDamage => "Mini crit!",
        _ => throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null)
    };

    public static uint GetModuleFlyTextColor(ModuleType moduleType)
    {
        switch (moduleType)
        {
            case ModuleType.DirectCriticalDamage:
            case ModuleType.CriticalDamage:
            case ModuleType.DirectDamage:
                return Constants.DamageColor;
            case ModuleType.CriticalHeal:
                return Constants.HealColor;
            default:
                throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null);
        }
    }

    public static FlyTextParameters GetModuleDefaultTextParameters(ModuleType moduleType)
    {
        switch (moduleType)
        {
            case ModuleType.DirectCriticalDamage:
            case ModuleType.CriticalDamage:
            case ModuleType.CriticalHeal:
                return new FlyTextParameters(60, 7, true);
            case ModuleType.DirectDamage:
                return new FlyTextParameters(0, 0, false);
            default:
                throw new ArgumentOutOfRangeException(nameof(moduleType), moduleType, null);
        }
    }
}
