﻿namespace BossMod.Shadowbringers.Ultimate.TEA;

class P3Inception1(BossModule module) : Components.CastCounter(module, (uint)AID.JudgmentCrystalAOE)
{
    private readonly List<Actor> _plasmaspheres = [];
    private readonly Actor?[] _tetherSources = new Actor?[PartyState.MaxPartySize];
    private readonly WPos[] _assignedPositions = new WPos[PartyState.MaxPartySize];

    public bool AllSpheresSpawned => _plasmaspheres.Count == 4;
    public bool CrystalsDone => NumCasts > 0;

    private const float _crystalRadius = 5;
    private const float _sphereRadius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!AllSpheresSpawned)
            return;

        var sphere = _tetherSources[slot];
        if (sphere != null)
        {
            if (!sphere.IsDead && Raid.WithSlot(true, true, true).WhereSlot(s => _tetherSources[s] != null).InRadiusExcluding(actor, _sphereRadius * 2).Any())
                hints.Add("GTFO from other tethers!");
        }
        else if (!CrystalsDone)
        {
            if (Raid.WithSlot(true, true, true).WhereSlot(s => _tetherSources[s] == null).InRadiusExcluding(actor, _crystalRadius * 2).Any())
                hints.Add("GTFO from other crystals!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!AllSpheresSpawned)
            return;

        foreach (var (slot, player) in Raid.WithSlot(true, true, true))
        {
            var sphere = _tetherSources[slot];
            if (sphere != null)
            {
                if (!sphere.IsDead)
                {
                    Arena.Actor(sphere, Colors.Object, true);
                    Arena.AddLine(sphere.Position, player.Position, slot == pcSlot ? Colors.Safe : Colors.Danger);
                    Arena.AddCircle(player.Position, _sphereRadius, Colors.Danger);
                }
            }
            else if (!CrystalsDone)
            {
                Arena.AddCircle(player.Position, _crystalRadius, Colors.Danger);
            }
        }

        var pcSphere = _tetherSources[pcSlot];
        if (pcSphere != null)
        {
            if (!pcSphere.IsDead)
            {
                Arena.AddCircle(_assignedPositions[pcSlot], 1, Colors.Safe);
            }
        }
        else if (!CrystalsDone)
        {
            Arena.AddCircle(_assignedPositions[pcSlot] + new WDir(-5, -5), 1, Colors.Safe);
            Arena.AddCircle(_assignedPositions[pcSlot] + new WDir(-5, +5), 1, Colors.Safe);
            Arena.AddCircle(_assignedPositions[pcSlot] + new WDir(+5, -5), 1, Colors.Safe);
            Arena.AddCircle(_assignedPositions[pcSlot] + new WDir(+5, +5), 1, Colors.Safe);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Plasmasphere && (OID)source.OID == OID.Plasmasphere)
        {
            _plasmaspheres.Add(source);
            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
                _tetherSources[slot] = source;

            if (AllSpheresSpawned)
                InitAssignments();
        }
    }

    private void InitAssignments()
    {
        // alex is either at N or S cardinal; 2 spheres are E and 2 spheres are W
        // for tethered player, assign 45-degree spot on alex's side, as far away from source as possible
        var alexNorth = ((TEA)Module).AlexPrime()?.Position.Z < Arena.Center.Z;
        var boxPos = Arena.Center + new WDir(0, alexNorth ? 13 : -13);
        for (var slot = 0; slot < _tetherSources.Length; ++slot)
        {
            var sphere = _tetherSources[slot];
            if (sphere != null)
            {
                var sphereWest = sphere.Position.X < Arena.Center.X;
                var sameSideSphere = _plasmaspheres.Find(o => o != sphere && (o.Position.X < Arena.Center.X) == sphereWest);
                var sphereNorth = sphere.Position.Z < sameSideSphere?.Position.Z;

                var spotDir = alexNorth ? (sphereNorth ? 90.Degrees() : 135.Degrees()) : (sphereNorth ? 45.Degrees() : 90.Degrees());
                if (!sphereWest)
                    spotDir = -spotDir;
                _assignedPositions[slot] = Arena.Center + 18 * spotDir.ToDirection();
            }
            else
            {
                // TODO: consider assigning concrete spots...
                _assignedPositions[slot] = boxPos;
            }
        }
    }
}
