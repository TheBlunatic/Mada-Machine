using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blunatic.Core;
using Microsoft.Xna.Framework;

namespace Blunatic.Mgc
{
    public static class ConsoleClick
    {
        // Enums
        public enum Mode
        {
            None,
            Left,
            Middle,
            Right,
        }

        // Structs
        public struct Occurance
        {
            public Mode ClickMode;
            public Vec StartingCell;
            public Vector2 StartingCellRelative;
            public Vec LastCell;
            public Vector2 LastCellRelative;
            public Vec PenultimateCell;
            public Vector2 PenultimateCellRelative;
            public bool HasLeftStartingCell;
            public int TickAge;

            public Occurance()
            {
                ClickMode = Mode.None;
                HasLeftStartingCell = false;
                StartingCell = new Vec(0, 0);
                StartingCellRelative = new Vector2(0.5f);
                LastCell = new Vec(0, 0);
                LastCellRelative = new Vector2(0.5f);
                PenultimateCell = new Vec(0, 0);
                PenultimateCellRelative = new Vector2(0.5f);
                TickAge = 0;
            }
        }
        public class Detector
        {
            // Properties
            public Occurance LastClickTick => _lastTickClick;
            public Occurance CurrentClickTick => _currentTickClick;

            // Fields
            private Occurance _lastTickClick;
            private Occurance _currentTickClick;

            private Microsoft.Xna.Framework.Rectangle _confine;

            // Constructors
            public Detector()
            {
                _lastTickClick = new Occurance();
                _currentTickClick = new Occurance();
                _confine = new Microsoft.Xna.Framework.Rectangle(0, 0, int.MaxValue, int.MaxValue);
            }
            public Detector(Microsoft.Xna.Framework.Rectangle confineToRectangle) : this()
            {
                _confine = confineToRectangle;
            }

            // Private Methods
            private void _initialiseNewClick(MonoGameInstance mgi, MonoGameConsole mgc, Mode newMode)
            {
                _currentTickClick.ClickMode = newMode;
                _currentTickClick.StartingCell = mgc.GetCursorCellPos(mgi);
                _currentTickClick.StartingCellRelative = mgc.GetCursorInsideCellPos(mgi);
                _currentTickClick.PenultimateCell = _currentTickClick.StartingCell;
                _currentTickClick.PenultimateCellRelative = _currentTickClick.StartingCellRelative;
                _currentTickClick.LastCell = _currentTickClick.StartingCell;
                _currentTickClick.LastCellRelative = _currentTickClick.StartingCellRelative;
                _currentTickClick.TickAge = 0;
                _currentTickClick.HasLeftStartingCell = false;
            }
            private void _updateClickDetection(MonoGameInstance mgi, MonoGameConsole mgc)
            {
                _lastTickClick = _currentTickClick;

                _currentTickClick.TickAge++;
                _currentTickClick.PenultimateCell = _currentTickClick.LastCell;
                _currentTickClick.PenultimateCellRelative = _currentTickClick.LastCellRelative;
                _currentTickClick.LastCell = mgc.GetCursorCellPos(mgi);
                _currentTickClick.LastCellRelative = mgc.GetCursorInsideCellPos(mgi);

                if (_currentTickClick.LastCell != _currentTickClick.StartingCell) _currentTickClick.HasLeftStartingCell = true;

                switch (_currentTickClick.ClickMode)
                {
                    case Mode.None:
                        if (_currentTickClick.LastCell.IsInBounds(_confine))
                        {
                            if (mgi.CursorState.WasJustPressed(MouseButton.Left)) _initialiseNewClick(mgi, mgc, Mode.Left);
                            else if (mgi.CursorState.WasJustPressed(MouseButton.Right)) _initialiseNewClick(mgi, mgc, Mode.Right);
                            else if (mgi.CursorState.WasJustPressed(MouseButton.Middle)) _initialiseNewClick(mgi, mgc, Mode.Middle);
                        }
                        break;
                    case Mode.Left:
                        if (!mgi.CursorState.IsPressed(MouseButton.Left)) _initialiseNewClick(mgi, mgc, Mode.None);
                        break;
                    case Mode.Right:
                        if (!mgi.CursorState.IsPressed(MouseButton.Right)) _initialiseNewClick(mgi, mgc, Mode.None);
                        break;
                    case Mode.Middle:
                        if (!mgi.CursorState.IsPressed(MouseButton.Middle)) _initialiseNewClick(mgi, mgc, Mode.None);
                        break;
                }
            }

            // Methods
            public void ChangeBounds(Rectangle rectangle)
            {
                _confine = rectangle;
            }
            public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
            {
                _updateClickDetection(mgi, mgc);
            }
        }
    }
}
