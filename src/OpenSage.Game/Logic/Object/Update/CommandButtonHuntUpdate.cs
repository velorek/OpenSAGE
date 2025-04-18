﻿using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class CommandButtonHuntUpdate : UpdateModule
{
    private string _commandButtonName;

    public CommandButtonHuntUpdate(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistAsciiString(ref _commandButtonName);
    }
}

/// <summary>
/// Allows this object to hunt using a particular ability or command via scripts. AI only.
/// </summary>
public sealed class CommandButtonHuntUpdateModuleData : UpdateModuleData
{
    internal static CommandButtonHuntUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<CommandButtonHuntUpdateModuleData> FieldParseTable = new IniParseTable<CommandButtonHuntUpdateModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new CommandButtonHuntUpdate(gameObject, gameEngine);
    }
}
