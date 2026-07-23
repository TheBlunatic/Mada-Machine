using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public class MonoGameConsoleTextInput : IMonoGameConsoleElement
    {
        // Classes
        public class TextUpdatedEventArgs : EventArgs
        {
            public MonoGameConsoleTextInput Sender { get; init; }
            public string ChangedFrom { get; init; }
            public string ChangedTo { get; init; }
        }

        // Properties
        public Vec Position => Vec.GetXY(_interactionRectangle);
        public Vec Dimensions => Vec.GetDimensions(_interactionRectangle);
        public string Text
        {
            get
            {
                return _valuedText;
            }
            set
            {
                _valuedText = value.Substring(0, Math.Min(_maxLength, value.Length));
            }
        }
        public bool CapturingControls => _active;

        // Events
        public event EventHandler<TextUpdatedEventArgs> TextUpdated;

        // Fields
        private int _maxLength;
        private string _displayedText;
        private string _valuedText;
        private bool _active;
        private Rectangle _interactionRectangle;
        private string _allowedCharacters;
        private int _cursor;

        // Constructors
        public MonoGameConsoleTextInput(Vec location, int length, string startingText, string allowedCharacters)
        {
            _maxLength = length;
            Text = startingText;
            _displayedText = _valuedText;
            _active = false;
            _interactionRectangle = new Rectangle(location, new Vec(_maxLength, 1));
            _allowedCharacters = allowedCharacters;
        }

        // Methods
        public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            Vec hoveredPos = mgc.GetCursorHoveredCellPos(mgi);

            if (_active)
            {
                if (!hoveredPos.IsInBounds(_interactionRectangle) && (mgi.CursorState.WasJustPressed(MouseButton.Left) || mgi.CursorState.WasJustPressed(MouseButton.Right)))
                {
                    _displayedText = _valuedText;
                    _active = false;
                }
                else if (mgi.KeyState.WasJustPressed(Keys.Enter))
                {
                    _active = false;
                    string changedFrom = _valuedText;
                    _valuedText = _displayedText;
                    TextUpdated?.Invoke(this, new TextUpdatedEventArgs() { Sender = this, ChangedFrom = changedFrom, ChangedTo = _displayedText });
                    _displayedText = _valuedText;
                }
                else
                {
                    _displayedText = mgi.KeyState.ApplyTypingToString(_displayedText, _allowedCharacters, _cursor, out _cursor, _maxLength);
                    _displayedText = _displayedText.Substring(0, Math.Min(_maxLength, _displayedText.Length));
                    _cursor = Math.Clamp(_cursor, 0, _displayedText.Length);
                }
            }
            else
            {
                _displayedText = _valuedText;
                if (hoveredPos.IsInBounds(_interactionRectangle) && mgi.CursorState.WasJustPressed(MouseButton.Left))
                {
                    _active = true;
                    _cursor = _displayedText.Length;
                }
            }
        }
        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            string actualString = _displayedText;
            for (int i = 0; i < actualString.Length; i++)
            {
                mgc.SetCell(Vec.GetXY(_interactionRectangle) + new Vec(i, 0), Ch.MatchChar(actualString[i]), Color.White, _active ? new Color(30, 30, 70) : new Color(15, 15, 35));
            }
            for (int i = actualString.Length; i < _maxLength; i++)
            {
                mgc.SetCell(Vec.GetXY(_interactionRectangle) + new Vec(i, 0), Ch.Space, Color.White, _active ? new Color(30, 30, 70) : new Color(15, 15, 35));
            }
            if (_active && (_cursor < _maxLength) && mgi.Ticks % 60 < 30)
            {
                mgc.SetCell(Vec.GetXY(_interactionRectangle) + new Vec(_cursor, 0), null, Color.Black, new Color(255 - 30, 255 - 30, 255 - 70));
            }
        }

    }
}
