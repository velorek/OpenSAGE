﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class KeepObjectDie : DieModule
{
    public KeepObjectDie(GameObject gameObject, GameEngine gameEngine, KeepObjectDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

public sealed class KeepObjectDieModuleData : DieModuleData
{
    internal static KeepObjectDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<KeepObjectDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<KeepObjectDieModuleData>
        {
            { "CollapsingTime", (parser, x) => x.CollapsingTime = parser.ParseInteger() },
            { "StayOnRadar", (parser, x) => x.StayOnRadar = parser.ParseBoolean() }
        });

    [AddedIn(SageGame.Bfme2)]
    public int CollapsingTime { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public bool StayOnRadar { get; private set; }

    internal override KeepObjectDie CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new KeepObjectDie(gameObject, gameEngine, this);
    }
}
