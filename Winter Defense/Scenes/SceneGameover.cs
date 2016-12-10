using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ViewportAdapters;
using Winter_Defense.Managers;

namespace Winter_Defense.Scenes
{
    class SceneGameover : SceneBase
    {
        //--------------------------------------------------
        // Textures

        private Texture2D _backgroundTexture;

        //----------------------//------------------------//

        public override void LoadContent()
        {
            base.LoadContent();

            _backgroundTexture = ImageManager.loadScene("sceneGameover", "Background");

            SoundManager.StartBgm(SoundManager.BGMType.Gameover);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!SceneManager.Instance.IsTransitioning && InputManager.Instace.CurrentKeyState.GetPressedKeys().Length > 0)
            {
                SoundManager.PlayConfirmSe();
                SceneManager.Instance.ChangeScene("SceneTitle");
            }
        }

        public override void Draw(SpriteBatch spriteBatch, ViewportAdapter viewportAdapter)
        {
            base.Draw(spriteBatch, viewportAdapter);

            spriteBatch.Begin(transformMatrix: viewportAdapter.GetScaleMatrix(), samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(_backgroundTexture, Vector2.Zero, Color.White);
            spriteBatch.End();
        }
    }
}
