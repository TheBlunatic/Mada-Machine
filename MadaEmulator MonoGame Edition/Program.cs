using Blunatic.Core;
using Blunatic.Scenes;
using MadaEmulator_MonoGame_Edition;

//using var game = new MonoGameInstance((mgi) => new PopupScene(mgi, PopupScene.Type.Info, "No scene currently loaded."), new Vec(1920, 1080) * 0.8, false);
using var game = new MonoGameInstance((mgi) => new EmulatorScene(mgi), new Vec(1920, 1080) * 0.8, false);
game.Run();