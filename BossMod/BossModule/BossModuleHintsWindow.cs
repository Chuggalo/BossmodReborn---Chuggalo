﻿using ImGuiNET;

namespace BossMod;

public sealed class BossModuleHintsWindow : UIWindow
{
    private readonly BossModuleManager _mgr;
    private readonly ZoneModuleManager _zmm;

    public BossModuleHintsWindow(BossModuleManager mgr, ZoneModuleManager zmm) : base("Boss module hints", false, new(400, 100))
    {
        _mgr = mgr;
        _zmm = zmm;
        RespectCloseHotkey = false;
    }

    public override void PreOpenCheck()
    {
        IsOpen = BossModuleManager.Config.HintsInSeparateWindow && (_mgr.ActiveModule != null || ShowZoneModule());
        Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
        if (BossModuleManager.Config.Lock)
            Flags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs;
        if (BossModuleManager.Config.HintsInSeparateWindowTransparent)
            Flags |= ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground;
        ForceMainWindow = BossModuleManager.Config.HintsInSeparateWindowTransparent; // NoBackground flag without ForceMainWindow works incorrectly for whatever reason
    }

    public override void Draw()
    {
        if (ShowZoneModule())
        {
            _zmm.ActiveModule?.DrawGlobalHints();
        }
        else
        {
            try
            {
                _mgr.ActiveModule?.Draw(default, PartyState.PlayerSlot, true, false);
            }
            catch (Exception ex)
            {
                Service.Log($"Boss module draw-hints crashed: {ex}");
                _mgr.ActiveModule = null;
            }
        }
    }

    private bool ShowZoneModule() => _mgr.ActiveModule?.StateMachine.ActivePhase == null && (_zmm.ActiveModule?.WantDrawHints() ?? false);
}
