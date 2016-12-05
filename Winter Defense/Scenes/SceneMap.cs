using System;

using Winter_Defense.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using MonoGame.Extended.Particles;
using Winter_Defense.Characters;
using System.Collections.Generic;
using Winter_Defense.Objects;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.Particles.Modifiers;
using Winter_Defense.Particles;

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
        // Crystal

        private GameCrystal _crystal;

        //--------------------------------------------------
        // Particle Effects

        private ParticleEffect _snowballDestroyParticleEffect;
        private ParticleEffect _blizzardParticleEffect;

        //--------------------------------------------------
        // Background

        private Texture2D _backgroundTexture;

        //--------------------------------------------------
        // Half Screen Size

        private Vector2 _halfScreenSize;

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

            // Camera init
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
            _projectilesColliderTexture.SetData(new Color[] { Color.Orange });

            // Background init
            _backgroundTexture = ImageManager.loadScene("sceneMap", "Background");

            // Particles init
            var particleTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            ParticlesInit(new TextureRegion2D(particleTexture));

            // Misc init
            _halfScreenSize = SceneManager.Instance.VirtualSize / 2;

            // Random init
            _rand = new Random();

            // Load the map
            LoadMap(MapManager.FirstMap);

            // Crystal init
            var mapSize = new Vector2(MapManager.Instance.MapWidth, MapManager.Instance.MapHeight);
            var crystalPosition = new Vector2(mapSize.X / 2 - 48, 96);
            _crystal = new GameCrystal(crystalPosition, ImageManager.loadCharacter("Crystal"));
        }

        private void ParticlesInit(TextureRegion2D textureRegion)
        {

            var blizzardProfile = Profile.Line(Vector2.UnitX, SceneManager.Instance.VirtualSize.X + 50.0f);
            _blizzardParticleEffect = new ParticleEffect
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 1000, TimeSpan.FromSeconds(6.0f), blizzardProfile, false)
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(5f, 15f),
                            Quantity = 2,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1.0f, 3.0f),
                            Color = new HslColor(186, 0.13f, 0.96f),
                            Opacity = 0.9f
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 20f },
                            new LinearGravityModifier { Direction = new Vector2(1, 0), Strength = 5f },
                            new RotationModifier { RotationRate = 1.0f }
                        }
                    }
                }
            };
            _snowballDestroyParticleEffect = new ParticleEffect()
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(1.5), Profile.Spray(-Vector2.UnitY, (float)Math.PI), false)
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(30f, 60f),
                            Quantity = 10,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(2.0f, 4.5f),
                            Color = new HslColor(186, 0.13f, 0.96f)
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 150f },
                            new MapContainerModifier { RestitutionCoefficient = 0.2f },
                            new RotationModifier { RotationRate = 2.0f },
                            new OpacityFastFadeModifier()
                        }
                    }
                }
            };
        }

        private void LoadMap(int mapId)
        {
            MapManager.Instance.LoadMap(Content, mapId);
            InitMapObjects();
        }

        private void InitMapObjects()
        {
            SpawnPlayer();
        }

        private void SpawnPlayer()
        {
            var spawnObject = MapManager.Instance.GetPlayerSpawn();
            var spawnPoint = new Vector2(spawnObject.Position.X, spawnObject.Position.Y);
            _player.Position = new Vector2(spawnPoint.X, spawnPoint.Y - _player.CharacterSprite.GetColliderHeight());
        }

        public void CreateProjectile(string name, Vector2 position, int dx, int dy, int damage, ProjectileSubject subject)
        {
            _projectiles.Add(new GameProjectile(_projectilesTextures[name], position, dx, dy, damage, subject));
        }

        public override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _crystal.Update(gameTime);
            _player.Update(gameTime);
            _snowballDestroyParticleEffect.Update(deltaTime);
            _blizzardParticleEffect.Update(deltaTime);
            _blizzardParticleEffect.Trigger(new Vector2(_halfScreenSize.X - 50.0f, -50.0f));
            UpdateCamera();
            base.Update(gameTime);

            for (var i = 0; i < _projectiles.Count; i++)
            {
                _projectiles[i].Update(gameTime);
                if (_projectiles[i].Subject == ProjectileSubject.FromEnemy && _projectiles[i].BoundingBox.Intersects(_player.BoundingRectangle))
                    _player.ReceiveAttack(_projectiles[i].Damage, _projectiles[i].LastPosition);

                if (_projectiles[i].RequestErase)
                {
                    if (_projectiles[i].RequestParticles)
                    {
                        var spriteBr = _projectiles[i].Sprite.BoundingRectangle;
                        var particlePosition = new Vector2((int)spriteBr.Center.X, (int)spriteBr.Center.Y);
                        _snowballDestroyParticleEffect.Trigger(particlePosition);
                    }
                    _projectiles.Remove(_projectiles[i]);
                }
            }

            DebugValues["Delta Time"] = gameTime.ElapsedGameTime.TotalMilliseconds.ToString();
            DebugValues["Player Frame List"] = _player.CharacterSprite.CurrentFrameList.ToString();
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

            // Draw the background
            spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix(), samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            spriteBatch.End();

            // Draw the camera (with the map)
            MapManager.Instance.Draw(_camera, spriteBatch);
            
            spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

            // Draw the crystal
            _crystal.Draw(spriteBatch);

            // Draw the player
            _player.DrawCharacter(spriteBatch);
            if (debugMode) _player.DrawColliderBox(spriteBatch);

            // Draw the projectiles
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(spriteBatch);
                if (debugMode) spriteBatch.Draw(_projectilesColliderTexture, projectile.BoundingBox, Color.White * 0.5f);
            }

            // Draw the particles
            spriteBatch.Draw(_snowballDestroyParticleEffect);
            spriteBatch.Draw(_blizzardParticleEffect);

            spriteBatch.End();
        }
    }
}
