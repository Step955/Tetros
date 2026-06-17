using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tetros.engine
{
    public class GameDefaultStruct
    {
        virtual public void Unload() { }
        virtual public void Initialize(GraphicsDevice graphicsDevice) { }
        virtual public void Load(ContentManager content) { }
        virtual public void LoadTexture(Texture2D butTexture, Rectangle TextureRect) { }
        virtual public void Update(GameTime gameTime) { }
        virtual public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime) { }
        virtual public void OpenCloseCams() { }
        virtual public void EnableGUI() { }
        virtual public void DisableGUI() { }
    }
}
