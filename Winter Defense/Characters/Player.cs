using System;
using System.Collections.Generic;

using Winter_Defense.Managers;
using Winter_Defense.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Winter_Defense.Scenes;
using Winter_Defense.Objects;
using MonoGame.Extended.Particles;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended;
using MonoGame.Extended.Particles.Modifiers;
using Winter_Defense.Particles;

namespace Winter_Defense.Characters
{
    class Player : CharacterBase
    {
        //--------------------------------------------------
        // Attacks constants

        private const int ShotAttack = 0;

        //--------------------------------------------------
        // Bottom sprite

        private CharacterSprite _bottomSprite;

        //--------------------------------------------------
        // Ground impact

        private bool _groundImpact;

        //--------------------------------------------------
        // Recharging

        private bool _recharging;

        //--------------------------------------------------
        // Particle Effects

        private ParticleEffect _shotParticleEffect;
        private ParticleEffect _rechargingParticleEffect;
        private ParticleEffect _walkParticleEffect;
        private ParticleEffect _groundImpactParticleEffect;
        private float _walkParticleEffectInterval;

        //--------------------------------------------------
        // Keys locked (no movement)

        private bool _keysLocked;

        //----------------------//------------------------//

        public Player(Texture2D texture) : base(texture)
        {
            // Stand
            CharacterSprite.CreateFrameList("stand", 120);
            CharacterSprite.AddCollider("stand", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 32)
            });

