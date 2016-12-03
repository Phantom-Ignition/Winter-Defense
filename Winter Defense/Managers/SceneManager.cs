using System;

using Winter_Defense.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;

namespace Winter_Defense.Managers
{
    class SceneManager
    {
        //--------------------------------------------------
        // Public variables

        public Vector2 WindowSize = new Vector2(852, 480);
        public Vector2 VirtualSize = new Vector2(427, 240);
        public GraphicsDevice GraphicsDevice;
        public SpriteBatch SpriteBatch;
        public ViewportAdapter ViewportAdapter => GameMain.ViewportAdapter;
        public ContentManager Content { private set; get; }

        public bool RequestingExit = false;

        //--------------------------------------------------
        // SceneManager Singleton variables

        private static SceneManager _instance = null;
        private static readonly object _padlock = new object();
        public static SceneManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    if (_instance == null)
                        _instance = new SceneManager();
                    return _instance;
                }
            }
        }

        //--------------------------------------------------
        // Transition

        private SceneBase _currentScene, _newScene;
        private Sprite _transitionImage;
        private bool _isTransitioning = false;
        public bool IsTransitioning => _isTransitioning;
        private bool _beginTransitionFade = false;

        //--------------------------------------------------
        // Map transitions

        private int? _mapToLoad;
        private bool _mapTransition = false;
        private Action<int> _mapLoadCallback;

        //--------------------------------------------------
        // Debug mode

        public bool DebugMode { get; set; } = false;

        //----------------------//------------------------//

        private SceneManager()
        {
            _currentScene = new SceneMap();
        }

        public void RequestExit()
        {
            RequestingExit = true;
        }

        public SceneBase GetCurrentScene()
        {
            return _currentScene;
        }

        public void LoadContent(ContentManager Content)
        {
            this.Content = new ContentManager(Content.ServiceProvider, "Content");
            var transitionTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            transitionTexture.SetData<Color>(new Color[] { Color.Black });
            _transitionImage = new Sprite(transitionTexture);
            _transitionImage.Scale = new Vector2(VirtualSize.X, VirtualSize.Y);
            _transitionImage.Alpha = 0.0f;
            _transitionImage.IsVisible = false;
            _currentScene.LoadContent();
        }

        public void UnloadContent()
        {
            _currentScene.UnloadContent();
        }

        public void Update(GameTime gameTime)
        {
            if (_isTransitioning)
                UpdateTransition(gameTime);
            else if (InputManager.Instace.KeyPressed(Keys.F2))
                DebugMode = !DebugMode;

            _currentScene.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentScene.Draw(spriteBatch, ViewportAdapter);
            spriteBatch.Begin();
            spriteBatch.Draw(_transitionImage.TextureRegion.Texture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White * _transitionImage.Alpha);
            _currentScene.DrawDebugValues(spriteBatch);
            spriteBatch.End();
        }

        public void ChangeScene(string newScene)
        {
            if (_isTransitioning) return;
            _newScene = (SceneBase)Activator.CreateInstance(Type.GetType("Winter_Defense.Scenes." + newScene));
            InitializeTransition();
        }

        public void MapTransition(int mapId, Action<int> loadCallback = null)
        {
            if (_isTransitioning) return;
            _mapTransition = true;
            _mapToLoad = mapId;
            _mapLoadCallback = loadCallback;
            InitializeTransition();
        }

        private void InitializeTransition()
        {
            _transitionImage.Alpha = 0;
            _transitionImage.IsVisible = true;
            _isTransitioning = true;
            _beginTransitionFade = true;
        }

        private void UpdateTransition(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_beginTransitionFade)
            {
                if (_transitionImage.Alpha < 1.0f)
                    _transitionImage.Alpha += deltaTime / 200f;
                else
                    _beginTransitionFade = false;
            }
            else
            {
                if (_mapTransition && _mapToLoad != null)
                {
                    var mapId = _mapToLoad ?? MapManager.FirstMap;
                    MapManager.Instance.LoadMap(Content, mapId);
                    if (_mapLoadCallback != null)
                    {
                        _mapLoadCallback(mapId);
                        _mapLoadCallback = null;
                    }
                    _mapToLoad = null;
                    _mapTransition = false;
                }

                if (!_mapTransition && _newScene != null)
                {
                    _currentScene.UnloadContent();
                    _currentScene = _newScene;
                    _currentScene.LoadContent();
                    _newScene = null;
                }

                if (_transitionImage.Alpha > 0.0f)
                {
                    _transitionImage.Alpha -= deltaTime / 200f;
                }
                else
                {
                    _transitionImage.IsVisible = false;
                    _isTransitioning = false;
                }
            }
        }
    }
}
