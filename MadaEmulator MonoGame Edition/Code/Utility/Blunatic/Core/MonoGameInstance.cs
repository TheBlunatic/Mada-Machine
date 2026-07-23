using Blunatic.Mgc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blunatic.Core
{
    public interface IScene
    {
        public bool UpdatePreviousScene { get; }
        public bool DrawPreviousScene { get; }
        public void Update(MonoGameInstance mgi);
        public void Draw(MonoGameInstance mgi);
    }
    public class MonoGameInstance : Game
    {
        // Constants
        private static readonly string NAMESPACE = $"Blunatic";

        private static readonly string CONTENT_PATH = $"Content\\";
        private static readonly string INTERNAL_CONTENT_PATH = $"{CONTENT_PATH}{NAMESPACE}\\";
        private static readonly string TEXTURES_PATH = $"{INTERNAL_CONTENT_PATH}{TEXTURES_PATH_FOR_LOAD}";
        private static readonly string TEXTURES_PATH_FOR_LOAD = $"{NAMESPACE}\\Textures\\";
        private static readonly string SOUNDS_PATH = $"{INTERNAL_CONTENT_PATH}Sounds\\";

        private static readonly string RESOURCES_PATH = $"Resources\\";
        private static readonly string INTERNAL_RESOURCES_PATH = $"{RESOURCES_PATH}{NAMESPACE}\\";

        private static readonly string CONTROLS_FILE = $"{INTERNAL_RESOURCES_PATH}controls.html";

        // Structs
        public struct TexDrawInfo
        {
            public Texture2D Texture2D { get; private set; }
            public Rectangle SourceRectangle { get; private set; }

            public TexDrawInfo(Texture2D texture2D, Rectangle rectangle)
            {
                Texture2D = texture2D;
                SourceRectangle = rectangle;
            }
        }

        // Properties
        public GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public Vec ScreenDimensions { get { return new Vec(GraphicsDeviceManager.PreferredBackBufferWidth, GraphicsDeviceManager.PreferredBackBufferHeight); } set
            {
                if (GraphicsDeviceManager.IsFullScreen)
                {
                    _rememberedDimensions = value;
                }
                else
                {
                    GraphicsDeviceManager.PreferredBackBufferWidth = value.X;
                    GraphicsDeviceManager.PreferredBackBufferHeight = value.Y;

                    GraphicsDeviceManager.ApplyChanges();
                }
            }
        }
        public Vec ScreenCentre { get { return ScreenDimensions / 2; } }
        public SpriteBatch SpriteBatch { get; private set; }
        public Input.KeyState KeyState => _keyState;
        public Input.ControllerState ControllerState => _controllerState;
        public Input.CursorState CursorState => _cursorState;
        public Controls Controls => _controlAssignments;
        public int Ticks => _ticks;
        public GameTime GameTime => _gameTime;
        public TimeSpan FrameTime => _frameTime;
        public int LoadedSoundCount => _soundDict.Count;
        public int StackedSceneCount => _sceneStack.Count;

        // Fields
        private Input.KeyState _keyState;
        private Input.ControllerState _controllerState;
        private Input.CursorState _cursorState;
        private Controls _controlAssignments;

        public Texture2D GlyphTexture { get; private set; }

        private Dictionary<string, SoundEffect> _soundDict;

        private int _ticks;

        private List<IScene> _sceneStack;

        private Func<MonoGameInstance, IScene> _initialSceneConstructor;

        private Vec _rememberedDimensions;

        private GameTime _gameTime;
        private TimeSpan _frameTime;

        // Constructors
        public MonoGameInstance(Func<MonoGameInstance, IScene> initialSceneConstructor, Vec windowedDimensions, bool fullscreen)
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = CONTENT_PATH.Substring(0, CONTENT_PATH.Length - 1);
            IsMouseVisible = true;

            Directory.CreateDirectory(RESOURCES_PATH);
            Directory.CreateDirectory(INTERNAL_RESOURCES_PATH);

            GraphicsDeviceManager.PreferredBackBufferWidth = windowedDimensions.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = windowedDimensions.Y;

            if (fullscreen)
            {
                SetFullscreen();
            }

            _initialSceneConstructor = initialSceneConstructor;
        }

        // Interface Methods
        public new void Exit()
        {
            base.Exit();
        }
        public void SetFullscreen()
        {
            _rememberedDimensions = ScreenDimensions;

            GraphicsDeviceManager.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            GraphicsDeviceManager.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            GraphicsDeviceManager.IsFullScreen = true;
            GraphicsDeviceManager.ApplyChanges();
        }
        public void ExitFullscreen()
        {
            GraphicsDeviceManager.PreferredBackBufferWidth = _rememberedDimensions.X;
            GraphicsDeviceManager.PreferredBackBufferHeight = _rememberedDimensions.Y;

            GraphicsDeviceManager.IsFullScreen = false;
            GraphicsDeviceManager.ApplyChanges();
        }

        public void PlaySound(string sound)
        {
            _soundDict[sound].Play();
        }

        public void SceneIn(IScene newScene)
        {
            _sceneStack.Add(newScene);
        }
        public void SceneReplace(IScene newScene)
        {
            SceneOut();
            SceneIn(newScene);
        }
        public void SceneOut()
        {
            _sceneStack.RemoveAt(_sceneStack.Count - 1);
        }

        public bool ControlIsPressed(string control)
        {
            return _keyState.IsMultiControlPressed(_controlAssignments.GetKeys(control));
        }
        public bool ControlWasJustPressed(string control)
        {
            return _keyState.WasMultiControlJustPressed(_controlAssignments.GetKeys(control));
        }
        public bool ControlWasJustReleased(string control)
        {
            return _keyState.WasMultiControlJustReleased(_controlAssignments.GetKeys(control));
        }

        // Game Methods
        protected override void Initialize()
        {
            //SetFullscreen();

            _keyState = new Input.KeyState(this);
            _cursorState = new Input.CursorState(this);
            _controllerState = new Input.ControllerState();
            _controlAssignments = new Controls(CONTROLS_FILE);

            _gameTime = new GameTime();
            _frameTime = new TimeSpan(0);

            _ticks = 0;

            _sceneStack = new List<IScene>();

            base.Initialize();

            SceneIn(_initialSceneConstructor(this));
            _initialSceneConstructor = null;
        }

        private void LoadTextures()
        {
            GlyphTexture = Content.Load<Texture2D>($"{TEXTURES_PATH_FOR_LOAD}glyphs");
        }
        private void LoadSoundEffects()
        {
            _soundDict = new Dictionary<string, SoundEffect>();

            if (!Directory.Exists(SOUNDS_PATH)) return;

            foreach (string s in Directory.GetFiles(SOUNDS_PATH))
            {
                string pathPart = s.Substring(CONTENT_PATH.Length);
                string extension = s.Split('.').Last();
                string contentPart = s.Substring(0, s.Length - (extension.Length + 1));
                string namePart = contentPart.Split('\\').Last();

                _soundDict.Add(namePart, Content.Load<SoundEffect>(contentPart));
            }
        }
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            LoadTextures();
            LoadSoundEffects();

            PostLoadInitialisation();
        }
        protected void PostLoadInitialisation()
        {
            MonoGameConsole.Initialise(this);
        }

        protected override void Update(GameTime gameTime)
        {
            _gameTime = gameTime;
            _frameTime = gameTime.ElapsedGameTime;

            _keyState.Update();
            _cursorState.Update(this);
            _controllerState.Update();

            _ticks++;

            void updateSceneAt(int index)
            {
                if (_sceneStack[index].UpdatePreviousScene && index != 0)
                {
                    updateSceneAt(index - 1);
                }
                _sceneStack[index].Update(this);
            }

            if (_sceneStack.Count != 0)
            {
                updateSceneAt(_sceneStack.Count - 1);
            }
            else
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            void drawSceneAt(int index)
            {
                if (_sceneStack[index].DrawPreviousScene && index != 0)
                {
                    drawSceneAt(index - 1);
                }
                _sceneStack[index].Draw(this);
            }

            if (_sceneStack.Count != 0)
            {
                drawSceneAt(_sceneStack.Count - 1);
            }

            base.Draw(gameTime);
        }
    }
}
