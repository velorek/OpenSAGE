﻿using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Settings;
using Veldrid;

namespace OpenSage.Diagnostics;

internal sealed class RenderedView : DisposableBase
{
    private readonly DiagnosticViewContext _context;
    private readonly InputMessageBuffer _inputMessageBuffer;
    private readonly Scene3D _scene3D;
    private readonly RenderTarget _renderTarget;

    private Vector2 _cachedSize;

    public RenderPipeline RenderPipeline { get; }

    public Scene3D Scene => _scene3D;

    public RenderedView(
        DiagnosticViewContext context,
        in Vector3 cameraTarget = default,
        float cameraDistance = 200,
        Action<IGameObjectCollection> createGameObjects = null)
    {
        _context = context;

        _inputMessageBuffer = new InputMessageBuffer();

        _scene3D = AddDisposable(new Scene3D(
            context.Game,
            _inputMessageBuffer,
            () => new Viewport(0, 0, ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y, 0, 1),
            new ArcballCameraController(cameraTarget, cameraDistance),
            WorldLighting.CreateDefault(),
            Environment.TickCount,
            isDiagnosticScene: true));

        createGameObjects?.Invoke(_scene3D.GameObjects);

        RenderPipeline = AddDisposable(new RenderPipeline(context.Game));

        _renderTarget = AddDisposable(new RenderTarget(context.Game.GraphicsDevice));
    }

    public void Draw()
    {
        ImGui.BeginChild("RenderedViewContent", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.NoMove);

        var currentSize = ImGui.GetContentRegionAvail();
        if (_cachedSize != currentSize)
        {
            var newSize = new Size((int)currentSize.X, (int)currentSize.Y);
            _renderTarget.EnsureSize(newSize);
            _cachedSize = currentSize;

            _scene3D.Camera.OnViewportSizeChanged();
        }

        var cursorScreenPos = ImGui.GetCursorScreenPos();
        var isMouseInRenderedView = ImGui.IsWindowFocused() && ImGui.IsMouseHoveringRect(cursorScreenPos, cursorScreenPos + currentSize);

        var inputMessages = isMouseInRenderedView
            ? ImGuiUtility.TranslateInputMessages(
                new Mathematics.Rectangle((int)cursorScreenPos.X, (int)cursorScreenPos.Y, (int)currentSize.X, (int)currentSize.Y),
                _context.Window.MessageQueue)
            : Array.Empty<InputMessage>();

        _inputMessageBuffer.PumpEvents(inputMessages, _context.Game.RenderTime);

        _scene3D.LocalLogicTick(_context.Game.MapTime, 1.0f);

        RenderPipeline.Execute(new RenderContext
        {
            ContentManager = _context.Game.ContentManager,
            GameTime = _context.Game.RenderTime,
            GraphicsDevice = _context.Game.GraphicsDevice,
            RenderTarget = _renderTarget.Framebuffer,
            Scene2D = null,
            Scene3D = _scene3D
        });

        var imagePointer = _context.ImGuiRenderer.GetOrCreateImGuiBinding(
            _context.Game.GraphicsDevice.ResourceFactory,
            _renderTarget.ColorTarget);

        ImGui.Image(
            imagePointer,
            currentSize,
            _context.Game.GetTopLeftUV(),
            _context.Game.GetBottomRightUV());

        ImGui.EndChild();
    }
}
