﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class TransportAIUpdate : AIUpdate
{
    internal override TransportAIUpdateModuleData ModuleData { get; }

    internal TransportAIUpdate(GameObject gameObject, GameEngine gameEngine, TransportAIUpdateModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
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
/// Used on TRANSPORT KindOfs that contain other objects.
/// </summary>
public sealed class TransportAIUpdateModuleData : AIUpdateModuleData
{
    internal new static TransportAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private new static readonly IniParseTable<TransportAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<TransportAIUpdateModuleData>());

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new TransportAIUpdate(gameObject, gameEngine, this);
    }
}
