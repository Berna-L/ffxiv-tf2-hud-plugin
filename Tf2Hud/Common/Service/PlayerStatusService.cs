﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Hooking;


namespace Tf2Hud.Common.Service;

public unsafe class PlayerStatusService: IDisposable
{

    public enum ReviveType
    {
        Normal,
        LimitBreak
    }
    
    private delegate void AddToScreenLogWithScreenLogKindDelegate(
        Character* target,
        Character* source,
        FlyTextKind logKind,
        byte option,
        byte actionKind,
        int actionId,
        int val1,
        int val2,
        byte damageType);
    private readonly Hook<AddToScreenLogWithScreenLogKindDelegate>? addToScreenLogWithScreenLogKindHook;

    
    private const uint Weakness = 43;
    private const uint BrinkOfDeath = 44;

    private static readonly ISet<uint> HealerLimitBreakThree = new[]
    {
        208u,  // Pulse of Life (WHM)
        4247u, // Angel Feathers (SCH)
        4248u, // Astral Stasis (AST)
        24859u // Techno Pigeon, I mean, Techne Makre (SGE)
    }.ToImmutableHashSet();

    public delegate void LastOneAlive();

    public event LastOneAlive? OnLastOneAlive;
    public event EventHandler<ReviveType>? OnRevive;
    private bool playerWasDead;
    private bool wasLastOneAlive;
    private bool healerLimitBreakThreeApplied;


    public PlayerStatusService()
    {
        CriticalCommonLib.Service.Framework.Update += OnUpdate;
        
        try {
            var addToScreenLogWithScreenLogKindAddress = CriticalCommonLib.Service.Scanner.ScanText("E8 ?? ?? ?? ?? BF ?? ?? ?? ?? EB 39");
            this.addToScreenLogWithScreenLogKindHook = Hook<AddToScreenLogWithScreenLogKindDelegate>.FromAddress(addToScreenLogWithScreenLogKindAddress, this.AddToScreenLogWithScreenLogKindDetour);
            this.addToScreenLogWithScreenLogKindHook.Enable();

        }
        catch (Exception)
        {
            // ignored
        }
    }

    public void Dispose()
    {
        addToScreenLogWithScreenLogKindHook?.Dispose();
        CriticalCommonLib.Service.Framework.Update -= OnUpdate;
    }

    private void AddToScreenLogWithScreenLogKindDetour(Character* target, Character* source, FlyTextKind flyTextKind, byte option, byte actionKind, int actionId, int val1, int val2, byte damageType)
    {
        // A Healer LB3 always applies a heal to the player (even if they were dead) with the LB3 action attached
        healerLimitBreakThreeApplied = HealerLimitBreakThree.Contains((uint)actionId);
        this.addToScreenLogWithScreenLogKindHook!.Original(target, source, flyTextKind, option, actionKind, actionId, val1, val2, damageType);
    }

    private void OnUpdate(Framework framework)
    {
        RevivedCheck();
        LastOneAliveCheck();
    }

    private void RevivedCheck()
    {
        var player = CriticalCommonLib.Service.ClientState.LocalPlayer;
        if (player is null) return;
        if (player.IsDead)
        {
            playerWasDead = true;
            return;
        }

        if (playerWasDead)
        {
            if (player.StatusList.Any(s => s.StatusId is Weakness or BrinkOfDeath))
            {
                //InvokeSafely is from an internal extension in Dalamud :(
                this.OnRevive?.Invoke(this, ReviveType.Normal);
            }

            if (healerLimitBreakThreeApplied)
            {
                this.OnRevive?.Invoke(this, ReviveType.LimitBreak);
            }
        }

        playerWasDead = false;
    }

    private void LastOneAliveCheck()
    {
        // Don't care if you aren't in an <i>actual</i> party
        if (Services.PartyList.Length < 2) return;
        var player = CriticalCommonLib.Service.ClientState.LocalPlayer;
        if (player is null) return;
        if (player.IsDead)
        {
            wasLastOneAlive = false;
            return;
        }
        foreach (var partyMember in Services.PartyList)
        {
            if (partyMember.GameObject is null) continue;
            if (partyMember.GameObject.ObjectId != player.ObjectId)
            {
                if (!partyMember.GameObject.IsDead)
                {
                    wasLastOneAlive = false;
                    return;
                }
            }
        }

        if (!wasLastOneAlive)
        {
            OnLastOneAlive?.Invoke();
        }

        wasLastOneAlive = true;
    }
}
