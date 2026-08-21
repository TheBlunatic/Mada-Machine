using Microsoft.Xna.Framework;
using System;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public class MonoGameConsoleRegion : IMonoGameConsoleElement
    {
        // Classes
        public class RegionClickArgs : EventArgs
        {
            public Vec ConsolePosition;
            public Vec RelativePosition;

            public RegionClickArgs(Vec consolePosition, Vec relativePosition)
            {
                ConsolePosition = consolePosition;
                RelativePosition = relativePosition;
            }
        }

        // Properties
        public Vec Position { get { return Vec.GetXY(_area); } set { _area = new Rectangle(value, Vec.GetDimensions(_area)); ClickDetector.ChangeBounds(_area); } }
        public Vec Dimensions { get { return Vec.GetDimensions(_area); } set { _area = new Rectangle(Vec.GetXY(_area), value); ClickDetector.ChangeBounds(_area); } }
        public bool CapturingControls => false;

        public ConsoleClick.Detector ClickDetector { get; private set; }

        public Rectangle Area => _area;

        public bool IsHovered { get; private set; }
        public bool IsLeftClicked { get; private set; }
        public bool IsRightClicked { get; private set; }
        public bool IsMiddleClicked { get; private set; }
        public Color? DebugColor { get; set; }

        // Events
        public event EventHandler LeftClicked;
        public event EventHandler MiddleClicked;
        public event EventHandler RightClicked;

        // Fields
        private Rectangle _area;

        // Constructors
        public MonoGameConsoleRegion(Vec position, Vec dimensions) : this(new Rectangle(position, dimensions))
        {

        }
        public MonoGameConsoleRegion(Rectangle area)
        {
            _area = area;
            DebugColor = null;
            ClickDetector = new ConsoleClick.Detector(area);
        }

        // Methods
        public Vec GetRelativePositionOfScreenPosition(Vec screenPosition)
        {
            return screenPosition - Position;
        }
        public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            ClickDetector.Update(mgi, mgc);

            Vec cursorHover = mgc.GetCursorCellPos(mgi);
            IsHovered = cursorHover.IsInBounds(_area);

            IsLeftClicked = IsHovered && mgi.CursorState.WasJustPressed(MouseButton.Left);
            IsRightClicked = IsHovered && mgi.CursorState.WasJustPressed(MouseButton.Right);
            IsMiddleClicked = IsHovered && mgi.CursorState.WasJustPressed(MouseButton.Middle);

            if (IsLeftClicked) LeftClicked?.Invoke(this, new RegionClickArgs(cursorHover, cursorHover - Position));
            if (IsMiddleClicked) MiddleClicked?.Invoke(this, new RegionClickArgs(cursorHover, cursorHover - Position));
            if (IsRightClicked) RightClicked?.Invoke(this, new RegionClickArgs(cursorHover, cursorHover - Position));
        }

        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            if (DebugColor.HasValue)
            {
                mgc.Fill(_area, Ch.Period, MonoGameConsole.GetFurthestColor(DebugColor.Value), DebugColor.Value);
            }
        }
    }
}
