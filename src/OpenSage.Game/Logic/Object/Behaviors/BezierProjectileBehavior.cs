﻿using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public class BezierProjectileBehavior : UpdateModule
{
    private readonly BezierProjectileBehaviorData _moduleData;

    private uint _launcherObjectId;
    private uint _targetObjectId;
    private uint _unknown1;
    private readonly float[] _unknownFloats = new float[7];
    private string _weaponThatFiredThis;
    private uint _unknown2;

    internal FXList DetonationFX { get; set; }

    internal BezierProjectileBehavior(GameObject gameObject, GameEngine gameEngine, BezierProjectileBehaviorData moduleData)
        : base(gameObject, gameEngine)
    {
        _moduleData = moduleData;
    }

    internal override void Update(BehaviorUpdateContext context)
    {
        // TODO: Bezier implementation.
        if (!GameObject.IsProjectile)
        {
            return;
        }

        var direction = Vector3.TransformNormal(Vector3.UnitX, context.GameObject.TransformMatrix);
        var velocity = direction * context.GameObject.Speed;

        GameObject.SetTranslation(GameObject.Translation + velocity * (GameEngine.MsPerLogicFrame / 1000.0f));

        CheckForHit(
            context,
            _moduleData.DetonateCallsKill,
            DetonationFX ?? _moduleData.GroundHitFX?.Value);
    }

    internal static void CheckForHit(
        BehaviorUpdateContext context,
        bool detonateCallsKill,
        FXList groundHitFX)
    {
        // Did we hit an object?
        // TODO

        if (DidHitGround(context))
        {
            // TODO: Interpolate to find actual point we hit the ground?
            Detonate(context, new WeaponTarget(context.GameObject.Translation), detonateCallsKill, groundHitFX);
        }
    }

    private static bool DidHitGround(BehaviorUpdateContext context)
    {
        var translation = context.GameObject.Translation;

        var terrainHeight = context.GameEngine.Terrain.HeightMap.GetHeight(
            translation.X,
            translation.Y);

        return translation.Z <= terrainHeight;
    }

    private static void Detonate(
        BehaviorUpdateContext context,
        WeaponTarget target,
        bool detonateCallsKill,
        FXList groundHitFX)
    {
        // TODO: Should this ever be null?
        if (context.GameObject.CurrentWeapon != null)
        {
            context.GameObject.CurrentWeapon.SetTarget(target);
            context.GameObject.CurrentWeapon.Fire();
        }

        if (target.TargetType == WeaponTargetType.Position)
        {
            groundHitFX?.Execute(new FXListExecutionContext(
                context.GameObject.Rotation,
                context.GameObject.Translation,
                context.GameEngine));
        }

        if (detonateCallsKill)
        {
            context.GameObject.Kill(DeathType.Detonated);
        }
        else
        {
            context.GameObject.Die(DeathType.Detonated);
        }
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistObjectID(ref _launcherObjectId);
        reader.PersistObjectID(ref _targetObjectId);
        reader.PersistUInt32(ref _unknown1);

        reader.PersistArray(
            _unknownFloats,
            static (StatePersister persister, ref float item) =>
            {
                persister.PersistSingleValue(ref item);
            });

        reader.PersistAsciiString(ref _weaponThatFiredThis);
        reader.PersistUInt32(ref _unknown2);
    }
}

[AddedIn(SageGame.Bfme)]
public class BezierProjectileBehaviorData : UpdateModuleData
{
    internal static BezierProjectileBehaviorData Parse(IniParser parser) => parser.ParseBlock(BezierProjectileFieldParseTable);

