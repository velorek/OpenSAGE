﻿using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class EjectPilotDie : DieModule
{
    private readonly EjectPilotDieModuleData _moduleData;

    internal EjectPilotDie(GameObject gameObject, GameEngine gameEngine, EjectPilotDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
        _moduleData = moduleData;
    }

    private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
    {
        var veterancy = (VeterancyLevel)GameObject.Rank;

        if (!_moduleData.VeterancyLevels.Get(veterancy))
        {
            return;
        }

        var isOnGround = true; // todo: determine if unit is airborne
        var creationList = isOnGround ? _moduleData.GroundCreationList : _moduleData.AirCreationList;
        foreach (var gameObject in GameEngine.ObjectCreationLists.Create(creationList.Value, context))
        {
            gameObject.Rank = GameObject.Rank;
            GameEngine.AudioSystem.PlayAudioEvent(gameObject, GameObject.Definition.UnitSpecificSounds.VoiceEject?.Value);
        }
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
/// Allows use of SoundEject and VoiceEject within UnitSpecificSounds section of the object.
/// </summary>
public sealed class EjectPilotDieModuleData : DieModuleData
{
    internal static EjectPilotDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<EjectPilotDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
        .Concat(new IniParseTable<EjectPilotDieModuleData>
        {
            { "GroundCreationList", (parser, x) => x.GroundCreationList = parser.ParseObjectCreationListReference() },
            { "AirCreationList", (parser, x) => x.AirCreationList = parser.ParseObjectCreationListReference() },
            { "VeterancyLevels", (parser, x) => x.VeterancyLevels = parser.ParseEnumBitArray<VeterancyLevel>() },
        });

    public LazyAssetReference<ObjectCreationList> GroundCreationList { get; private set; }
    public LazyAssetReference<ObjectCreationList> AirCreationList { get; private set; }
    public BitArray<VeterancyLevel> VeterancyLevels { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new EjectPilotDie(gameObject, gameEngine, this);
    }
}
