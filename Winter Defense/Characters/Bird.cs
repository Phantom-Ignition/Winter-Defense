using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Winter_Defense.Characters
{
    class Bird : EnemyBase
    {
        public Bird(Texture2D texture) : base(texture)
        {
            // Stand
            CharacterSprite.CreateFrameList("stand", 200);
            CharacterSprite.AddCollider("stand", new Rectangle(15, 3, 36, 30));
            CharacterSprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 32),
                new Rectangle(64, 0, 64, 32),
                new Rectangle(128, 0, 64, 32)
            });

            // Jumping
            CharacterSprite.CreateFrameList("jumping", 200);
            CharacterSprite.AddCollider("jumping", new Rectangle(15, 3, 36, 30));
            CharacterSprite.AddFrames("jumping", new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 32),
                new Rectangle(64, 0, 64, 32),
                new Rectangle(128, 0, 64, 32)
            });

            // Walking
            CharacterSprite.CreateFrameList("walking", 200);
            CharacterSprite.AddCollider("walking", new Rectangle(15, 3, 36, 30));
            CharacterSprite.AddFrames("walking", new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 32),
                new Rectangle(64, 0, 64, 32),
                new Rectangle(128, 0, 64, 32)
            });
        }

        protected override float MaxMoveSpeed()
        {
            return 180.0f;
        }
    }
}
