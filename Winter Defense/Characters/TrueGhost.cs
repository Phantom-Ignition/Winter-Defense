using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Winter_Defense.Extensions;

namespace Winter_Defense.Characters
{
    class TrueGhost : EnemyBase
    {
        //--------------------------------------------------
        // Floating position

        private Vector2 _floatPosition;

        //----------------------//------------------------//

        public TrueGhost(Texture2D texture) : base(texture)
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
        }

        protected override void UpdateSprite(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.TotalGameTime.TotalMilliseconds / 3;
            _floatPosition.Y = (float)MathUtils.SinInterpolation(-2, 2, deltaTime);
            CharacterSprite.Offset = _floatPosition;
            base.UpdateSprite(gameTime);
        }

        protected override float MaxMoveSpeed()
        {
            return 35f;
        }
    }
}
