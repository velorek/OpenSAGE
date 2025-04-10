﻿#nullable enable

using System.Numerics;
using OpenSage.Audio;
using OpenSage.Data.Ini;
using OpenSage.Logic.AI;
using OpenSage.Logic.AI.AIStates;

namespace OpenSage.Logic.Object;

public class HackInternetAIUpdate : AIUpdate
{
    internal override HackInternetAIUpdateModuleData ModuleData { get; }

    private UnknownStateData? _packingUpData;

    internal HackInternetAIUpdate(GameObject gameObject, GameEngine gameEngine, HackInternetAIUpdateModuleData moduleData) : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
    }

    private protected override HackInternetAIUpdateStateMachine CreateStateMachine() => new(GameObject, GameEngine, this);

    private protected override void RunUpdate(BehaviorUpdateContext context)
    {
        if (StateMachine.CurrentState is IdleState)
        {
            if (_packingUpData != null && _packingUpData.TargetPosition != default)
            {
                SetTargetPoint(_packingUpData.TargetPosition);
            }
            _packingUpData = null;
        }
    }

    public void StartHackingInternet()
    {
        Stop();

        StateMachine.SetState(StartHackingInternetState.StateId);
    }

    internal override void SetTargetPoint(Vector3 targetPoint)
    {
        Stop();

        if (StateMachine.CurrentState is StopHackingInternetState)
        {
            // we can't move just yet
            _packingUpData = new UnknownStateData { TargetPosition = targetPoint };
        }
        else
        {
            base.SetTargetPoint(targetPoint);
        }
    }

    internal override void Stop()
    {
        switch (StateMachine.CurrentState)
        {
            case StartHackingInternetState:
                // this takes effect immediately
                StateMachine.SetState(IdleState.StateId);
                break;
            case HackInternetState:
                StateMachine.SetState(StopHackingInternetState.StateId);
                break;
                // If we're in StopHackingInternetState, we need to see that through
        }

        base.Stop();
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        var hasPackingUpData = _packingUpData != null;
        reader.PersistBoolean(ref hasPackingUpData);
        if (hasPackingUpData)
        {
            _packingUpData ??= new UnknownStateData();
            reader.PersistObject(_packingUpData);
        }
    }
}

internal sealed class HackInternetAIUpdateStateMachine : AIUpdateStateMachine
{
    public override HackInternetAIUpdate AIUpdate { get; }

    public HackInternetAIUpdateStateMachine(GameObject gameObject, GameEngine gameEngine, HackInternetAIUpdate aiUpdate)
        : base(gameObject, gameEngine, aiUpdate)
    {
        AIUpdate = aiUpdate;

        AddState(StartHackingInternetState.StateId, new StartHackingInternetState(this));
        AddState(HackInternetState.StateId, new HackInternetState(this));
        AddState(StopHackingInternetState.StateId, new StopHackingInternetState(this));
    }

    internal LogicFrameSpan GetVariableFrames(LogicFrameSpan time, GameEngine gameEngine)
    {
        // take a random float, *2 for 0 - 2, -1 for -1 - 1, *variance for our actual variance factor
        return new LogicFrameSpan((uint)(time.Value + time.Value * ((gameEngine.Random.NextSingle() * 2 - 1) * AIUpdate.ModuleData.PackUnpackVariationFactor)));
    }
}

/// <summary>
/// Allows use of UnitPack, UnitUnpack, and UnitCashPing within the UnitSpecificSounds section
/// of the object.
/// Also allows use of PACKING and UNPACKING condition states.
/// </summary>
public sealed class HackInternetAIUpdateModuleData : AIUpdateModuleData
{
    internal new static HackInternetAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private new static readonly IniParseTable<HackInternetAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<HackInternetAIUpdateModuleData>
        {
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "CashUpdateDelay", (parser, x) => x.CashUpdateDelay = parser.ParseTimeMillisecondsToLogicFrames() },
            { "CashUpdateDelayFast", (parser, x) => x.CashUpdateDelayFast = parser.ParseTimeMillisecondsToLogicFrames() },
            { "RegularCashAmount", (parser, x) => x.RegularCashAmount = parser.ParseInteger() },
            { "VeteranCashAmount", (parser, x) => x.VeteranCashAmount = parser.ParseInteger() },
            { "EliteCashAmount", (parser, x) => x.EliteCashAmount = parser.ParseInteger() },
            { "HeroicCashAmount", (parser, x) => x.HeroicCashAmount = parser.ParseInteger() },
            { "XpPerCashUpdate", (parser, x) => x.XpPerCashUpdate = parser.ParseInteger() },
            { "PackUnpackVariationFactor", (parser, x) => x.PackUnpackVariationFactor = parser.ParseFloat() },
        });

    public LogicFrameSpan UnpackTime { get; private set; }
    public LogicFrameSpan PackTime { get; private set; }
    public LogicFrameSpan CashUpdateDelay { get; private set; }

    /// <summary>
    /// Hack speed when in a container (presumably with <see cref="InternetHackContainModuleData"/>).
    /// </summary>
    /// <remarks>
    /// The ini comments say "Fast speed used inside a container (can only hack inside an Internet Center)", however
    /// other mods will use this inside of e.g. listening outposts ("hacker vans"), so this can definitely be used
    /// in <i>any</i> container, not just internet centers.
    /// </remarks>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public LogicFrameSpan CashUpdateDelayFast { get; private set; }

    public int RegularCashAmount { get; private set; }
    public int VeteranCashAmount { get; private set; }
    public int EliteCashAmount { get; private set; }
    public int HeroicCashAmount { get; private set; }
    public int XpPerCashUpdate { get; private set; }

    /// <summary>
    /// Adds +/- the factor to the pack and unpack time, randomly.
    /// </summary>
    /// <example>
    /// If this is 0.5 and the unpack time is 1000ms, the actual unpack time may be anywhere between 500 and 1500ms.
    /// </example>
    public float PackUnpackVariationFactor { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new HackInternetAIUpdate(gameObject, gameEngine, this);
    }
}
