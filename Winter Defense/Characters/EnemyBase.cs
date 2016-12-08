using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;
using System;
using Winter_Defense.Extensions;
using Winter_Defense.Managers;
using Winter_Defense.Particles;

namespace Winter_Defense.Characters
{
    //--------------------------------------------------
    // Enemy Type

    public enum EnemyType
    {
        None,
        Ghost,
        Bird
    }

    class EnemyBase : CharacterBase
    {
        //--------------------------------------------------
        // Combat system

        protected EnemyType _enemyType;
        public EnemyType EnemyType => _enemyType;

        protected bool _active;
        public bool Active => _active;

        //--------------------------------------------------
        // Crystal Position X

        private float _crystalPositionX;

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

        public EnemyBase(Texture2D texture) : base(texture)
        {
            _active = true;
            _enemyType = EnemyType.None;
            _crystalPositionX = SceneManager.Instance.VirtualSize.X / 2;
            
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
                Emitters = new[]
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

        public void SetPositionFromGround(Vector2 position)
        {
            Position = new Vector2(position.X - CharacterSprite.Collider.Width / 2,
                position.Y - CharacterSprite.Collider.Height * 2);
        }

        public void OnAttack()
        {
            _exploding = true;
            _active = false;
            _explosionParticleEffect.Trigger(BoundingRectangle.Center.ToVector2());
            CharacterSprite.Alpha = 0.0f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            UpdateMovement();
            CharacterSprite.Effect = _velocity.X > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (_exploding)
            {
                var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _explosionParticleEffect.Update(deltaTime);

                _explodingEraseInterval += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                _requestErase = _explodingEraseInterval >= ExplodingEraseMaxInterval;
                return;
            }
        }

        protected override void UpdateSprite(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 3;

            _floatPosition.Y = (float)MathUtils.SinInterpolation(-2, 2, deltaTime);
            CharacterSprite.Offset = _floatPosition;

            base.UpdateSprite(gameTime);
        }

        private void UpdateMovement()
        {
            _movement = Math.Sign(_crystalPositionX - _position.X);
        }

        public override void DrawCharacter(SpriteBatch spriteBatch)
        {
            base.DrawCharacter(spriteBatch);
            spriteBatch.Draw(_explosionParticleEffect);
        }
    }
}
