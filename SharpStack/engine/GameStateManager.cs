using Tetros.menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata;
using Tetros.gameStates;

namespace Tetros.engine
{
    public class GameStateManager
    {
        public GameDefaultStruct currentState;

        private GraphicsDevice _graphicsDevice;
        private ContentManager _content;

        public GameStateManager() {

            currentState = new MainMenu();

        }

        public void ChangeGameState(GameDefaultStruct newState)
        {
            currentState.Unload();
            currentState = newState;
            currentState.Initialize(_graphicsDevice);
            currentState.Load(_content);
        }

        public void LoadContent(ContentManager content)
        {
            _content = content;
            currentState.Load(content);
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            currentState.Initialize(graphicsDevice);
        }

        public void Update(GameTime gameTime)
        {
            currentState.Update(gameTime);
        }

        public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            currentState.Draw(graphicsDevice, gameTime);
        }
    }
}
