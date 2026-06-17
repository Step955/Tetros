using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Tetros.gameParts
{
    public class Bricks
    {
        public abstract class Brick
        {
            Vector2 Position = new Vector2(5, -5);
            public List<Vector2> Blocks { get; protected set; }

            public byte TextureIndex = 1;
            //Color Color = Color.White;

            public Brick()
            {
                Random rand = new Random();
                TextureIndex = (byte)rand.Next(1, 8);
            }

            public void start()
            {
                Game1._timeEvents.AddTimeEvent(0.25f, Gravity);
            }

            public void Render(SpriteBatch spriteBatch, Texture2D texture)
            {
                foreach (Vector2 b in Blocks) {
                    spriteBatch.Draw(texture, new Rectangle(((Position + b) * 100).ToPoint(), new Point(100, 100)), Color.White);
                }
            }

            public void RenderPreview(SpriteBatch spriteBatch, Texture2D texture, Vector2 previewPos)
            {
                foreach (Vector2 b in Blocks)
                {
                    spriteBatch.Draw(texture, new Rectangle((((new Vector2(0,0) + b) * 50)+previewPos).ToPoint(), new Point(50, 50)), Color.White);
                }
            }


            public bool CheckValidMovement(Vector2 move)
            {
                Vector2 newPos = Position + move;

                foreach (Vector2 b in Blocks)
                {
                    Vector2 blockPos = newPos + b;

                    // Always check horizontal walls, even above the field
                    if (blockPos.X < 0 || blockPos.X >= 10)
                        return false;

                    // Block is still above the field — skip array access
                    if (blockPos.Y < 0)
                        continue;

                    // Hit the bottom
                    if (blockPos.Y >= 16)
                        return false;

                    // Cell already occupied
                    if (GameField.field[(int)blockPos.Y, (int)blockPos.X] != 0)
                        return false;
                }

                return true;
            }
            

            public void Gravity()
            {
                if (CheckValidMovement(new Vector2(0, 1)))
                {
                    Position.Y += 1;
                }
                else
                {
                    Game1._timeEvents.RemoveTimeEvent(Gravity);
                    GameField.NewBrick();
                }
            }

            public byte[,] Place(byte[,] field)
            {
                byte[,] newField = (byte[,])field.Clone();

                foreach (Vector2 b in Blocks)
                {
                    Vector2 virtualBlocPos = Position + b;

                    // Add a boundary check before accessing the array
                    if (virtualBlocPos.Y >= 0)
                    {
                        newField[(int)virtualBlocPos.Y, (int)virtualBlocPos.X] = TextureIndex;
                    }
                }
                return newField;
            }

            public void Move(Vector2 move)
            {
                if (CheckValidMovement(move))
                {
                    Position += move;
                }
                else
                {
                    Debug.WriteLine("Invalid move");
                }
            }
            public void Rotate(bool clockwise)
            {
                List<Vector2> newBlocks = new List<Vector2>();
                foreach (Vector2 b in Blocks)
                {
                    if (clockwise)
                    {
                        newBlocks.Add(new Vector2(-b.Y, b.X));
                    }
                    else
                    {
                        newBlocks.Add(new Vector2(b.Y, -b.X));
                    }
                }
                List<Vector2> oldBlocks = Blocks;
                Blocks = newBlocks;
                if (!CheckValidMovement(new Vector2(0, 0)))
                {
                    Blocks = oldBlocks;
                }
            }

            public void unload()
            {
                Game1._timeEvents.RemoveTimeEvent(Gravity);
            }

        }

        public class BrickLine : Brick {
            public BrickLine() : base() {
                Blocks = new List<Vector2> { new Vector2(0, -1), new Vector2(0, 0), new Vector2(0, 1) };
            }
        }

        public class BrickLine2 : Brick {
            public BrickLine2() : base() {
                Blocks = new List<Vector2> { new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0) };
            }
        }

        public class BrickCube : Brick {
            public BrickCube() : base()
            {
                Blocks = new List<Vector2> { new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 0), new Vector2(0, 0) };
            }
        }

        public class BrickT : Brick {
            public BrickT() : base()
            {
                Blocks = new List<Vector2> { new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1) };
            }
        }
        public class BrickT2 : Brick {
            public BrickT2() : base()
            {
                Blocks = new List<Vector2> { new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, -1) };
            }
        }
        public class BrickT3 : Brick {
            public BrickT3() : base()
            {
                Blocks = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, 0) };
            }
        }
        public class BrickT4 : Brick {
            public BrickT4() : base()
            {
                Blocks = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, -1), new Vector2(-1, 0) };
            }
        }

        public class BrickL : Brick {
            public BrickL() : base()
            {
                Blocks = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, 1) };
            }
        }
        public class BrickL2 : Brick {
            public BrickL2() : base()
            {
                Blocks = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, -1), new Vector2(-1, 1) };
            }
        }
        public class BrickL3 : Brick {
            public BrickL3() : base()
            {
                Blocks = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, -1), new Vector2(1, -1) };
            }
        }
        public class BrickL4 : Brick {
            public BrickL4() : base()
            {
                Blocks = new List<Vector2> { new Vector2(0, 1), new Vector2(0, 0), new Vector2(0, -1), new Vector2(-1, -1) };
            }
        }
        public class BrickL5 : Brick {
            public BrickL5() : base()
            {
                Blocks = new List<Vector2> { new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 1) };
            }
        }

        public class BrickL6 : Brick {
            public BrickL6() : base()
            {
                Blocks = new List<Vector2> { new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, 1) };
            }
        }
        public class BrickL7 : Brick {
            public BrickL7() : base()
            {
                Blocks = new List<Vector2> { new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, -1) };
            }
        }
        
        public class BrickL8 : Brick {
            public BrickL8() : base()
            {
                Blocks = new List<Vector2> { new Vector2(1, 0), new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, -1) };
            }
        }

        public static class BrickFactory
        {
            private static readonly Random random = new Random();
            private static readonly Type[] brickTypes =
            {
                typeof(BrickLine), typeof(BrickLine2), 
                typeof(BrickCube), 
                typeof(BrickT), typeof(BrickT2), typeof(BrickT3), typeof(BrickT4), 
                typeof(BrickL), typeof(BrickL2), typeof(BrickL3), typeof(BrickL4), typeof(BrickL5), typeof(BrickL6), typeof(BrickL7), typeof(BrickL8)

            };
            public static Brick GetNewBrick()
            {
                int index = random.Next(brickTypes.Length);
                Brick brick = (Brick)Activator.CreateInstance(brickTypes[index]);
                return brick;
            }
        }
    }
}