    internal static readonly IniParseTable<BezierProjectileBehaviorData> BezierProjectileFieldParseTable = new IniParseTable<BezierProjectileBehaviorData>
    {
        { "FirstHeight", (parser, x) => x.FirstHeight = parser.ParseInteger() },
        { "SecondHeight", (parser, x) => x.SecondHeight = parser.ParseInteger() },
        { "FirstPercentIndent", (parser, x) => x.FirstPercentIndent = parser.ParsePercentage() },
        { "SecondPercentIndent", (parser, x) => x.SecondPercentIndent = parser.ParsePercentage() },
        { "TumbleRandomly", (parser, x) => x.TumbleRandomly = parser.ParseBoolean() },
        { "CrushStyle", (parser, x) => x.CrushStyle = parser.ParseBoolean() },
        { "DieOnImpact", (parser, x) => x.DieOnImpact = parser.ParseBoolean() },
        { "BounceCount", (parser, x) => x.BounceCount = parser.ParseInteger() },
        { "BounceDistance", (parser, x) => x.BounceDistance = parser.ParseInteger() },
        { "BounceFirstHeight", (parser, x) => x.BounceFirstHeight = parser.ParseInteger() },
        { "BounceSecondHeight", (parser, x) => x.BounceSecondHeight = parser.ParseInteger() },
        { "BounceFirstPercentIndent", (parser, x) => x.BounceFirstPercentIndent = parser.ParsePercentage() },
        { "BounceSecondPercentIndent", (parser, x) => x.BounceSecondPercentIndet = parser.ParsePercentage() },
        { "GroundHitFX", (parser, x) => x.GroundHitFX = parser.ParseFXListReference() },
        { "GroundHitWeapon", (parser, x) => x.GroundHitWeapon = parser.ParseAssetReference() },
        { "GroundBounceFX", (parser, x) => x.GroundBounceFX = parser.ParseAssetReference() },
        { "GroundBounceWeapon", (parser, x) => x.GroundBounceWeapon = parser.ParseAssetReference() },
        { "DetonateCallsKill", (parser, x) => x.DetonateCallsKill = parser.ParseBoolean() },
        { "FlightPathAdjustDistPerSecond", (parser, x) => x.FlightPathAdjustDistPerSecond = parser.ParseInteger() },
        { "CurveFlattenMinDist", (parser, x) => x.CurveFlattenMinDist = parser.ParseFloat() },
        { "InvisibleFrames", (parser, x) => x.InvisibleFrames = parser.ParseInteger() },
        { "PreLandingStateTime", (parser, x) => x.PreLandingStateTime = parser.ParseInteger() },
        { "PreLandingEmotion", (parser, x) => x.PreLandingEmotion = parser.ParseEnum<EmotionType>() },
        { "PreLandingEmotionRadius", (parser, x) => x.PreLandingEmotionRadius = parser.ParseFloat() },
        { "FadeInTime", (parser, x) => x.FadeInTime = parser.ParseInteger() },
        { "IgnoreTerrainHeight", (parser, x) => x.IgnoreTerrainHeight = parser.ParseBoolean() },
        { "FirstPercentHeight", (parser, x) => x.FirstPercentHeight = parser.ParsePercentage() },
        { "SecondPercentHeight", (parser, x) => x.SecondPercentHeight = parser.ParsePercentage() },
        { "FinalStuckTime", (parser, x) => x.FinalStuckTime = parser.ParseInteger() },
        { "OrientToFlightPath", (parser, x) => x.OrientToFlightPath = parser.ParseBoolean() },
        { "GarrisonHitKillRequiredKindOf", (parser, x) => x.GarrisonHitKillRequiredKindOf = parser.ParseEnum<ObjectKinds>() },
        { "GarrisonHitKillForbiddenKindOf", (parser, x) => x.GarrisonHitKillForbiddenKindOf = parser.ParseEnum<ObjectKinds>() },
        { "GarrisonHitKillCount", (parser, x) => x.GarrisonHitKillCount = parser.ParseInteger() },
        { "GarrisonHitKillFX", (parser, x) => x.GarrisonHitKillFX = parser.ParseAssetReference() },
        { "PreLandingEmotionAffectsAllies", (parser, x) => x.PreLandingEmotionAffectsAllies = parser.ParseBoolean() },
    };

    public int FirstHeight { get; private set; }
    public int SecondHeight { get; private set; }
    public Percentage FirstPercentIndent { get; private set; }
    public Percentage SecondPercentIndent { get; private set; }
    public bool TumbleRandomly { get; private set; }

    public bool CrushStyle { get; private set; }
    public bool DieOnImpact { get; private set; }
    public int BounceCount { get; private set; }
    public int BounceDistance { get; private set; }
    public int BounceFirstHeight { get; private set; }
    public int BounceSecondHeight { get; private set; }
    public Percentage BounceFirstPercentIndent { get; private set; }
    public Percentage BounceSecondPercentIndet { get; private set; }
    public LazyAssetReference<FXList> GroundHitFX { get; private set; }
    public string GroundHitWeapon { get; private set; }
    public string GroundBounceFX { get; private set; }
    public string GroundBounceWeapon { get; private set; }
    public bool DetonateCallsKill { get; private set; }
    public int FlightPathAdjustDistPerSecond { get; private set; }
    public float CurveFlattenMinDist { get; private set; }
    public int InvisibleFrames { get; private set; }
    public int PreLandingStateTime { get; private set; }
    public EmotionType PreLandingEmotion { get; private set; }
    public float PreLandingEmotionRadius { get; private set; }
    public int FadeInTime { get; private set; }
    public bool IgnoreTerrainHeight { get; private set; }
    public Percentage FirstPercentHeight { get; private set; }
    public Percentage SecondPercentHeight { get; private set; }
    public int FinalStuckTime { get; private set; }
    public bool OrientToFlightPath { get; private set; }
    public ObjectKinds GarrisonHitKillRequiredKindOf { get; private set; }
    public ObjectKinds GarrisonHitKillForbiddenKindOf { get; private set; }
    public int GarrisonHitKillCount { get; private set; }
    public string GarrisonHitKillFX { get; private set; }

    [AddedIn(SageGame.Bfme2)]
    public bool PreLandingEmotionAffectsAllies { get; private set; }

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new BezierProjectileBehavior(gameObject, gameEngine, this);
    }
}
