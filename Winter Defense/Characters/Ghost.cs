using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using System;
using System.Collections.Generic;
using Winter_Defense.Extensions;
using Winter_Defense.Managers;
using Winter_Defense.Particles;

namespace Winter_Defense.Characters
{
    class Ghost : EnemyBase
    {
        //--------------------------------------------------
        // Floating position

        private Vector2 _floatPosition;

        //--------------------------------------------------
        // Explosion

        private bool _exploding;
        private float _explodingEraseInterval;
        private const float ExplodingEraseMaxInterval = 1500.0f;
        private ParticleEffect _explosionParticleEffect;

        //----------------------//------------------------//

        public Ghost(Texture2D texture) : base (texture)
        {
            // Stand
            CharacterSprite.CreateFrameList("stand", 120);
            CharacterSprite.AddCollider("stand", new Rectangle(3, 32, 25, 30));
            CharacterSprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 64)
            });

            // Jumping
            CharacterSprite.CreateFrameList("jumping", 120);
            CharacterSprite.AddCollider("jumping", new Rectangle(3, 32, 25, 30));
            CharacterSprite.AddFrames("jumping", new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 64)
            });

            // Walking
            CharacterSprite.CreateFrameList("walking", 120);
            CharacterSprite.AddCollider("walking", new Rectangle(3, 32, 25, 30));
            CharacterSprite.AddFrames("walking", new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 64),
                new Rectangle(32, 0, 32, 64),
                new Rectangle(64, 0, 32, 64),
                new Rectangle(96, 0, 32, 64)
            });

            // Particles init
            var particleTexture = new Texture2D(SceneManager.Instance.GraphicsDevice, 1, 1);
            particleTexture.SetData(new[] { Color.White });
            ParticlesInit(new TextureRegion2D(particleTexture));
        }

        private void ParticlesInit(TextureRegion2D textureRegion)
        {
            var profile = Profile.Spray(Vector2.One, (float)Math.PI * 2);
            _explosionParticleEffect = new ParticleEffect()
            {
                Emitters = new []
                {
                    new ParticleEmitter(textureRegion, 30, TimeSpan.FromMilliseconds(ExplodingEraseMaxInterval), profile, false)
                    {
                        Parameters = new ParticleReleaseParameters
                        {
                            Speed = new Range<float>(40f, 120f),
                            Quantity = 30,
                            Rotation = new Range<float>(-1f, 1f),
                            Scale = new Range<float>(1f, 4.5f),
                            Color = new HslColor(208.0f, 0.59f, 0.56f)
                        },
                        Modifiers = new IModifier[]
                        {
                            new LinearGravityModifier { Direction = Vector2.UnitY, Strength = 60f },
                            new RotationModifier { RotationRate = 1f },
                            new OpacityFastFadeModifier(),
                            new MapContainerModifier { RestitutionCoefficient = 0.6f }
                        }
                    }
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            if (_exploding)
            {
                var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _explosionParticleEffect.Update(deltaTime);

                _explodingEraseInterval += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                _requestErase = _explodingEraseInterval >= ExplodingEraseMaxInterval;
                return;
            }
            base.Update(gameTime);
        }

        protected override void UpdateSprite(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 3;

            _floatPosition.Y = (float)MathUtils.SinInterpolation(-2, 2, deltaTime);
            CharacterSprite.Offset = _floatPosition;

            base.UpdateSprite(gameTime);
        }

        public override void OnAttack()
        {
            _exploding = true;
            _active = false;
            _explosionParticleEffect.Trigger(BoundingRectangle.Center.ToVector2());
            CharacterSprite.Alpha = 0.0f;
        }

        protected override float MaxMoveSpeed()
        {
            return 35f;
        }

        public override void DrawCharacter(SpriteBatch spriteBatch)
        {
            base.DrawCharacter(spriteBatch);
            spriteBatch.Draw(_explosionParticleEffect);
        }
    }
}
