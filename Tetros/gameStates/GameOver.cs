using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetros.engine;
using Tetros.menus;

namespace Tetros.gameStates
{
    internal class GameOver: GameDefaultStruct
    {
        SpriteFont TextFont;
        private Dictionary<string, GUIElements> _GUIElements = new Dictionary<string, GUIElements>();
        Texture2D ButtonTexture;

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            _GUIElements.TryAdd("Button", new GUIElements.TouchClickButton());
            _GUIElements.TryAdd("GOText", new GUIElements.TextTopMiddle());
            _GUIElements.TryAdd("GtMText", new GUIElements.TextTopMiddle());            
            _GUIElements.TryAdd("ScoreText", new GUIElements.TextTopMiddle());            

            _GUIElements["Button"].InitializeTouch(graphicsDevice, (object sender, EventArgs e) => Game1._gameStateManager.ChangeGameState(new GameScreen()), new Rectangle(290, 1600, 500, 200));

            
        }

        public override void Load(ContentManager content)
        {
            ButtonTexture = content.Load<Texture2D>("GoToMenu");
            TextFont = content.Load<SpriteFont>("Font");

            _GUIElements["Button"].LoadTexture(ButtonTexture, new Rectangle(0, 0, 500, 200));

            _GUIElements["GOText"].InitializeText("Game Over", Color.White, new Vector2(540, 300), TextFont);
            _GUIElements["GtMText"].InitializeText("Restart", Color.White, new Vector2(540, 1625), TextFont);
            _GUIElements["ScoreText"].InitializeText("Score: " + Game1.score, Color.White, new Vector2(540, 1200), TextFont);
        }

        public override void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            foreach (var element in _GUIElements.Values) { element.Draw(graphicsDevice, gameTime); }

        }

        public override void Unload()
        {
            foreach (var element in _GUIElements.Values) { element.Disable(); }
        }
    }
}