            // Walking
            CharacterSprite.CreateFrameList("walking", 120);
            CharacterSprite.AddCollider("walking", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFrames("walking", new List<Rectangle>()
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32),
                new Rectangle(128, 0, 32, 32)
            });

            // Jumping up
            CharacterSprite.CreateFrameList("jumping", 60, false);
            CharacterSprite.AddCollider("jumping", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFrames("jumping", new List<Rectangle>()
            {
                new Rectangle(0, 128, 32, 32),
                new Rectangle(32, 128, 32, 32)
            });
            
            // Jumping apex
            CharacterSprite.CreateFrameList("jumping_apex", 120, false);
            CharacterSprite.AddCollider("jumping_apex", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFrames("jumping_apex", new List<Rectangle>()
            {
                new Rectangle(64, 128, 32, 32),
                new Rectangle(96, 128, 32, 32)
            });

            // Jumping falling
            CharacterSprite.CreateFrameList("jumping_impact", 30, false);
            CharacterSprite.AddCollider("jumping_impact", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFrames("jumping_impact", new List<Rectangle>()
            {
                new Rectangle(128, 128, 32, 32),
                new Rectangle(160, 128, 32, 32)
            });

            // Recharging
            CharacterSprite.CreateFrameList("recharging", 120);
            CharacterSprite.AddCollider("recharging", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFrames("recharging", new List<Rectangle>()
            {
                new Rectangle(0, 64, 64, 32),
                new Rectangle(64, 64, 32, 32),
                new Rectangle(96, 64, 64, 32),
                new Rectangle(160, 64, 32, 32)
            }, new int[] { 0, 0, -32, 0 }, new int[] { 0, 0, 0, 0 });

            // Shot
            CharacterSprite.CreateFrameList("attack_shot", 80, false);
            CharacterSprite.AddCollider("attack_shot", new Rectangle(6, 2, 16, 30));
            CharacterSprite.AddFramesToAttack("attack_shot", 0);
            CharacterSprite.AddFrames("attack_shot", new List<Rectangle>()
            {
                new Rectangle(0, 32, 64, 32),
                new Rectangle(64, 32, 64, 32),
                new Rectangle(128, 32, 32, 32),
                new Rectangle(160, 33, 32, 31)
            }, new int[] { 0, 0, 0, 0 }, new int[] { 0, 0, 0, 1 });

            SetupBottomSprite(texture);

            Position = new Vector2(32, 160);

            // Attacks setup
            _attackFrameList = new string[]
            {
                "attack_shot"
            };
            AttackCooldown = 300f;

            // Particles init
            var particleTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            ParticlesInit(new TextureRegion2D(particleTexture));
        }

        private void SetupBottomSprite(Texture2D texture)
        {
            // Stand
            _bottomSprite = new CharacterSprite(texture);
            _bottomSprite.CreateFrameList("stand", 120);
            _bottomSprite.AddCollider("stand", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(160, 0, 32, 32)
            });

            // Walking
            _bottomSprite.CreateFrameList("walking", 120);
            _bottomSprite.AddCollider("walking", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("walking", new List<Rectangle>()
            {
                new Rectangle(0, 160, 32, 32),
                new Rectangle(32, 160, 32, 32),
                new Rectangle(64, 160, 32, 32),
                new Rectangle(96, 160, 32, 32)
            });

            // Jumping up
            _bottomSprite.CreateFrameList("jumping", 60, false);
            _bottomSprite.AddCollider("jumping", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("jumping", new List<Rectangle>()
            {
                new Rectangle(0, 288, 32, 32),
                new Rectangle(32, 288, 32, 32)
            });

            // Jumping apex
            _bottomSprite.CreateFrameList("jumping_apex", 120, false);
            _bottomSprite.AddCollider("jumping_apex", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("jumping_apex", new List<Rectangle>()
            {
                new Rectangle(64, 288, 32, 32),
                new Rectangle(96, 289, 32, 31)
            }, new int[] { 0, 0 }, new int[] { 0, 1 });

            // Jumping falling
            _bottomSprite.CreateFrameList("jumping_impact", 30, false);
            _bottomSprite.AddCollider("jumping_impact", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("jumping_impact", new List<Rectangle>()
            {
                new Rectangle(128, 288, 32, 32),
                new Rectangle(160, 288, 32, 32)
            });

            // Recharging
            _bottomSprite.CreateFrameList("recharging", 120);
            _bottomSprite.AddCollider("recharging", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("recharging", new List<Rectangle>()
            {
                new Rectangle(0, 225, 32, 32),
                new Rectangle(32, 225, 32, 32),
                new Rectangle(64, 225, 32, 32),
                new Rectangle(96, 225, 32, 32)
            }, new int[] { 0, 0, 0, 0 }, new int[] { 1, 1, 1, 1 });

            // Shot
            _bottomSprite.CreateFrameList("attack_shot", 80, false);
            _bottomSprite.AddCollider("attack_shot", new Rectangle(6, 2, 16, 30));
            _bottomSprite.AddFrames("attack_shot", new List<Rectangle>()
            {
                new Rectangle(0, 193, 32, 31),
                new Rectangle(32, 193, 32, 31),
                new Rectangle(64, 193, 32, 31),
                new Rectangle(96, 193, 32, 31)
            }, new int[] { 0, 0, 0, 0 }, new int[] { 1, 1, 1, 1 });
        }

        private void ParticlesInit(TextureRegion2D textureRegion)
        {
            var shotProfile = Profile.Spray(new Vector2(1, 0), (float)Math.PI / 3.0f);
            _shotParticleEffect = new ParticleEffect
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 25, TimeSpan.FromSeconds(1), shotProfile, false)
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(30f, 100f),
                            Quantity = 5,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1.5f, 3.5f),
                            Color = new Range<HslColor>(new HslColor(186, 0.8f, 0.96f), new HslColor(186, 1.0f, 0.96f))
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 90f },
                            new RotationModifier { RotationRate = 1f },
                            new OpacityFastFadeModifier(),
                            new MapContainerModifier { RestitutionCoefficient = 0.3f }
                        }
                    }
                }
            };
            var rechargingProfile = Profile.Line(new Vector2(-1, -1), 5.0f);
            _rechargingParticleEffect = new ParticleEffect
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 500, TimeSpan.FromSeconds(0.2f), rechargingProfile, false)
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(20f, 40f),
                            Quantity = 3,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1.0f, 3.0f),
                            Color = new HslColor(186, 0.13f, 0.96f)
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = -Vector2.One, Strength = 200f },
                            new MapContainerModifier { RestitutionCoefficient = 0.3f },
                            new OpacityFastFadeModifier()
                        }
                    }
                }
            };
            _groundImpactParticleEffect = new ParticleEffect
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 50, TimeSpan.FromSeconds(0.4f), Profile.Line(Vector2.UnitX, 11))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(20f, 40f),
                            Quantity = 10,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(3.0f, 5.0f),
                            Color = new HslColor(186, 0.13f, 0.96f)
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 70f },
                            new MapContainerModifier { RestitutionCoefficient = 0.3f },
                            new OpacityFastFadeModifier()
                        }
                    }
                }
            };
            _walkParticleEffect = new ParticleEffect
            {
                Emitters = new[]
                {
                    new ParticleEmitter(textureRegion, 50, TimeSpan.FromSeconds(0.4f), Profile.Line(Vector2.UnitX, 10))
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(20f, 30f),
                            Quantity = 5,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(3.0f, 5.0f),
                            Color = new HslColor(186, 0.13f, 0.96f),
                            Opacity = 0.9f
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.Zero, Strength = 100.0f },
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 70.0f },
                            new MapContainerModifier { RestitutionCoefficient = 0.3f },
                            new OpacityFastFadeModifier()
                        }
                    }
                }
            };
        }

        public void UpdateWithKeyLock(GameTime gameTime, bool keyLock)
        {
            _keysLocked = keyLock;
            if (!keyLock)
                CheckKeys(gameTime);
            base.Update(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var isOnGroundBefore = _isOnGround;
            CheckKeys(gameTime);
            base.Update(gameTime);
            UpdateSprites(gameTime, isOnGroundBefore);
            UpdateParticles(deltaTime);

            if (InputManager.Instace.KeyDown(Keys.P))
                _rechargingParticleEffect.Trigger(SceneManager.Instance.VirtualSize / 2);
        }

        private void UpdateSprites(GameTime gameTime, bool isOnGroundBefore)
        {
            UpdateBottomSprite(gameTime);
            if (_isOnGround && !isOnGroundBefore && !_groundImpact)
            {
                _groundImpact = true;
                TriggerGroundImpactParticles();
            }
            if (_groundImpact && CharacterSprite.Looped)
            {
                _groundImpact = false;
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            _shotParticleEffect.Update(deltaTime);
            _rechargingParticleEffect.Update(deltaTime);
            _walkParticleEffect.Update(deltaTime);
            _groundImpactParticleEffect.Update(deltaTime);

            _walkParticleEffectInterval += deltaTime;
            if (WalkingByInput() && _isOnGround && _walkParticleEffectInterval > 0.2f)
            {
                TriggerWalkParticles();
                _walkParticleEffectInterval = 0.0f;
            }
        }

        private void UpdateBottomSprite(GameTime gameTime)
        {
            _bottomSprite.Update(gameTime);
        }

        public override void UpdateFrameList()
        {
            // Main Sprite
            if (_dying)
            {
                CharacterSprite.SetIfFrameListExists("dying");
            }
            else if (_isAttacking)
            {
                CharacterSprite.SetFrameList(_attackFrameList[_attackType]);
            }
            else if (_recharging)
            {
                CharacterSprite.SetFrameList("recharging");
            }
            else if (_groundImpact)
            {
                CharacterSprite.SetFrameList("jumping_impact");
            }
            else if (!_isOnGround)
            {
                if (Math.Abs(_velocity.Y) < 100.0f || _velocity.Y > 0)
                {
                    CharacterSprite.SetFrameList("jumping_apex");
                }
                else
                {
                    CharacterSprite.SetFrameList("jumping");
                }
            }
            else if (WalkingByInput())
            {
                CharacterSprite.SetFrameList("walking");
            }
            else
            {
                CharacterSprite.SetFrameList("stand");
            }

            // Bottom sprite specials motions
            if (!_isOnGround)
            {
                if (Math.Abs(_velocity.Y) < 100.0f || _velocity.Y > 0)
                {
                    _bottomSprite.SetFrameList("jumping_apex");
                }
                else
                {
                    _bottomSprite.SetFrameList("jumping");
                }
            }
            else if (_recharging && !WalkingByInput())
            {
                _bottomSprite.SetFrameList("recharging");
            }
            else if (_groundImpact && !_recharging)
            {
                _bottomSprite.SetFrameList("jumping_impact");
            }
            else if (WalkingByInput())
            {
                _bottomSprite.SetFrameList("walking");
            }
            else if (_isAttacking)
            {
                _bottomSprite.SetFrameList(_attackFrameList[_attackType]);
            }
            else
            {
                _bottomSprite.SetFrameList("stand");
            }
        }

        private void CheckKeys(GameTime gameTime)
        {
            if (_dying)
                return;

            // Movement
            if (InputManager.Instace.KeyDown(Keys.Left) && Math.Abs(_knockbackAcceleration) < 1200f)
            {
                CharacterSprite.SetDirection(SpriteDirection.Left);
                _bottomSprite.SetDirection(SpriteDirection.Left);
                _movement = -1.0f;
            }
            else if (InputManager.Instace.KeyDown(Keys.Right) && Math.Abs(_knockbackAcceleration) < 1200f)
            {
                CharacterSprite.SetDirection(SpriteDirection.Right);
                _bottomSprite.SetDirection(SpriteDirection.Right);
                _movement = 1.0f;
            }

            // Attack
            if (!_isAttacking && InputManager.Instace.KeyDown(Keys.Z))
                RequestAttack(ShotAttack);

            // Jump
            _isJumping = InputManager.Instace.KeyDown(Keys.C);

            // Recharging
            _recharging = _isOnGround && InputManager.Instace.KeyDown(Keys.X);
            if (_recharging && CharacterSprite.CurrentFrame == 0)
            {
                TriggerRechargingParticles();
            }
        }

        public override void DoAttack()
        {
            if (_shot) return;
            _shot = true;
            var damage = 1;
            var position = Position;
            var dx = 300;

            // Initial position of the projectile
            if (CharacterSprite.Effect == SpriteEffects.FlipHorizontally)
            {
                position += new Vector2(-2, 12);
                dx *= -1;
            }
            else
            {
                position += new Vector2(25, 12);
            }

            var sign = Math.Sign(dx);
            var particlePosition = new Vector2(position.X + 5, position.Y + 5);
            TriggerShotParticles(new Vector2(sign, 0), particlePosition);
            _knockbackAcceleration = 3800.0f * -sign;
            ((SceneMap)SceneManager.Instance.GetCurrentScene()).CreateProjectile("snowball", position, dx, 0, damage, ProjectileSubject.FromPlayer);
        }

        private void TriggerShotParticles(Vector2 direction, Vector2 position)
        {
            var profile = (SprayProfile)_shotParticleEffect.Emitters[0].Profile;
            profile.Direction = direction;
            _shotParticleEffect.Trigger(position);
        }

        private void TriggerRechargingParticles()
        {
            var linearModifier = (LinearGravityModifier)_rechargingParticleEffect.Emitters[0].Modifiers[0];
            var profile = (LineProfile)_rechargingParticleEffect.Emitters[0].Profile;
            var positionInc = new Vector2(34, 28);
            if (CharacterSprite.Effect == SpriteEffects.FlipHorizontally)
            {
                positionInc.X = -5;
                linearModifier.Direction = new Vector2(1, -1);
                profile.Axis = new Vector2(1, -1);
            }
            else
            {
                linearModifier.Direction = -Vector2.One;
                profile.Axis = new Vector2(-1, -1);
            }
            _rechargingParticleEffect.Trigger(Position + positionInc);
        }

        private void TriggerGroundImpactParticles()
        {
            var position = new Vector2(BoundingRectangle.Center.X, BoundingRectangle.Bottom);
            _groundImpactParticleEffect.Trigger(position);
        }

        private void TriggerWalkParticles()
        {
            var inc = Math.Sign(_velocity.X) * 5;
            var position = new Vector2(BoundingRectangle.Center.X + inc, BoundingRectangle.Bottom);
            var linearModifier = (LinearGravityModifier)_walkParticleEffect.Emitters[0].Modifiers[0];
            linearModifier.Direction = Math.Sign(_velocity.X) * -Vector2.UnitX;
            _walkParticleEffect.Trigger(position);
        }

        private bool WalkingByInput()
        {
            return (InputManager.Instace.KeyDown(Keys.Left) || InputManager.Instace.KeyDown(Keys.Right)) && !_keysLocked;
        }

        #region Draw
        public override void DrawCharacter(SpriteBatch spriteBatch)
        {
            _bottomSprite.Draw(spriteBatch, new Vector2(BoundingRectangle.X, BoundingRectangle.Y));
            base.DrawCharacter(spriteBatch);
            spriteBatch.Draw(_shotParticleEffect);
            spriteBatch.Draw(_rechargingParticleEffect);
            spriteBatch.Draw(_walkParticleEffect);
            spriteBatch.Draw(_groundImpactParticleEffect);
        }
        #endregion
    }
}
