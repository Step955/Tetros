using Android.Speech.Tts;
using Java.Security.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetros.engine
{
    internal class GUIElements : GameDefaultStruct
    {
        virtual public void Initialize(GraphicsDevice graphicsDevice, EventHandler onEvent, Rectangle buttonBody){}
        virtual public void Initialize(GraphicsDevice graphicsDevice, EventHandler onPressed, EventHandler onHeld, EventHandler onRelease, Rectangle buttonBody) {}
        virtual public void Initialize(GraphicsDevice graphicsDevice, EventHandler onPressed, EventHandler onRelease, Rectangle buttonBody) { }
        virtual public void InitializeTouch(GraphicsDevice graphicsDevice, EventHandler touch, Rectangle buttonBody) { }
        virtual public void InitializeText(string text, Color color, Vector2 poss, SpriteFont font) { }
        virtual public void ChangeText(string newText) { }
        virtual public void Disable() { }
        virtual public void Enable() { }
        virtual public void Load(Texture2D elementTexture) { }

        public class Text : GUIElements
        {
            private string _text;
            private Color _textColor;
            private SpriteFont _textFont;
            private Vector2 _textPossition;
            private SpriteBatch _spriteBatch;
            public override void ChangeText(string newText) => _text = newText;
            public void ChangeColor(Color newColor) => _textColor = newColor;
            public void ChangePossition(Vector2 newPos) => _textPossition = newPos;
            public override void InitializeText(string text, Color color, Vector2 possition, SpriteFont font)
            {
                _text = text;
                _textColor = color;
                _textFont = font;
                _textPossition = possition;
            }

            public override void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
            {
                _spriteBatch ??= new SpriteBatch(graphicsDevice);
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_textFont, _text, _textPossition, _textColor);
                _spriteBatch.End();
            }
        }

        public class TextTopMiddle: Text
        {
            private string _text;
            private Color _textColor;
            private SpriteFont _textFont;
            private Vector2 _textPossition;
            private SpriteBatch _spriteBatch;
            public override void ChangeText(string newText) => _text = newText;
            public void ChangeColor(Color newColor) => _textColor = newColor;
            public void ChangePossition(Vector2 newPos) => _textPossition = newPos;
            public override void InitializeText(string text, Color color, Vector2 possition, SpriteFont font)
            {
                _text = text;
                _textColor = color;
                _textFont = font;
                _textPossition = possition;
            }

            public override void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
            {
                _spriteBatch ??= new SpriteBatch(graphicsDevice);
                _spriteBatch.Begin();
                Vector2 size = _textFont.MeasureString(_text);
                size.Y = 0;
                _spriteBatch.DrawString(_textFont, _text, (_textPossition-(size/2)), _textColor);
                _spriteBatch.End();
            }
        }

        public class ClickButton : GUIElements
        {

            private Rectangle _buttonBody;
            private event EventHandler OnClick;
            public bool _enabled = true;
            override public void Initialize(GraphicsDevice graphicsDevice, EventHandler onCliced, Rectangle buttonBody)
            {
                OnClick += onCliced;
                _buttonBody = buttonBody;

                Game1._userInputManager.BindMouseButton(processInput, UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
            }
            public override void Disable()
            {
                Game1._userInputManager.UnbindMouseButton(processInput, UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
            }
            public override void Enable()
            {
                Game1._userInputManager.BindMouseButton(processInput, UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
            }
            override public void Load(ContentManager content)
            {
            }
            override public void Update(GameTime gameTime)
            {

            }
            override public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
            {
            }
            public void processInput(MouseState mouseState)
            {
                Vector2 clickCords = Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
                if (_buttonBody.Contains(clickCords) && _enabled)
                {
                    OnClick?.Invoke(this, EventArgs.Empty);
                    Debug.WriteLine(Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_CLICK));
                    Debug.WriteLine(Game1._userInputManager.GetOriginCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_CLICK));
                }
            }



        }

        public class HoldButton : GUIElements
        {
            private Rectangle _buttonBody;
            private event EventHandler OnPressed;
            private event EventHandler OnHold;
            private event EventHandler OnRelease;
            override public void Initialize(GraphicsDevice graphicsDevice, EventHandler onPressed,EventHandler onHeld, EventHandler onRelease, Rectangle buttonBody)
            {
                OnPressed += onPressed;
                OnHold += onHeld;
                OnRelease += onRelease;
                _buttonBody = buttonBody;
                Game1._userInputManager.BindMouseButton(processPressedInput, UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
                Game1._userInputManager.BindMouseButton(processHoldInput, UserInputManager.MouseButtons.MOUSE_LEFT_HELD);
                Game1._userInputManager.BindMouseButton(processReleasInput, UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED);
            }

            override public void Initialize(GraphicsDevice graphicsDevice, EventHandler onPressed, EventHandler onRelease, Rectangle buttonBody)
            {
                OnPressed += onPressed;
                OnRelease += onRelease;
                _buttonBody = buttonBody;
                Game1._userInputManager.BindMouseButton(processPressedInput, UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
                Game1._userInputManager.BindMouseButton(processHoldInput, UserInputManager.MouseButtons.MOUSE_LEFT_HELD);
                Game1._userInputManager.BindMouseButton(processReleasInput, UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED);
            }

            override public void Initialize(GraphicsDevice graphicsDevice, EventHandler onHeld, Rectangle buttonBody)
            {
                OnHold += onHeld;
                _buttonBody = buttonBody;
                Game1._userInputManager.BindMouseButton(processHoldInput, UserInputManager.MouseButtons.MOUSE_LEFT_HELD);
                Game1._userInputManager.BindMouseButton(processHoldInput, UserInputManager.MouseButtons.MOUSE_LEFT_HELD);
                Game1._userInputManager.BindMouseButton(processReleasInput, UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED);
            }

            override public void Load(ContentManager content)
            {

            }
            override public void Update(GameTime gameTime)
            {

            }
            override public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
            {

            }
            public void processPressedInput(MouseState mouseState)
            {
                Vector2 clickCords = Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_CLICK);
                if (_buttonBody.Contains(clickCords) && mouseState.LeftButton == ButtonState.Pressed)
                {
                    OnPressed?.Invoke(this, EventArgs.Empty);
                    Debug.WriteLine(Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_CLICK));
                    Debug.WriteLine(Game1._userInputManager.GetOriginCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_CLICK));
                }
            }

            public void processHoldInput(MouseState mouseState)
            {
                Vector2 clickCords = Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_HELD);
                if (_buttonBody.Contains(clickCords) && mouseState.LeftButton == ButtonState.Pressed)
                {
                    OnHold?.Invoke(this, EventArgs.Empty);
                    Debug.WriteLine(Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_HELD));
                    Debug.WriteLine(Game1._userInputManager.GetOriginCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_HELD));
                }
                else
                {
                    OnRelease?.Invoke(this, EventArgs.Empty);
                    Debug.WriteLine(Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED));
                    Debug.WriteLine(Game1._userInputManager.GetOriginCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED));
                }
            }

            public void processReleasInput(MouseState mouseState)
            {
                Vector2 clickCords = Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED);
                OnRelease?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine(Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED));
                Debug.WriteLine(Game1._userInputManager.GetOriginCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED));

            }
        }

        public class ReleasButton : GUIElements
        {
            private Rectangle _buttonBody;
            private event EventHandler OnRelease;
            override public void Initialize(GraphicsDevice graphicsDevice, EventHandler onRelease, Rectangle buttonBody)
            {
                OnRelease += onRelease;
                _buttonBody = buttonBody;
                Game1._userInputManager.BindMouseButton(processReleasInput, UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED);
            }

            override public void Load(ContentManager content)
            {

            }
            override public void Update(GameTime gameTime)
            {

            }
            override public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
            {

            }

            public void processReleasInput(MouseState mouseState)
            {
                Vector2 clickCords = Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED);
                OnRelease?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine(Game1._userInputManager.GetScaledCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED));
                Debug.WriteLine(Game1._userInputManager.GetOriginCursoreCords(UserInputManager.MouseButtons.MOUSE_LEFT_RELEASED));

            }
        }

        public class TouchClickButton : GUIElements
        {
            private Rectangle _buttonBody;
            private Texture2D _buttonTexture;
            private Rectangle _buttonTextureRect;
            private event EventHandler _onPressed;
            private bool _isPressed = false;
            private SpriteBatch _spriteBatch;

            public override void InitializeTouch(GraphicsDevice graphicsDevice, EventHandler touch, Rectangle buttonBody)
            {
                _buttonBody = buttonBody;
                _onPressed = touch;
                Game1._userInputManager.BindTouch(processInput);
            }
            public override void Disable()
            {
                Game1._userInputManager.UnbindTouch(processInput);
            }
            override public void LoadTexture(Texture2D butTexture, Rectangle TextureRect)
            {
                _buttonTexture = butTexture;
                _buttonTextureRect = TextureRect;
            }
            override public void Update(GameTime gameTime)
            {

            }
            override public void Draw(GraphicsDevice graphicsDevice, GameTime gameTime)
            {
                _spriteBatch ??= new SpriteBatch(graphicsDevice);
                _spriteBatch.Begin();
                _spriteBatch.Draw(_buttonTexture, _buttonBody, _buttonTextureRect, Color.White);
                _spriteBatch.End();
            }



            public void processInput(TouchCollection newTouches, TouchCollection oldTouches)
            {
                foreach (TouchLocation newTouchLocation in newTouches)
                {
                    if (_buttonBody.Contains(Game1.Instance.ScreenToRenderTarget(newTouchLocation.Position)))
                    {
                        if (oldTouches.Contains(newTouchLocation) || _isPressed)
                        {
                            return;
                        }
                        else
                        {
                            _isPressed = true;
                            _onPressed?.Invoke(this, EventArgs.Empty);
                            return;
                        }

                    }
                }
                _isPressed = false; 
            }
        }
    }
}
