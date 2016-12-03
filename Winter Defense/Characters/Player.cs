using System;
using System.Collections.Generic;

using Winter_Defense.Managers;
using Winter_Defense.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Winter_Defense.Characters
{
    class Player : CharacterBase
    {
        //--------------------------------------------------
        // Attacks constants

        private const int NormalAttack = 0;
        private const int AerialAttack = 1;

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
        // Keys locked (no movement)

        private bool _keysLocked;

        //----------------------//------------------------//

        public Player(Texture2D texture) : base(texture)
        {
            // Stand
            CharacterSprite.CreateFrameList("stand", 120);
            CharacterSprite.AddCollider("stand", new Rectangle(0, 0, 32, 32));
            CharacterSprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 32)
            });

            // Walking
            CharacterSprite.CreateFrameList("walking", 120);
            CharacterSprite.AddCollider("walking", new Rectangle(0, 0, 32, 32));
            CharacterSprite.AddFrames("walking", new List<Rectangle>()
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(64, 0, 32, 32),
                new Rectangle(96, 0, 32, 32),
                new Rectangle(128, 0, 32, 32),
            });

            // Jumping up
            CharacterSprite.CreateFrameList("jumping", 60, false);
            CharacterSprite.AddCollider("jumping", new Rectangle(0, 0, 32, 32));
            CharacterSprite.AddFrames("jumping", new List<Rectangle>()
            {
                new Rectangle(0, 128, 32, 32),
                new Rectangle(32, 128, 32, 32),
            });
            
            // Jumping apex
            CharacterSprite.CreateFrameList("jumping_apex", 0, false);
            CharacterSprite.AddCollider("jumping_apex", new Rectangle(0, 0, 32, 32));
            CharacterSprite.AddFrames("jumping_apex", new List<Rectangle>()
            {
                new Rectangle(64, 128, 32, 32),
                new Rectangle(96, 128, 32, 32),
            });

            // Jumping falling
            CharacterSprite.CreateFrameList("jumping_impact", 40, false);
            CharacterSprite.AddCollider("jumping_impact", new Rectangle(0, 0, 32, 32));
            CharacterSprite.AddFrames("jumping_impact", new List<Rectangle>()
            {
                new Rectangle(128, 128, 32, 32),
                new Rectangle(160, 128, 32, 32),
            });

            // Recharging
            CharacterSprite.CreateFrameList("recharging", 120);
            CharacterSprite.AddCollider("recharging", new Rectangle(0, 0, 32, 32));
            CharacterSprite.AddFrames("recharging", new List<Rectangle>()
            {
                new Rectangle(0, 64, 64, 32),
                new Rectangle(64, 64, 32, 32),
                new Rectangle(96, 64, 64, 32),
                new Rectangle(160, 64, 32, 32)
            }, new int[] { 0, 0, -32, 0 }, new int[] { 0, 0, 0, 0 });

            SetupBottomSprite(texture);

            Position = new Vector2(32, 160);

            _keysLocked = false;
        }

        private void SetupBottomSprite(Texture2D texture)
        {
            // Stand
            _bottomSprite = new CharacterSprite(texture);
            _bottomSprite.CreateFrameList("stand", 120);
            _bottomSprite.AddCollider("stand", new Rectangle(0, 0, 32, 32));
            _bottomSprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(160, 0, 32, 32)
            });

            // Walking
            _bottomSprite.CreateFrameList("walking", 120);
            _bottomSprite.AddCollider("walking", new Rectangle(0, 0, 32, 32));
            _bottomSprite.AddFrames("walking", new List<Rectangle>()
            {
                new Rectangle(0, 160, 32, 32),
                new Rectangle(32, 160, 32, 32),
                new Rectangle(64, 160, 32, 32),
                new Rectangle(96, 160, 32, 32)
            });

            // Jumping up
            _bottomSprite.CreateFrameList("jumping", 60, false);
            _bottomSprite.AddCollider("jumping", new Rectangle(0, 0, 32, 32));
            _bottomSprite.AddFrames("jumping", new List<Rectangle>()
            {
                new Rectangle(0, 288, 32, 32),
                new Rectangle(32, 288, 32, 32),
            });

            // Jumping apex
            _bottomSprite.CreateFrameList("jumping_apex", 0, false);
            _bottomSprite.AddCollider("jumping_apex", new Rectangle(0, 0, 32, 32));
            _bottomSprite.AddFrames("jumping_apex", new List<Rectangle>()
            {
                new Rectangle(64, 289, 32, 32),
                new Rectangle(96, 289, 32, 32),
            });

            // Jumping falling
            _bottomSprite.CreateFrameList("jumping_impact", 40, false);
            _bottomSprite.AddCollider("jumping_impact", new Rectangle(0, 0, 32, 32));
            _bottomSprite.AddFrames("jumping_impact", new List<Rectangle>()
            {
                new Rectangle(128, 288, 32, 32),
                new Rectangle(160, 288, 32, 32),
            });

            // Recharging
            _bottomSprite.CreateFrameList("recharging", 120);
            _bottomSprite.AddCollider("recharging", new Rectangle(0, 0, 32, 32));
            _bottomSprite.AddFrames("recharging", new List<Rectangle>()
            {
                new Rectangle(0, 225, 32, 32),
                new Rectangle(32, 225, 32, 32),
                new Rectangle(64, 225, 32, 32),
                new Rectangle(96, 225, 32, 32)
            }, new int[] { 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0 });
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
            bool isOnGroundBefore = _isOnGround;
            CheckKeys(gameTime);
            base.Update(gameTime);
            UpdateSprites(gameTime, isOnGroundBefore);
        }

        private void UpdateSprites(GameTime gameTime, bool isOnGroundBefore)
        {
            UpdateBottomSprite(gameTime);
            if (_isOnGround && !isOnGroundBefore && !_groundImpact)
            {
                _groundImpact = true;
                CharacterSprite.SetFrameList("jumping_impact");
                _bottomSprite.SetFrameList("jumping_impact");
            }
            if (_groundImpact && CharacterSprite.Looped)
            {
                _groundImpact = false;
            }
        }

        private void UpdateBottomSprite(GameTime gameTime)
        {
            _bottomSprite.Update(gameTime);
        }

        public override void UpdateFrameList()
        {
            var walking = (InputManager.Instace.KeyDown(Keys.Left) || InputManager.Instace.KeyDown(Keys.Right)) && !_keysLocked;

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
                if (Math.Abs(_velocity.Y) < 100.0f)
                {
                    CharacterSprite.SetFrameList("jumping_apex");
                }
                else
                {
                    CharacterSprite.SetFrameList("jumping");
                }
            }
            else if (walking)
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
            else if (_recharging && !walking)
            {
                _bottomSprite.SetFrameList("recharging");
            }
            else if (_groundImpact && !_recharging)
            {
                _bottomSprite.SetFrameList("jumping_impact");
            }
            else if (walking)
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

            _isJumping = InputManager.Instace.KeyDown(Keys.C);
            _recharging = InputManager.Instace.KeyDown(Keys.R);
        }

        public override void DrawCharacter(SpriteBatch spriteBatch)
        {
            _bottomSprite.Draw(spriteBatch, new Vector2(BoundingRectangle.X, BoundingRectangle.Y));
            base.DrawCharacter(spriteBatch);
        }
    }
}
