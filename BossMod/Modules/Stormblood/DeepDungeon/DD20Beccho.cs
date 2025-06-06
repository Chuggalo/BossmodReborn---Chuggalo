﻿namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD20Beccho;

public enum OID : uint
{
    Boss = 0x23E7, // R3.0
    ChokeshinAdds = 0x23E8 // R1.0
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Fragility = 11901, // ChokeshinAdds->self, 3.0s cast, range 8 circle
    NeuroSquama = 11900, // Boss->self, 3.0s cast, range 50 circle, gaze
    Proboscis = 11898, // Boss->player, no cast, single-target
    PsychoSquama = 11899 // Boss->self, 3.0s cast, range 50+R 90-degree cone
}

class PsychoSquamaAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PsychoSquama, new AOEShapeCone(53f, 45f.Degrees()));
class NeuroSquamaLookAway(BossModule module) : Components.CastGaze(module, (uint)AID.NeuroSquama);
class FragilityAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fragility, 8f);

class DD20BecchoStates : StateMachineBuilder
{
    public DD20BecchoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PsychoSquamaAOE>()
            .ActivateOnEnter<NeuroSquamaLookAway>()
            .ActivateOnEnter<FragilityAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 541, NameID = 7481)]
public class DD20Beccho(WorldState ws, Actor primary) : HoHArena1(ws, primary);
