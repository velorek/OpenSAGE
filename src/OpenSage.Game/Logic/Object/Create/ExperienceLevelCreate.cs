﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
internal sealed class ExperienceLevelCreateBehavior : CreateModule
{
    ExperienceLevelCreateModuleData _moduleData;

    internal ExperienceLevelCreateBehavior(GameObject gameObject, GameEngine gameEngine, ExperienceLevelCreateModuleData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    public override void OnCreate()
    {
        GameObject.Rank = _moduleData.LevelToGrant;
    }
}

public sealed class ExperienceLevelCreateModuleData : CreateModuleData
{
    internal static ExperienceLevelCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<ExperienceLevelCreateModuleData> FieldParseTable = new IniParseTable<ExperienceLevelCreateModuleData>
    {
        { "LevelToGrant", (parser, x) => x.LevelToGrant = parser.ParseInteger() },
        { "MPOnly", (parser, x) => x.MPOnly = parser.ParseBoolean() }
    };

    public int LevelToGrant { get; private set; }
    public bool MPOnly { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new ExperienceLevelCreateBehavior(gameObject, gameEngine, this);
    }
}
