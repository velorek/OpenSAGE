﻿#nullable enable

namespace OpenSage.Logic.AI.AIStates;

internal sealed class HuntState : State
{
    private readonly AttackAreaStateMachine _stateMachine;

    private uint _unknownInt;

    public HuntState(AIUpdateStateMachine stateMachine) : base(stateMachine)
    {
        _stateMachine = new AttackAreaStateMachine(stateMachine);
    }

    public override void Persist(StatePersister reader)
    {
        reader.PersistVersion(1);

        var unknownBool = true;
        reader.PersistBoolean(ref unknownBool);
        if (!unknownBool)
        {
            throw new InvalidStateException();
        }

        reader.PersistObject(_stateMachine);
        reader.PersistUInt32(ref _unknownInt);
    }
}
