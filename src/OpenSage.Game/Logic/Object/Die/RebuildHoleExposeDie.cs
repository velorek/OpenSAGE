﻿using FixedMath.NET;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class RebuildHoleExposeDie : DieModule
{
    private readonly RebuildHoleExposeDieModuleData _moduleData;

    internal RebuildHoleExposeDie(GameObject gameObject, GameEngine gameEngine, RebuildHoleExposeDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
    {
        var hole = GameEngine.GameLogic.CreateObject(_moduleData.HoleDefinition.Value, GameObject.Owner);
        hole.SetTransformMatrix(GameObject.TransformMatrix);
        var holeHealth = (Fix64)_moduleData.HoleMaxHealth;
        hole.MaxHealth = holeHealth;
        hole.Health = holeHealth;
        hole.FindBehavior<RebuildHoleUpdate>().SetOriginalStructure(GameObject);

        base.Die(context, deathType);
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

/// <summary>
/// Requires the object specified in <see cref="HoleDefinition"/> to have the REBUILD_HOLE KindOf and
/// <see cref="RebuildHoleUpdateModuleData"/> module in order to work.
/// </summary>
public sealed class RebuildHoleExposeDieModuleData : DieModuleData
{
    internal static RebuildHoleExposeDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<RebuildHoleExposeDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<RebuildHoleExposeDieModuleData>
        {
            { "HoleName", (parser, x) => x.HoleDefinition = parser.ParseObjectReference() },
            { "HoleMaxHealth", (parser, x) => x.HoleMaxHealth = parser.ParseFloat() },
            { "FadeInTimeSeconds", (parser, x) => x.FadeInTimeSeconds = parser.ParseFloat() },
            { "TransferAttackers", (parser, x) => x.TransferAttackers = parser.ParseBoolean() }
        });

    public LazyAssetReference<ObjectDefinition> HoleDefinition { get; private set; }
    public float HoleMaxHealth { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public float FadeInTimeSeconds { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public bool TransferAttackers { get; private set; }

    public RebuildHoleExposeDieModuleData()
    {
        DieData.ExemptStatus = ObjectStatus.UnderConstruction;
    }

    internal override RebuildHoleExposeDie CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new RebuildHoleExposeDie(gameObject, gameEngine, this);
    }
}
