using Tetros.engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Tetros
{
    public class Game1 : Game
    {
        public static Game1 Instance { get; private set; }

        private float scale;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static GameStateManager _gameStateManager = new GameStateManager();
        public static TimeEventsManager _timeEvents = new TimeEventsManager();
        public static UserInputManager _userInputManager;

        public static RenderTarget2D _Screen;

        public static int score = 0;

        private enum ScaleMode
        {
            StretchToFill,
            PreserveAspectFit
        }

        private ScaleMode _scaleMode = ScaleMode.PreserveAspectFit;
        public Game1()
        {
            Instance = this;

            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);

        }


        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _Screen = new RenderTarget2D(GraphicsDevice, 1080, 2400, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            _userInputManager = new UserInputManager();
            _gameStateManager.Initialize(GraphicsDevice);
            _timeEvents.AddTimeEvent(10.0f, test);

            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameStateManager.LoadContent(Content);
            // TODO: use this.Content to load your game content here
            /*
            // Loading a Song using the content pipeline
            Song song = Content.Load<Song>("Undertale- Megalovania");

            // Set whether the song should repeat when finished
            MediaPlayer.IsRepeating = true;

            // Adjust the volume (0.0f to 1.0f)
            MediaPlayer.Volume = 0.5f;

            // Check if the media player is already playing, if so, stop it
            if (MediaPlayer.State == MediaState.Playing)
            {
                MediaPlayer.Stop();
            }

            // Start playing the background music
            MediaPlayer.Play(song);
            */
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            _userInputManager.HendleInputs(keyboardState, mouseState, TouchPanel.GetState());

            _timeEvents.Update(gameTime);


            // TODO: Add your update logic here
            _gameStateManager.Update(gameTime);

            base.Update(gameTime);
        }

        void test()
        {
            Debug.WriteLine("Test function called!");
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_Screen);
            GraphicsDevice.Clear(new Color(50,50,50));

            _gameStateManager.Draw(GraphicsDevice, gameTime);

            //Final draw to screen

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(new Color(20, 20, 20));

            // compute destination rectangle depending on chosen scale mode
            int backW = GraphicsDevice.Viewport.Width;
            int backH = GraphicsDevice.Viewport.Height;
            Rectangle destRect;

            if (_scaleMode == ScaleMode.StretchToFill)
            {
                // Stretch to fill full backbuffer (may change aspect ratio)
                destRect = new Rectangle(0, 0, backW, backH);
            }
            else
            {
                // Preserve aspect ratio and center (letterbox / pillarbox)
                scale = Math.Min(backW / (float)_Screen.Width, backH / (float)_Screen.Height);
                int drawW = (int)(_Screen.Width * scale);
                int drawH = (int)(_Screen.Height * scale);
                destRect = new Rectangle((backW - drawW) / 2, (backH - drawH) / 2, drawW, drawH);
            }

            // Use PointClamp for pixel-perfect scaling (or LinearClamp for smooth)
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_Screen, destRect, Color.White);
            _spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        public Vector2 ScreenToRenderTarget(Vector2 screenPosition)
        {
            if (_Screen == null) return screenPosition;

            int backW = GraphicsDevice.Viewport.Width;
            int backH = GraphicsDevice.Viewport.Height;

            if (_scaleMode == ScaleMode.StretchToFill)
            {
                // mapujeme přímo podle poměrů šířek/výšek
                float sx = _Screen.Width / (float)backW;
                float sy = _Screen.Height / (float)backH;
                return new Vector2(screenPosition.X * sx, screenPosition.Y * sy);
            }
            else
            {
                // zachování poměru stran + vystředění (letterbox/pillarbox)
                float s = Math.Min(backW / (float)_Screen.Width, backH / (float)_Screen.Height);
                float drawW = _Screen.Width * s;
                float drawH = _Screen.Height * s;
                float offsetX = (backW - drawW) / 2f;
                float offsetY = (backH - drawH) / 2f;

                // převedeme pozici do lokálních souřadnic vykresleného render targetu a pak do souřadnic render targetu
                float localX = (screenPosition.X - offsetX) / s;
                float localY = (screenPosition.Y - offsetY) / s;

                return new Vector2(localX, localY);
            }
        }

        public Vector2 ScreenToRenderTargetPoint(Point screenPoint)
        {
            Vector2 v = ScreenToRenderTarget(new Vector2(screenPoint.X, screenPoint.Y));
            return new Vector2((int)Math.Floor(v.X), (int)Math.Floor(v.Y));
        }
    }
}
