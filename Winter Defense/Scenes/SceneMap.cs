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
using Microsoft.Xna.Framework.Input;

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
        // Enemies

        private List<EnemyBase> _enemies;
        private Dictionary<EnemyType, string> _enemiesNames;

        //--------------------------------------------------
        // Crystal

        private GameCrystal _crystal;

        //--------------------------------------------------
        // Particle Effects

        private List<ParticleEffect> _particleEffects;

        private ParticleEffect _snowballDestroyParticleEffect;
        private ParticleEffect _blizzardParticleEffect;

        private Vector2 _blizzardTriggerPosition;

        //--------------------------------------------------
        // Background

        private Texture2D _backgroundTexture;

        //--------------------------------------------------
        // Hud

        private GameHud _gameHud;

        //--------------------------------------------------
        // Enemies Spawn Manager

        private EnemiesSpawnManager _enemiesSpawnManager;

        //--------------------------------------------------
        // Wave Interval

        private bool _waveInterval;
        private float _waveIntervalTick;
        private ParticleEffect _explosionParticleEffect;

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

            // Enemies init
            _enemies = new List<EnemyBase>();
            _enemiesNames = new Dictionary<EnemyType, string>
            {
                { EnemyType.Ghost, "Ghost" },
                { EnemyType.Bird, "Bird" }
            };

            // Background init
            _backgroundTexture = ImageManager.loadScene("sceneMap", "Background");

            // Hud init
            _gameHud = new GameHud();

            // Particles init
            var particleTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            ParticlesInit(new TextureRegion2D(particleTexture));

            _blizzardTriggerPosition = new Vector2(SceneManager.Instance.VirtualSize.X / 2 - 50.0f, -50.0f);

            // Spawn Manager init
            _enemiesSpawnManager = new EnemiesSpawnManager();
            _enemiesSpawnManager.Start();

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
            var profile = Profile.Spray(Vector2.One, (float)Math.PI * 2);
            _explosionParticleEffect = new ParticleEffect()
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 50, TimeSpan.FromMilliseconds(1500.0f), profile, false)
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(40f, 100f),
                            Quantity = 20,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1f, 4.5f),
                            Color = new HslColor(208.0f, 0.59f, 0.56f)
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 90f },
                            new RotationModifier { RotationRate = 1f },
                            new OpacityFastFadeModifier(),
                            new MapContainerModifier { RestitutionCoefficient = 0.6f }
                        }
                    }
                }
            };
            _particleEffects = new List<ParticleEffect>();
            _particleEffects.Add(_blizzardParticleEffect);
            _particleEffects.Add(_snowballDestroyParticleEffect);
            _particleEffects.Add(_explosionParticleEffect);
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

            UpdateCrystal(gameTime);
            if (_crystal.Breaking) return;
            _player.Update(gameTime);
            UpdateParticles(deltaTime);
            UpdateProjectiles(gameTime);
            UpdateEnemiesSpawn(gameTime);
            UpdateEnemies(gameTime);
            UpdateWave(gameTime);
            UpdateHud(gameTime);
            UpdateCamera();

            if (InputManager.Instace.KeyPressed(Keys.P))
                _explosionParticleEffect.Trigger(SceneManager.Instance.VirtualSize / 2);

            base.Update(gameTime);
            
            DebugValues["Delta Time"] = gameTime.ElapsedGameTime.TotalMilliseconds.ToString();
            var s = _enemies.Count > 0 ? _enemies[0].CharacterSprite.CurrentFrameList.ToString() : "";
            DebugValues["Enemy Frame List"] = s;
            DebugValues["Player Frame List"] = _player.CharacterSprite.CurrentFrameList.ToString();
            DebugValues["Spawn Time"] = _enemiesSpawnManager.SpawnInterval.ToString();
        }

        private void UpdateCrystal(GameTime gameTime)
        {
            _crystal.Update(gameTime);
            if (_crystal.Lives == 0)
            {
                SceneManager.Instance.ChangeScene("SceneGameover");
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            _particleEffects.ForEach(particle => particle.Update(deltaTime));
            _blizzardParticleEffect.Trigger(_blizzardTriggerPosition);
        }

        private void UpdateProjectiles(GameTime gameTime)
        {
            var toRemove = new List<GameProjectile>();
            foreach (var projectile in _projectiles)
            {
                projectile.Update(gameTime);

                if (projectile.RequestErase)
                {
                    if (projectile.RequestParticles)
                    {
                        var spriteBr = projectile.Sprite.BoundingRectangle;
                        var particlePosition = new Vector2(spriteBr.Center.X, spriteBr.Center.Y);
                        _snowballDestroyParticleEffect.Trigger(particlePosition);
                    }
                    toRemove.Add(projectile);
                }
            }
            toRemove.ForEach(projectile => _projectiles.Remove(projectile));
            toRemove.Clear();
        }

        private void UpdateEnemiesSpawn(GameTime gameTime)
        {
            _enemiesSpawnManager.Update(gameTime);
            while (_enemiesSpawnManager.Queue.Count > 0)
            {
                var model = _enemiesSpawnManager.ShiftModelFromQueue();
                var enemyName = _enemiesNames[model.Type];
                var texture = ImageManager.loadCharacter(enemyName);
                var enemy = (EnemyBase)Activator.CreateInstance(Type.GetType("Winter_Defense.Characters." + enemyName), texture);
                var halfTile = MapManager.Instance.TileSize.X / 2;
                var x = model.Side == 0 ? halfTile : MapManager.Instance.MapWidth - halfTile;
                var y = MapManager.Instance.MapHeight - MapManager.Instance.TileSize.Y;
                if (model.Type == EnemyType.Bird) y += 32;
                enemy.SetPositionFromGround(new Vector2(x, y));
                _enemies.Add(enemy);
            }
        }

        private void UpdateEnemies(GameTime gameTime)
        {
            var toRemove = new List<EnemyBase>();
            foreach (var enemy in _enemies)
            {
                enemy.Update(gameTime);
                if (enemy.Active)
                {
                    foreach (var projectile in _projectiles)
                    {
                        if (projectile.Active && projectile.BoundingBox.Intersects(enemy.BoundingRectangle))
                        {
                            enemy.ReceiveAttack(1, projectile.LastPosition);
                            projectile.Destroy(true);
                        }
                    }

                    if (enemy.BoundingRectangle.Intersects(_crystal.BoudingBox))
                    {
                        _crystal.OnDamage();
                        enemy.OnAttack();
                    }
                }
                if (enemy.RequestErase)
                    toRemove.Add(enemy);
            }
            toRemove.ForEach(enemy => _enemies.Remove(enemy));
            toRemove.Clear();
        }

        private void UpdateWave(GameTime gameTime)
        {
            if (_waveInterval)
            {
                _waveIntervalTick -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_waveIntervalTick <= 0.0f)
                {
                    _waveInterval = false;
                    _enemiesSpawnManager.StartNextWave();
                }
            }
            else
            {
                if (_enemies.Count == 0 && _enemiesSpawnManager.WaveCompleted)
                {
                    _waveInterval = true;
                    _waveIntervalTick = 2000.0f;
                }
            }
        }

        private void UpdateHud(GameTime gameTime)
        {
            _gameHud.SetData(_crystal.Lives, _player.Ammo, _enemiesSpawnManager.CurrentWave);
            _gameHud.Update(gameTime);
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
            if (debugMode) _crystal.DrawCollider(spriteBatch);

            // Draw the player
            _player.DrawCharacter(spriteBatch);
            if (debugMode) _player.DrawColliderBox(spriteBatch);

            // Draw the enemies
            _enemies.ForEach(enemy => enemy.DrawCharacter(spriteBatch));
            if (debugMode) _enemies.ForEach(enemy => enemy.DrawColliderBox(spriteBatch));

            // Draw the projectiles
            foreach (var projectile in _projectiles)
            {
                projectile.Draw(spriteBatch);
                if (debugMode) spriteBatch.Draw(_projectilesColliderTexture, projectile.BoundingBox, Color.White * 0.5f);
            }

            // Draw the particles
            _particleEffects.ForEach(particle => spriteBatch.Draw(particle));

            spriteBatch.End();

            // Draw the HUD
            spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix(), samplerState: SamplerState.PointClamp);
            _gameHud.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
