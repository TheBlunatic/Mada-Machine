using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public class MonoGameConsoleButton : IMonoGameConsoleElement
    {
        // Constants
        public static readonly Color DEFALULT_INACTIVE_BACKGROUND_COLOR = new Color(30, 30, 30);
        public static readonly Color DEFALULT_ACTIVE_BACKGROUND_COLOR = new Color(60, 60, 60);

        // Properties
        public Vec Position => _position;
        public Vec Dimensions => _dimensions;
        public bool CapturingControls => false;

        public bool IsHovered { get; private set; }
        public bool IsLeftClicked { get; private set; }
        public bool IsRightClicked { get; private set; }
        public bool IsMiddleClicked { get; private set; }
        public bool Inverted { get { return _inverted; } set { _inverted = value; } }
        public bool IsHidden { get; set; }

        public Color InactiveBackgroundColor { get; set; }
        public Color ActiveBackgroundColor { get; set; }

        // Events
        public event EventHandler LeftClicked;
        public event EventHandler MiddleClicked;
        public event EventHandler RightClicked;

        // Fields
        private Vec _position;
        private Vec _dimensions;
        private string _text;
        private Rectangle _rectangle;
        private bool _inverted;

        private string _startPattern = "[ ";
        private string _endPattern = $" ]";

        // Constructors
        public MonoGameConsoleButton(MonoGameInstance mgi, Vec pos, string text, bool havePatternedSides = true)
        {
            IsHidden = false;
            InactiveBackgroundColor = DEFALULT_INACTIVE_BACKGROUND_COLOR;
            ActiveBackgroundColor = DEFALULT_ACTIVE_BACKGROUND_COLOR;
            _inverted = false;
            _position = pos;
            _text = text;
            if (!havePatternedSides)
            {
                _startPattern = "";
                _endPattern = "";
            }
            _text = $"{_startPattern}{_text}{_endPattern}";
            _dimensions = MonoGameConsole.GetWriteStringDimensions(mgi, _text);
            _updateRectangle();
        }

        // Private Methods
        private void _updateRectangle()
        {
            _rectangle = new Rectangle(Position, Dimensions);
        }

        // Methods
        public void Centre()
        {
            _position.X -= Dimensions.X / 2;
            _updateRectangle();
        }

        public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            if (IsHidden) return;
            Vec cursorHover = mgc.GetCursorHoveredCellPos(mgi);

            IsHovered = cursorHover.IsInBounds(_rectangle);

            IsLeftClicked = IsHovered && mgi.CursorState.WasJustPressed(MouseButton.Left);
            IsRightClicked = IsHovered && mgi.CursorState.WasJustPressed(MouseButton.Right);
            IsMiddleClicked = IsHovered && mgi.CursorState.WasJustPressed(MouseButton.Middle);

            if (IsLeftClicked) LeftClicked?.Invoke(this, EventArgs.Empty);
            if (IsMiddleClicked) MiddleClicked?.Invoke(this, EventArgs.Empty);
            if (IsRightClicked) RightClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            if (IsHidden) return;
            Color fg = Color.White;
            Color bg = IsHovered ? ActiveBackgroundColor : InactiveBackgroundColor;

            if (_inverted)
            {
                fg = new Color(255 - fg.R, 255 - fg.G, 255 - fg.B);
                bg = new Color(255 - bg.R, 255 - bg.G, 255 - bg.B);
            }

            mgc.WriteString(mgi, _position, $"{Fm.Col(fg, bg)}{_text}");
        }
    }
}
