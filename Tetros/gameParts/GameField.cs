using Android.Media.Metrics;
using Android.Service.Voice;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tetros.engine;
using Tetros.gameStates;

namespace Tetros.gameParts
{
    internal class GameField: GameDefaultStruct
    {
        public static byte[,] field = new byte[16, 10];
        private static Bricks.Brick activeBrick;
        public static Bricks.Brick nextBrick;

        private RenderTarget2D _target;
        private SpriteBatch _spriteBatch;
        private Texture2D _blueTexture, _yellowTexture, _greenTexture, _redTexture, _orangeTexture, _purpleTexture;

        private Texture2D _wood1, _wood2, _wood3, _wood4, _wood5, _wood6, _wood7;

        public GameField()
        {
            /*
            Random rand = new Random();
            
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    field[i, j] = (byte)(rand.Next(5)+1);
                }
            }
            */

            //field[6, 4] = (byte)(rand.Next(5) + 1);
            
        }
        
        
        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            base.Initialize(graphicsDevice);

            activeBrick = Bricks.BrickFactory.GetNewBrick();
            nextBrick = Bricks.BrickFactory.GetNewBrick();
            activeBrick.start();

            _target = new RenderTarget2D(graphicsDevice, 1000, 1600);
            _spriteBatch = new SpriteBatch(graphicsDevice);
            field = new byte[16, 10];

            /*
            _blueTexture = new Texture2D(graphicsDevice, 1, 1);
            _blueTexture.SetData(new[] { Color.Blue });
            _yellowTexture = new Texture2D(graphicsDevice, 1, 1);
            _yellowTexture.SetData(new[] { Color.Yellow });
            _greenTexture = new Texture2D(graphicsDevice, 1, 1);
            _greenTexture.SetData(new[] { Color.Green });
            _redTexture = new Texture2D(graphicsDevice, 1, 1);
            _redTexture.SetData(new[] { Color.Red });
            _orangeTexture = new Texture2D(graphicsDevice, 1, 1);
            _orangeTexture.SetData(new[] { Color.Orange });
            _purpleTexture = new Texture2D(graphicsDevice, 1, 1);
            _purpleTexture.SetData(new[] { Color.Purple });
            */
        }
        
        
        public override void Load(ContentManager content)
        {
            _wood1 = content.Load<Texture2D>("woodBricks/wood1");
            _wood2 = content.Load<Texture2D>("woodBricks/wood2");
            _wood3 = content.Load<Texture2D>("woodBricks/wood3");
            _wood4 = content.Load<Texture2D>("woodBricks/wood4");
            _wood5 = content.Load<Texture2D>("woodBricks/wood5");
            _wood6 = content.Load<Texture2D>("woodBricks/wood6");
            _wood7 = content.Load<Texture2D>("woodBricks/wood7");
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int a = 0; a < field.GetLength(1); a++)
            {
                if(field[0,a] != 0)
                {
                    Game1._gameStateManager.ChangeGameState(new GameOver());
                }
            }



            for (int i = 0; i < field.GetLength(0); i++)
            {
                bool fullLine = true;
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == 0)
                    {
                        fullLine = false;
                        break;
                    }
                }
                if (fullLine)
                {
                    Game1.score += 10;
                    for (int k = i; k > 0; k--)
                    {
                        for (int l = 0; l < field.GetLength(1); l++)
                        {
                            field[k, l] = field[k - 1, l];
                        }
                    }
                    for (int l = 0; l < field.GetLength(1); l++)
                    {
                        field[0, l] = 0;
                    }
                }

            }
        }
        public static void NewBrick()
        {
            field = activeBrick.Place(field);
            activeBrick = nextBrick;
            nextBrick = Bricks.BrickFactory.GetNewBrick();
            activeBrick.start();
        }

        public override void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            base.Draw(graphicsDevice, gameTime);

            graphicsDevice.SetRenderTarget(_target);
            _spriteBatch.Begin();

            Texture2D texture;

            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] != 0)
                    {
                        Rectangle rectangle = new Rectangle(j * 100, i * 100, 100, 100);

                        /*
                        Texture2D texture = field[i, j] switch
                        {
                            1 => _yellowTexture,
                            2 => _greenTexture,
                            3 => _redTexture,
                            4 => _orangeTexture,
                            5 => _purpleTexture,
                            7 => _blueTexture,
                            _ => null
                        };
                        */
                        
                        texture = field[i, j] switch
                        {
                            1 => _wood1,
                            2 => _wood2,
                            3 => _wood3,
                            4 => _wood4,
                            5 => _wood5,
                            6 => _wood6,
                            7 => _wood7,
                            _ => null
                        };
                        

                        if (texture != null)
                            _spriteBatch.Draw(texture, rectangle, Color.White);
                    }
                }
            }

            texture = activeBrick.TextureIndex switch
            {
                1 => _wood1,
                2 => _wood2,
                3 => _wood3,
                4 => _wood4,
                5 => _wood5,
                6 => _wood6,
                7 => _wood7,
                _ => null
            };

            activeBrick.Render(_spriteBatch, texture);

            _spriteBatch.End();
            graphicsDevice.SetRenderTarget(Game1._Screen);

            _spriteBatch.Begin();
            //Rectangle rectangle = new Rectangle(0, 0, 1000, 1600);
            _spriteBatch.Draw(_target, new Rectangle(40, 90, 1000, 1600), Color.White);
            
            // Render the next brick preview
            Texture2D nextTexture = nextBrick.TextureIndex switch
            {
                1 => _wood1,
                2 => _wood2,
                3 => _wood3,
                4 => _wood4,
                5 => _wood5,
                6 => _wood6,
                7 => _wood7,
                _ => null
            };
            nextBrick.RenderPreview(_spriteBatch, nextTexture, new Vector2(515, 2100));

            _spriteBatch.End();
        }

        public override void Unload()
        {
            activeBrick.unload();
        }

        public void moveLeft(object sender, EventArgs e)
        {
            activeBrick.Move(new Vector2(-1, 0));
        }
        public void moveRight(object sender, EventArgs e)
        {
            activeBrick.Move(new Vector2(1, 0));
        }
        public void rotateLeft(object sender, EventArgs e)
        {
            activeBrick.Rotate(false);
        }
        public void rotateRight(object sender, EventArgs e)
        {
            activeBrick.Rotate(true);
        }

    }
}
