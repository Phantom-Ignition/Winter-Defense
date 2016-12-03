using System;

using Winter_Defense.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using Winter_Defense.Characters;
using System.Collections.Generic;
using Winter_Defense.Objects;

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
        // Projectiles

        private Dictionary<string, Texture2D> _projectilesTextures;
        private Texture2D _projectilesColliderTexture;
        private List<GameProjectile> _projectiles;

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

            // Projectiles init
            _projectilesTextures = new Dictionary<string, Texture2D>()
            {
                {"snowball", ImageManager.loadProjectile("Snowball")},
            };
            _projectiles = new List<GameProjectile>();
            _projectilesColliderTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            _projectilesColliderTexture.SetData<Color>(new Color[] { Color.Orange });

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

        public void CreateProjectile(string name, Vector2 position, int dx, int dy, int damage, ProjectileSubject subject)
        {
            _projectiles.Add(new GameProjectile(_projectilesTextures[name], position, dx, dy, damage, subject));
        }

        public override void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
            UpdateCamera();
            base.Update(gameTime);

            for (var i = 0; i < _projectiles.Count; i++)
            {
                _projectiles[i].Update(gameTime);
                if (_projectiles[i].Subject == ProjectileSubject.FromEnemy && _projectiles[i].BoundingBox.Intersects(_player.BoundingRectangle))
                    _player.ReceiveAttack(_projectiles[i].Damage, _projectiles[i].LastPosition);

                if (_projectiles[i].RequestErase)
                    _projectiles.Remove(_projectiles[i]);
            }

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

            // Draw the projectiles
            foreach (var projectile in _projectiles)
            {
                spriteBatch.Draw(projectile.Sprite);
                if (debugMode) spriteBatch.Draw(_projectilesColliderTexture, projectile.BoundingBox, Color.White * 0.5f);
            }

            spriteBatch.End();
        }
    }
}
