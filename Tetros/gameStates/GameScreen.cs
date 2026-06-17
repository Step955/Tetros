using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetros.engine;
using Microsoft.Xna.Framework;
using Java.Lang;
using System.Diagnostics;
using Tetros.gameParts;
using Microsoft.Xna.Framework.Content;
using Java.Util.Functions;

namespace Tetros.menus
{
    internal class GameScreen : GameDefaultStruct
    {
        
        bool buttonState = false;
        Texture2D ButtonsTextures;
        SpriteFont TextFont;
        GameField gameField = new GameField();

        private Dictionary<string, GUIElements> _GUIElements = new Dictionary<string, GUIElements>();

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            gameField.Initialize(graphicsDevice);

            /*
            _GUIElements.TryAdd("testButton", new GUIElements.TouchClickButton());
            _GUIElements["testButton"].InitializeTouch(graphicsDevice, TestButton, new Rectangle(100, 200, 100, 200));
            texture = new Texture2D(graphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Red });
            _GUIElements["testButton"].Load(texture);
            */

            _GUIElements.TryAdd("LeftMoveButton", new GUIElements.TouchClickButton());
            _GUIElements.TryAdd("RightMoveButton", new GUIElements.TouchClickButton());
            _GUIElements.TryAdd("LeftRotateButton", new GUIElements.TouchClickButton());
            _GUIElements.TryAdd("RightRotateButton", new GUIElements.TouchClickButton());

            _GUIElements.TryAdd("ScoreText", new GUIElements.TextTopMiddle());

            _GUIElements["LeftMoveButton"].InitializeTouch(graphicsDevice, gameField.moveLeft, new Rectangle(100, 2048, 200, 200));
            _GUIElements["RightMoveButton"].InitializeTouch(graphicsDevice, gameField.moveRight, new Rectangle(780, 2048, 200, 200));
            _GUIElements["LeftRotateButton"].InitializeTouch(graphicsDevice, gameField.rotateLeft, new Rectangle(100, 1769, 200, 200));
            _GUIElements["RightRotateButton"].InitializeTouch(graphicsDevice, gameField.rotateRight, new Rectangle(780, 1769, 200, 200));

            Game1.score = 0;

        }

        public override void Load(ContentManager content)
        {
            gameField.Load(content);
            ButtonsTextures = content.Load<Texture2D>("controll_buttons");
            TextFont = content.Load<SpriteFont>("Font");

            _GUIElements["LeftMoveButton"].LoadTexture(ButtonsTextures , new Rectangle(0,0,100,100));
            _GUIElements["RightMoveButton"].LoadTexture(ButtonsTextures , new Rectangle(100,0,100,100));
            _GUIElements["LeftRotateButton"].LoadTexture(ButtonsTextures , new Rectangle(200,0,100,100));
            _GUIElements["RightRotateButton"].LoadTexture(ButtonsTextures , new Rectangle(300,0,100,100));

            _GUIElements["ScoreText"].InitializeText(Game1.score.ToString(), Color.White, new Vector2(540, 1800), TextFont);
        }

        public override void Update(GameTime gameTime)
        {
            gameField.Update(gameTime);


        }

        public override void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            gameField.Draw(graphicsDevice, gameTime);
            _GUIElements["ScoreText"].ChangeText(Game1.score.ToString());
            foreach (var element in _GUIElements.Values) { element.Draw(graphicsDevice, gameTime); }
        }

        public override void Unload()
        {
            foreach (var element in _GUIElements.Values) { element.Disable(); }
            gameField.Unload();
        }

    }
}
