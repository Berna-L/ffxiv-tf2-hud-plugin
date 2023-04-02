using System.Threading.Tasks;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Ui;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.DutyState;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using CriticalCommonLib_Service = CriticalCommonLib.Service;

namespace Tf2Hud;

public class Service
{
    [PluginService]
    public static DutyState DutyState { get; private set; } = null!;
    [PluginService]
    public static PartyList PartyList { get; private set; } = null!;


    public static ContentDirector? ContentDirector
    {
        get
        {
            unsafe
            {
                var d = EventFramework.Instance()->GetInstanceContentDirector();
                if (d is null) return null;
                return d->ContentDirector;
            }
        }
    }

    public static class CriticalCommonLib
    {
        public static CharacterMonitor CharacterMonitor { get; private set; } = null!;
        public static GameUiManager GameUiManager { get; private set; } = null!;
        public static CraftMonitor CraftMonitor { get; private set; } = null!;
        public static GameInterface GameInterface { get; private set; } = null!;
        public static OdrScanner OdrScanner { get; private set; } = null!;
        public static InventoryScanner InventoryScanner { get; private set; } = null!;
        public static FrameworkService FrameworkService { get; private set; } = null!;

        public static InventoryMonitor InventoryMonitor { get; private set; } = null!;

        public static void Initialize()
        {
            CriticalCommonLib_Service.ExcelCache = new ExcelCache(CriticalCommonLib_Service.Data);
            CharacterMonitor = new CharacterMonitor();
            GameUiManager = new GameUiManager();
            CraftMonitor = new CraftMonitor(GameUiManager);
            GameInterface = new GameInterface();
            OdrScanner = new OdrScanner(CharacterMonitor);
            InventoryScanner = new InventoryScanner(CharacterMonitor, GameUiManager, GameInterface, OdrScanner);
            FrameworkService = new FrameworkService(CriticalCommonLib_Service.Framework);
            InventoryMonitor =
                new InventoryMonitor(CharacterMonitor, CraftMonitor, InventoryScanner, FrameworkService);
            InventoryScanner.Enable();
        }

        public static void Dispose()
        {
            InventoryMonitor.Dispose();
            FrameworkService.Dispose();
            InventoryScanner.Dispose();
            OdrScanner.Dispose();
            GameInterface.Dispose();
            CraftMonitor.Dispose();
            GameUiManager.Dispose();
            CharacterMonitor.Dispose();
        }
    }
}
