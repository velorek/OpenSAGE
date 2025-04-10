﻿using System;
using System.Linq;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.OrderGenerators;

// TODO: Cancel this when:
// 1. Builder dies
// 2. We lose access to the building
// 3. Player right-clicks
public sealed class ConstructBuildingOrderGenerator : OrderGenerator, IDisposable
{
    private readonly ObjectDefinition _buildingDefinition;
    private readonly int _definitionIndex;
    private readonly GameData _config;
    private readonly Scene3D _scene;

    private readonly GameObject _previewObject;

    private float _angle;

    public override bool CanDrag => true;

    internal ConstructBuildingOrderGenerator(
        ObjectDefinition buildingDefinition,
        int definitionIndex,
        GameData config,
        Player player,
        GameEngine gameContext,
        Scene3D scene) : base(gameContext.Game)
    {
        _buildingDefinition = buildingDefinition;
        _definitionIndex = definitionIndex;
        _config = config;
        _scene = scene;

        // TODO: Should this be relative to the current camera angle?
        _angle = MathUtility.ToRadians(_buildingDefinition.PlacementViewAngle);

        _previewObject = new GameObject(
            buildingDefinition,
            gameContext,
            player)
        {
            IsPlacementPreview = true
        };

        _scene.BuildPreviewObject = _previewObject;

        UpdatePreviewObjectPosition();
        UpdatePreviewObjectAngle();
        UpdateValidity();
    }

    public override void BuildRenderList(RenderList renderList, Camera camera, in TimeInterval gameTime)
    {
        // TODO: Draw arrow (locater02.w3d) to visualise rotation angle.

        _previewObject.LocalLogicTick(gameTime, 0, null);
        _previewObject.BuildRenderList(renderList, camera, gameTime);
    }

    public override OrderGeneratorResult TryActivate(Scene3D scene, KeyModifiers keyModifiers)
    {
        if (scene.Game.SageGame == SageGame.Bfme)
        {
            return OrderGeneratorResult.Inapplicable();
        }

        // TODO: Probably not right way to get dozer object.
        var dozer = scene.LocalPlayer.SelectedUnits.First();

        if (!IsValidPosition())
        {
            scene.Audio.PlayAudioEvent(dozer, dozer.Definition.UnitSpecificSounds?.VoiceNoBuild?.Value);

            // TODO: Display correct message:
            // - GUI:CantBuildRestrictedTerrain
            // - GUI:CantBuildNotFlatEnough
            // - GUI:CantBuildObjectsInTheWay
            // - GUI:CantBuildNoClearPath
            // - GUI:CantBuildShroud
            // - GUI:CantBuildThere

            return OrderGeneratorResult.Failure("Invalid position.");
        }

        var player = scene.LocalPlayer;
        if (player.BankAccount.Money < _buildingDefinition.BuildCost)
        {
            return OrderGeneratorResult.Failure("Not enough cash for construction");
        }

        var playerIdx = scene.GetPlayerIndex(player);
        var buildOrder = Order.CreateBuildObject(playerIdx, _definitionIndex, WorldPosition, _angle);

        return OrderGeneratorResult.SuccessAndExit(new[] { buildOrder });
    }

    private bool IsValidPosition()
    {
        // TODO: Check that the target area has been explored
        // TODO: Check that the builder can reach target position
        // TODO: Check that the terrain is even enough at the target position

        var existingObjects = _scene.Game.PartitionCellManager.QueryObjects(
            _previewObject,
            _previewObject.Translation,
            _previewObject.Geometry.BoundingSphereRadius,
            new PartitionQueries.CollidesWithObjectQuery(_previewObject));

        // as long as the items in our way are not structures and not owned by our enemy, we can build here
        return !_scene.Quadtree.FindIntersecting(_previewObject).Any(u =>
            u.Definition.KindOf.Get(ObjectKinds.Structure) ||
            _scene.LocalPlayer.Enemies.Contains(u.Owner));
    }

    public override void UpdatePosition(Vector2 mousePosition, Vector3 worldPosition)
    {
        base.UpdatePosition(mousePosition, worldPosition);

        UpdatePreviewObjectPosition();
        UpdateValidity();
    }

    private void UpdatePreviewObjectPosition()
    {
        _previewObject.SetTranslation(WorldPosition);
        _previewObject.UpdateColliders();
        _previewObject.BuildProgress = 1.0f;
    }

    public override void UpdateDrag(Vector3 position)
    {
        // Calculate angle from building position to current unprojected mouse position.
        var direction = position.Vector2XY() - WorldPosition.Vector2XY();
        _angle = MathUtility.GetYawFromDirection(direction);

        UpdatePreviewObjectAngle();
        UpdateValidity();
    }

    private void UpdatePreviewObjectAngle()
    {
        _previewObject.SetRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, _angle));
    }

    private void UpdateValidity()
    {
        _previewObject.IsPlacementInvalid = !IsValidPosition();
    }

    // Use radial cursor.
    public override string GetCursor(KeyModifiers keyModifiers) => null;

    public void Dispose()
    {
        _scene.Quadtree.Remove(_previewObject);
        _previewObject.Dispose();
        _scene.BuildPreviewObject = null;
    }
}
