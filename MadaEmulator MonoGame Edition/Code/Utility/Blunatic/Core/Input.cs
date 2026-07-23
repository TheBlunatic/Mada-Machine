using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Core
{
    public enum MouseButton
    {
        Left,
        Right,
        Middle,
    }
    public static class Input
    {
        // Classes
        public class KeyState
        {
            // Constants
            public const string LOWERCASE_LETTERS = "abcdefghijklmnopqrstuvwxyz";
            public const string UPPERCASE_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            public const string LETTERS = LOWERCASE_LETTERS + UPPERCASE_LETTERS;
            public const string NUMBERS = "0123456789";
            public const string HEX = NUMBERS + "ABCDEF";
            public const string ALPHANUMERIC = LETTERS + NUMBERS;
            public const string PUNCTUATION = ",./<>?\\|;:'@#~[{]}=+-_)(*&^%$£\"!`¬";
            public const string FILENAME = ALPHANUMERIC + "_.- ";
            public const string COMMON = ALPHANUMERIC + PUNCTUATION + " ";

            // Interfaces
            private interface ITypingAction
            {
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength);
            }

            // Structs
            private struct TypeCharacter : ITypingAction
            {
                private char _char;
                public TypeCharacter(char c)
                {
                    _char = c;
                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = startingCursor;
                    if (allowedCharacters.Contains(_char) && current.Length < maxStringLength)
                    {
                        current = current.Insert(startingCursor, $"{_char}");
                        endingCursor++;
                        return $"{current}";
                    }
                    return current;
                }
            }
            private struct TypeBackspace : ITypingAction
            {
                public TypeBackspace()
                {

                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = startingCursor;
                    if (current.Length > 0 && startingCursor != 0)
                    {
                        endingCursor--;
                        if (startingCursor == current.Length)
                        {
                            return current.Substring(0, current.Length - 1);
                        }
                        return current.Substring(0, startingCursor - 1) + current.Substring(startingCursor);
                    }
                    return current;
                }
            }
            private struct TypeDelete : ITypingAction
            {
                public TypeDelete()
                {

                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = startingCursor;
                    if (current.Length > 0 && startingCursor != current.Length)
                    {
                        if (startingCursor == 0)
                        {
                            return current.Substring(1);
                        }
                        return current.Substring(0, startingCursor) + current.Substring(startingCursor + 1);
                    }
                    return current;
                }
            }
            private struct MoveCursorForwards : ITypingAction
            {
                public MoveCursorForwards()
                {

                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = startingCursor;
                    if (endingCursor != current.Length)
                    {
                        endingCursor++;
                    }
                    return current;
                }
            }
            private struct MoveCursorBackwards : ITypingAction
            {
                public MoveCursorBackwards()
                {

                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = startingCursor;
                    if (endingCursor != 0)
                    {
                        endingCursor--;
                    }
                    return current;
                }
            }
            private struct MoveCursorForwardsFully : ITypingAction
            {
                public MoveCursorForwardsFully()
                {

                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = current.Length;
                    return current;
                }
            }
            private struct MoveCursorBackwardsFully : ITypingAction
            {
                public MoveCursorBackwardsFully()
                {

                }
                public string Act(string current, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
                {
                    endingCursor = 0;
                    return current;
                }
            }

            // Fields
            KeyboardState _currentKeyState;
            KeyboardState _previousKeyState;

            Queue<ITypingAction> _typingBuffer;
            List<ITypingAction> _typingActions;

            // Methods
            public KeyState(MonoGameInstance mgi)
            {
                mgi.Window.TextInput += _queueEvent;
                _previousKeyState = Keyboard.GetState();
                _currentKeyState = _previousKeyState;
                _typingActions = new List<ITypingAction>();
                _typingBuffer = new Queue<ITypingAction>();
            }

            public void Update()
            {
                _previousKeyState = _currentKeyState;
                _currentKeyState = Keyboard.GetState();
                _typingActions.Clear();
                lock (_typingBuffer)
                {
                    if (WasJustPressed(Keys.Left))
                    {
                        if (IsPressed(Keys.LeftControl))
                        {
                            _typingBuffer.Enqueue(new MoveCursorBackwardsFully());
                        }
                        else
                        {
                            _typingBuffer.Enqueue(new MoveCursorBackwards());
                        }
                    }
                    if (WasJustPressed(Keys.Right))
                    {
                        if (IsPressed(Keys.LeftControl))
                        {
                            _typingBuffer.Enqueue(new MoveCursorForwardsFully());
                        }
                        else
                        {
                            _typingBuffer.Enqueue(new MoveCursorForwards());
                        }
                    }
                    while (_typingBuffer.Count > 0)
                    {
                        _typingActions.Add(_typingBuffer.Dequeue());
                    }
                }
            }

            public KeyboardState GetState()
            {
                return _currentKeyState;
            }
            public string ApplyTypingToString(string input, string allowedCharacters, int startingCursor, out int endingCursor, int maxStringLength)
            {
                string output = input;
                foreach (ITypingAction t in _typingActions)
                {
                    output = t.Act(output, allowedCharacters, startingCursor, out startingCursor, maxStringLength);
                }

                endingCursor = startingCursor;
                return output;
            }

            private Controls.Modifier _getModifier(KeyboardState keyboardState)
            {
                if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl)) return Controls.Modifier.Ctrl;
                if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift)) return Controls.Modifier.Shift;
                if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt)) return Controls.Modifier.Alt;
                return Controls.Modifier.None;

            }

            private bool _isMultiControlPressed(KeyboardState keyboardState, params Controls.ControlAssignment[] controlAssignments)
            {
                foreach (Controls.ControlAssignment assignment in controlAssignments)
                {
                    if (keyboardState.IsKeyDown(assignment.Key) && assignment.CheckModifier(_getModifier(keyboardState))) return true;
                }
                return false;
            }
            public bool IsPressed(Keys key)
            {
                return _currentKeyState.IsKeyDown(key);
            }
            public bool IsMultiControlPressed(params Controls.ControlAssignment[] controlAssignments)
            {
                return _isMultiControlPressed(_currentKeyState, controlAssignments);
            }

            public bool WasJustPressed(Keys key)
            {
                return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
            }
            public bool WasMultiControlJustPressed(params Controls.ControlAssignment[] controlAssignments)
            {
                return _isMultiControlPressed(_currentKeyState, controlAssignments) && !_isMultiControlPressed(_previousKeyState, controlAssignments);
            }

            public bool WasJustReleased(Keys key)
            {
                return !_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyDown(key);
            }
            public bool WasMultiControlJustReleased(params Controls.ControlAssignment[] controlAssignments)
            {
                return !_isMultiControlPressed(_currentKeyState, controlAssignments) && _isMultiControlPressed(_previousKeyState, controlAssignments);
            }

            private void _queueEvent(object sender, TextInputEventArgs e)
            {
                switch (e.Key)
                {
                    case Keys.Back:
                        lock (_typingBuffer)
                        {
                            _typingBuffer.Enqueue(new TypeBackspace());
                        }
                        break;
                    case Keys.Left:
                        lock (_typingBuffer)
                        {
                            _typingBuffer.Enqueue(new MoveCursorBackwards());
                        }
                        break;
                    case Keys.Right:
                        lock (_typingBuffer)
                        {
                            _typingBuffer.Enqueue(new MoveCursorForwards());
                        }
                        break;
                    case Keys.Delete:
                        lock (_typingBuffer)
                        {
                            _typingBuffer.Enqueue(new TypeDelete());
                        }
                        break;
                    default:
                        lock (_typingBuffer)
                        {
                            _typingBuffer.Enqueue(new TypeCharacter(e.Character));
                        }
                        break;
                }
            }
        }
        public class CursorState
        {
            // Fields
            MouseState currentMouseState;
            MouseState previousMouseState;
            bool currentIsActive;
            bool previousIsActive;

            // Properties
            public Vec Position => new Vec(currentMouseState.Position);
            public Vec PreviousPosition => new Vec(previousMouseState.Position);
            public bool JustMoved => Position != PreviousPosition;

            // Private Methods
            private bool _isPressed(MouseButton mouseButton, MouseState state, bool isActive)
            {
                if (!isActive) return false;
                switch (mouseButton)
                {
                    case MouseButton.Left: return state.LeftButton == ButtonState.Pressed;
                    case MouseButton.Middle: return state.MiddleButton == ButtonState.Pressed;
                    case MouseButton.Right: return state.RightButton == ButtonState.Pressed;
                }
                throw new ArgumentException($"CursorState does not handle the following MouseButton: {mouseButton}");
            }

            // Methods
            public CursorState(MonoGameInstance mgi)
            {
                previousIsActive = mgi.IsActive;
                currentIsActive = previousIsActive;
                previousMouseState = Mouse.GetState();
                currentMouseState = previousMouseState;
            }

            public void Update(MonoGameInstance mgi)
            {
                previousIsActive = currentIsActive;
                currentIsActive = mgi.IsActive;
                previousMouseState = currentMouseState;
                currentMouseState = Mouse.GetState();
            }

            public bool IsPressed(MouseButton mouseButton)
            {
                return _isPressed(mouseButton, currentMouseState, currentIsActive);
            }

            public bool WasJustPressed(MouseButton mouseButton)
            {
                return _isPressed(mouseButton, currentMouseState, currentIsActive) && !_isPressed(mouseButton, previousMouseState, previousIsActive);
            }

            public bool WasJustReleased(MouseButton mouseButton)
            {
                return !_isPressed(mouseButton, currentMouseState, currentIsActive) && _isPressed(mouseButton, previousMouseState, previousIsActive);
            }

            public int GetCursorScrollThisTick()
            {
                return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
            }
        }
        public class ControllerState
        {
            // Fields
            GamePadState currentControllerStatePlayer1;
            GamePadState previousControllerStatePlayer1;

            GamePadState currentControllerStatePlayer2;
            GamePadState previousControllerStatePlayer2;

            GamePadState currentControllerStatePlayer3;
            GamePadState previousControllerStatePlayer3;

            GamePadState currentControllerStatePlayer4;
            GamePadState previousControllerStatePlayer4;

            // Methods

            public ControllerState()
            {
                previousControllerStatePlayer1 = GamePad.GetState(PlayerIndex.One);
                currentControllerStatePlayer1 = previousControllerStatePlayer1;

                previousControllerStatePlayer2 = GamePad.GetState(PlayerIndex.Two);
                currentControllerStatePlayer2 = previousControllerStatePlayer2;

                previousControllerStatePlayer3 = GamePad.GetState(PlayerIndex.Three);
                currentControllerStatePlayer3 = previousControllerStatePlayer3;

                previousControllerStatePlayer4 = GamePad.GetState(PlayerIndex.Four);
                currentControllerStatePlayer4 = previousControllerStatePlayer4;
            }

            public void Update()
            {
                previousControllerStatePlayer1 = currentControllerStatePlayer1;
                currentControllerStatePlayer1 = GamePad.GetState(PlayerIndex.One);

                previousControllerStatePlayer2 = currentControllerStatePlayer2;
                currentControllerStatePlayer2 = GamePad.GetState(PlayerIndex.Two);

                previousControllerStatePlayer3 = currentControllerStatePlayer3;
                currentControllerStatePlayer3 = GamePad.GetState(PlayerIndex.Three);

                previousControllerStatePlayer4 = currentControllerStatePlayer4;
                currentControllerStatePlayer4 = GamePad.GetState(PlayerIndex.Four);
            }

            public GamePadState GetState()
            {
                return currentControllerStatePlayer1;
            }

            public bool IsPressed(Buttons button)
            {
                return currentControllerStatePlayer1.IsButtonDown(button);
            }

            public bool WasJustPressed(Buttons button)
            {
                return currentControllerStatePlayer1.IsButtonDown(button) && !previousControllerStatePlayer1.IsButtonDown(button);
            }

            public bool WasJustReleased(Buttons button)
            {
                return !currentControllerStatePlayer1.IsButtonDown(button) && previousControllerStatePlayer1.IsButtonDown(button);
            }

            public GamePadState GetState(PlayerIndex player)
            {
                switch (player)
                {
                    case PlayerIndex.One: return currentControllerStatePlayer1;
                    case PlayerIndex.Two: return currentControllerStatePlayer2;
                    case PlayerIndex.Three: return currentControllerStatePlayer3;
                    case PlayerIndex.Four: return currentControllerStatePlayer4;
                }
                throw new Exception("Invalid controller player index in GetState");
            }

            public bool IsPressed(Buttons button, PlayerIndex player)
            {
                switch (player)
                {
                    case PlayerIndex.One: return currentControllerStatePlayer1.IsButtonDown(button);
                    case PlayerIndex.Two: return currentControllerStatePlayer2.IsButtonDown(button);
                    case PlayerIndex.Three: return currentControllerStatePlayer3.IsButtonDown(button);
                    case PlayerIndex.Four: return currentControllerStatePlayer4.IsButtonDown(button);
                }
                throw new Exception("Invalid controller player index in IsPressed");
            }

            public bool WasJustPressed(Buttons button, PlayerIndex player)
            {
                switch (player)
                {
                    case PlayerIndex.One: return currentControllerStatePlayer1.IsButtonDown(button) && !previousControllerStatePlayer1.IsButtonDown(button);
                    case PlayerIndex.Two: return currentControllerStatePlayer2.IsButtonDown(button) && !previousControllerStatePlayer2.IsButtonDown(button);
                    case PlayerIndex.Three: return currentControllerStatePlayer3.IsButtonDown(button) && !previousControllerStatePlayer3.IsButtonDown(button);
                    case PlayerIndex.Four: return currentControllerStatePlayer4.IsButtonDown(button) && !previousControllerStatePlayer4.IsButtonDown(button);
                }
                throw new Exception("Invalid controller player index in WasJustPressed");
            }

            public bool WasJustReleased(Buttons button, PlayerIndex player)
            {
                switch (player)
                {
                    case PlayerIndex.One: return !currentControllerStatePlayer1.IsButtonDown(button) && previousControllerStatePlayer1.IsButtonDown(button);
                    case PlayerIndex.Two: return !currentControllerStatePlayer2.IsButtonDown(button) && previousControllerStatePlayer2.IsButtonDown(button);
                    case PlayerIndex.Three: return !currentControllerStatePlayer3.IsButtonDown(button) && previousControllerStatePlayer3.IsButtonDown(button);
                    case PlayerIndex.Four: return !currentControllerStatePlayer4.IsButtonDown(button) && previousControllerStatePlayer4.IsButtonDown(button);
                }
                throw new Exception("Invalid controller player index in WasJustPressed");
            }
        }
    }
}
