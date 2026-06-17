using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tetros.engine
{
    public class UserInputManager
    {
        private MouseState _LastMouseState;

        private readonly Dictionary<MouseButtons, Action<MouseState>> _mouseActions = new();
        private readonly Dictionary<MouseButtons, Vector2> _mouseOriginalCords = new();
        private readonly Dictionary<MouseButtons, Vector2> _mouseScaledCords = new();

        private KeyboardState _PreviousKeyboardState;
        private KeyboardState _CurrentKeyboardState;

        private readonly Dictionary<Keys, Action> _onPressed = new();
        private readonly Dictionary<Keys, Action> _onHeld = new();
        private readonly Dictionary<Keys, Action> _onReleased = new();

        private Action<Keys> _anyPressed;
        private Action<Keys> _anyHeld;
        private Action<Keys> _anyReleased;

        private TouchCollection _LastTouches;
        private Action<TouchCollection, TouchCollection> _touchEvents;
      

        public UserInputManager()
        {
            foreach (var button in Enum.GetValues<MouseButtons>())
            {
                _mouseOriginalCords[button] = Vector2.Zero;
                _mouseScaledCords[button] = Vector2.Zero;
            }
        }

        public void HendleInputs(KeyboardState keyboardState, MouseState mouseState, TouchCollection touches)
        {
            _touchEvents?.Invoke(touches, _LastTouches);
            Parallel.Invoke(
                () => HandleMouseInputs(mouseState),
                () => HandleKeyboardInputs(keyboardState)
            );
        }

        private void HandleMouseInputs(MouseState mouseState)
        {
            var pos = new Vector2(mouseState.X, mouseState.Y);
            var scaledPos = Game1.Instance.ScreenToRenderTargetPoint(new Point(mouseState.X, mouseState.Y));

            void TryInvoke(MouseButtons click, MouseButtons held, MouseButtons released, ButtonState current, ButtonState last)
            {
                if (current == ButtonState.Pressed && last == ButtonState.Released)
                    InvokeMouseAction(click, pos, scaledPos, mouseState);

                if (current == ButtonState.Pressed)
                    InvokeMouseAction(held, pos, scaledPos, mouseState);

                if (current == ButtonState.Released && last == ButtonState.Pressed)
                    InvokeMouseAction(released, pos, scaledPos, mouseState);
            }

            Parallel.Invoke(
                () => TryInvoke(MouseButtons.MOUSE_LEFT_CLICK, MouseButtons.MOUSE_LEFT_HELD, MouseButtons.MOUSE_LEFT_RELEASED,
                                mouseState.LeftButton, _LastMouseState.LeftButton),
                () => TryInvoke(MouseButtons.MOUSE_RIGHT_CLICK, MouseButtons.MOUSE_RIGHT_HELD, MouseButtons.MOUSE_RIGHT_RELEASED,
                                mouseState.RightButton, _LastMouseState.RightButton),
                () => TryInvoke(MouseButtons.MOUSE_MIDDLE_CLICK, MouseButtons.MOUSE_MIDDLE_HELD, MouseButtons.MOUSE_MIDDLE_RELEASED,
                                mouseState.MiddleButton, _LastMouseState.MiddleButton)
            );

            _LastMouseState = mouseState;
        }

        private void InvokeMouseAction(MouseButtons button, Vector2 pos, Vector2 scaledPos, MouseState mouseState)
        {
            if (_mouseActions.TryGetValue(button, out Action<MouseState> handler))
            {
                _mouseOriginalCords[button] = pos;
                _mouseScaledCords[button] = scaledPos;
                handler?.Invoke(mouseState);
            }
        }

        private void HandleKeyboardInputs(KeyboardState keyboardState)
        {
            _PreviousKeyboardState = _CurrentKeyboardState;
            _CurrentKeyboardState = keyboardState;

            Parallel.Invoke(
                () =>
                {
                    // Handle key presses
                    foreach (var pair in _onPressed)
                    {
                        if (_PreviousKeyboardState.IsKeyUp(pair.Key) && _CurrentKeyboardState.IsKeyDown(pair.Key))
                        {
                            pair.Value?.Invoke();
                            _anyPressed?.Invoke(pair.Key);
                        }
                    }
                },
                () =>
                {
                    // Handle key holds
                    foreach (var pair in _onHeld)
                    {
                        if (_CurrentKeyboardState.IsKeyDown(pair.Key))
                        {
                            pair.Value?.Invoke();
                            _anyHeld?.Invoke(pair.Key);
                        }
                    }
                },
                () =>
                {
                    // Handle key releases
                    foreach (var pair in _onReleased)
                    {
                        if (_PreviousKeyboardState.IsKeyDown(pair.Key) && _CurrentKeyboardState.IsKeyUp(pair.Key))
                        {
                            pair.Value?.Invoke();
                            _anyReleased?.Invoke(pair.Key);
                        }
                    }
                }
            );
        }

        public Vector2 GetOriginCursoreCords(MouseButtons button) {
            return _mouseOriginalCords[button];

        }
        public Vector2 GetScaledCursoreCords(MouseButtons button) {

            return _mouseScaledCords[button];
        }

        //Bind and unbind methods

        // Mouse Bindings
        public void BindMouseButton(Action<MouseState> handler, MouseButtons key) => AddToMouseDictionary(_mouseActions, key, handler);
        public void UnbindMouseButton(Action<MouseState> handler, MouseButtons key) => RemoveFromMouseDictionary(_mouseActions, key, handler);

        //Touch Bindings

        public void BindTouch(Action<TouchCollection, TouchCollection> handler) => _touchEvents += handler;
        public void UnbindTouch(Action<TouchCollection, TouchCollection> handler) => _touchEvents -= handler;

        // Keyboard Bindings
        //specific key
        public void BindPressdKey(Keys key, Action handler) => AddToDictionary(_onPressed, key, handler);
        public void BindHeldKey(Keys key, Action handler) => AddToDictionary(_onHeld, key, handler);
        public void BindReleasedKey(Keys key, Action handler) => AddToDictionary(_onReleased, key, handler);


        public void UnbindPressedKey(Keys key, Action handler) => RemoveFromdictionary(_onPressed, key, handler);
        public void UnbindHeldKey(Keys key, Action handler) => RemoveFromdictionary(_onHeld, key, handler);
        public void UnbindReleasedKey(Keys key, Action handler) => RemoveFromdictionary(_onReleased, key, handler);

        //any key
        public void BindAnyPressedKey(Action<Keys> handler) => _anyPressed += handler;
        public void BindAnyHeldKey(Action<Keys> handler) => _anyHeld += handler;
        public void BindAnyReleasedKey(Action<Keys> handler) => _anyReleased += handler;


        public void UnbindAnyPressedKey(Action<Keys> handler) => _anyPressed -= handler;
        public void UnbindAnyHeldKey(Action<Keys> handler) => _anyHeld -= handler;
        public void UnbindAnyReleasedKey(Action<Keys> handler) => _anyReleased -= handler;



        // Mouse event binding helper methods
        private static void AddToMouseDictionary(Dictionary<MouseButtons, Action<MouseState>> dict, MouseButtons key, Action<MouseState> handler)
        {
            if (dict.TryGetValue(key, out Action<MouseState> existingHandler))
                dict[key] = existingHandler + handler;
            else
                dict[key] = handler;
        }

        private static void RemoveFromMouseDictionary(Dictionary<MouseButtons, Action<MouseState>> dict, MouseButtons key, Action<MouseState> handler)
        {
            if (!dict.TryGetValue(key, out Action<MouseState> existingHandler))
                return;
            existingHandler -= handler;
            if (existingHandler == null)
                dict.Remove(key);
            else
                dict[key] = existingHandler;
        }

        // Keyboard event binding helper methods
        private static void AddToDictionary(Dictionary<Keys, Action> dict, Keys key, Action handler)
        {
            if (dict.TryGetValue(key, out Action existingHandler))
                dict[key] = existingHandler + handler;
            else
                dict[key] = handler;
        }

        private static void RemoveFromdictionary(Dictionary<Keys, Action> dict, Keys key, Action handler)
        {
            if (!dict.TryGetValue(key, out Action existingHandler))
                return;

            existingHandler -= handler;
            if (existingHandler == null)
                dict.Remove(key);
            else
                dict[key] = existingHandler;
        }


        // Enums for mouse states
        public enum MouseButtons
        {
            MOUSE_LEFT_CLICK,
            MOUSE_RIGHT_CLICK,
            MOUSE_MIDDLE_CLICK,

            MOUSE_LEFT_HELD,
            MOUSE_RIGHT_HELD,
            MOUSE_MIDDLE_HELD,

            MOUSE_LEFT_RELEASED,
            MOUSE_RIGHT_RELEASED,
            MOUSE_MIDDLE_RELEASED
        }
    }
}