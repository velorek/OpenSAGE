﻿using System.Collections.Generic;
using System.IO;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.CommandListOverlay;
using OpenSage.Gui.ControlBar;
using OpenSage.Mods.Bfme.Gui;

namespace OpenSage.Mods.Bfme;

public class BfmeDefinition : IGameDefinition
{
    public SageGame Game => SageGame.Bfme;
    public string DisplayName => "The Lord of the Rings (tm): The Battle for Middle-earth (tm)";
    public IGameDefinition BaseGame => null;

    public bool LauncherImagePrefixLang => true;
    public string LauncherImagePath => "Splash.jpg";

    public IEnumerable<RegistryKeyPath> RegistryKeys { get; } = new[]
    {
        new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "InstallPath")
    };

    public IEnumerable<RegistryKeyPath> LanguageRegistryKeys { get; } = new[]
{
        new RegistryKeyPath(@"SOFTWARE\Electronic Arts\EA Games\The Battle for Middle-earth", "Language")
    };

    public SteamInstallationDefinition Steam { get; } = null;

    public string Identifier { get; } = "bfme";

    public IMainMenuSource MainMenu { get; } = new AptMainMenuSource("MainMenu.apt");
    public IControlBarSource ControlBar { get; } = new AptControlBarSource();
    public ICommandListOverlaySource CommandListOverlay { get; } = new RadialUnitOverlaySource();

    public uint ScriptingTicksPerSecond => 5;

    public string GetLocalizedStringsPath(string language) => Path.Combine("lang", language, "lotr");

    public OnDemandAssetLoadStrategy CreateAssetLoadStrategy()
    {
        return new OnDemandAssetLoadStrategy(PathResolvers.W3d, PathResolvers.BfmeTexture);
    }

    public Scene25D CreateScene25D(Scene3D scene3D, AssetStore assetStore) => new(scene3D, assetStore);

    public static BfmeDefinition Instance { get; } = new BfmeDefinition();

    public string LauncherExecutable => "lotrbfme.exe";
}
