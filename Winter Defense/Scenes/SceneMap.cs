using System;

using Winter_Defense.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using Winter_Defense.Characters;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Winter_Defense.Scenes
{
    class SceneMap : SceneBase
    {
        //--------------------------------------------------
        // Camera stuff

        private Camera2D _camera;
        private const float CameraSmooth = 0.1f;
        private const int PlayerCameraOffsetX = 20;
        private const int PlayerCameraOffsetY = 0;

        //--------------------------------------------------
        // Player

        private Player _player;

        public Player Player { get { return _player; } }

        //--------------------------------------------------
        // Random

        private Random _rand;

        //----------------------//------------------------//

        public Camera2D GetCamera()
        {
            return _camera;
        }

        public override void LoadContent()
        {
            base.LoadContent();

            var viewportSize = SceneManager.Instance.VirtualSize;
            _camera = new Camera2D(SceneManager.Instance.ViewportAdapter);

            // Player init
            _player = new Player(ImageManager.loadCharacter("Player"));

            // Random init
            _rand = new Random();

            // Load the map
            LoadMap(MapManager.FirstMap);
        }

        private void LoadMap(int mapId)
        {
            MapManager.Instance.LoadMap(Content, mapId);
            InitMapObjects();
        }

        private void MapLoadedFromTransition(int mapId)
        {
            InitMapObjects();
        }

        private void InitMapObjects()
        {
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            var spawnPoint = new Vector2(MapManager.Instance.GetPlayerSpawn().X, MapManager.Instance.GetPlayerSpawn().Y);
            _player.Position = new Vector2(spawnPoint.X, spawnPoint.Y - _player.CharacterSprite.GetColliderHeight());
        }

        public override void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
            UpdateCamera();
            base.Update(gameTime);

            DebugValues["Delta Time"] = gameTime.ElapsedGameTime.TotalMilliseconds.ToString();
            DebugValues["Player Y"] = _player.Velocity.Y.ToString();
        }

        private void UpdateCamera()
        {
            var size = SceneManager.Instance.WindowSize;
            var viewport = SceneManager.Instance.ViewportAdapter;
            var newPosition = _player.Position - new Vector2(viewport.VirtualWidth / 2f, viewport.VirtualHeight / 2f);
            var playerOffsetX = PlayerCameraOffsetX + _player.CharacterSprite.GetColliderWidth() / 2;
            var playerOffsetY = PlayerCameraOffsetY + _player.CharacterSprite.GetFrameHeight() / 2;
            var x = MathHelper.Lerp(_camera.Position.X, newPosition.X + playerOffsetX, CameraSmooth);
            x = MathHelper.Clamp(x, 0.0f, MapManager.Instance.MapWidth - viewport.VirtualWidth);
            var y = MathHelper.Lerp(_camera.Position.Y, newPosition.Y + playerOffsetY, CameraSmooth);
            y = MathHelper.Clamp(y, 0.0f, MapManager.Instance.MapHeight - viewport.VirtualHeight);
            _camera.Position = new Vector2((int)x, (int)y);
        }

        public override void Draw(SpriteBatch spriteBatch, ViewportAdapter viewportAdapter)
        {
            base.Draw(spriteBatch, viewportAdapter);
            var debugMode = SceneManager.Instance.DebugMode;

            // Draw the camera (with the map)
            MapManager.Instance.Draw(_camera, spriteBatch);

            spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

            // Draw the player
            _player.DrawCharacter(spriteBatch);
            if (debugMode) _player.DrawColliderBox(spriteBatch);

            spriteBatch.End();
        }
    }
}
