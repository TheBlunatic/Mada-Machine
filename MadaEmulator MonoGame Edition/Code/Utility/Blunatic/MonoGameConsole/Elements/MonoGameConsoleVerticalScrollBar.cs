using Blunatic.Core;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blunatic.Mgc
{
    public class MonoGameConsoleVerticalScrollBar : IMonoGameConsoleElement
    {
        // Properties
        public Vec Position
        {
            get
            {
                return Vec.GetXY(_rectangle);
            }
            set
            {
                _rectangle = new Rectangle(value, Vec.GetDimensions(_rectangle));
                _detector.ChangeBounds(_rectangle);
                _needsInvalidate = true;
            }
        }
        public Vec Dimensions
        {
            get
            {
                return Vec.GetDimensions(_rectangle);
            }
            set
            {
                _rectangle = new Rectangle(Vec.GetXY(_rectangle), value);
                _detector.ChangeBounds(_rectangle);
                _needsInvalidate = true;
            }
        }
        public bool CapturingControls => _detector.CurrentClickTick.ClickMode == ConsoleClick.Mode.Left;

        public int ViewSize
        {
            get
            {
                return _viewSize;
            }
            set
            {
                _viewSize = value;
                _needsInvalidate = true;
            }
        }

        public int TotalSize
        {
            get
            {
                return _totalSize;
            }
            set
            {
                _totalSize = value;
                _needsInvalidate = true;
            }
        }

        public double TargetValue
        {
            get
            {
                return _targetValue;
            }
            set
            {
                _targetValue = value;
                _needsInvalidate = true;
            }
        }

        public double Progress
        {
            get
            {
                return _targetValue / (_totalSize - _viewSize);
            }
            set
            {
                _targetValue = (_totalSize - _viewSize) * value;
                _needsInvalidate = true;
            }
        }

        public bool IsHidden { get; set; }
        public bool IsClicked { get; private set; }

        // Fields
        private Rectangle _rectangle;

        private ConsoleClick.Detector _detector;

        private int _viewSize;
        private int _totalSize;

        private double _idealBarHeight;
        private int _barHeight;

        private double _targetValue;

        private double _idealBarPosition;
        private int _barPosition;

        private bool _needsInvalidate;

        private double _dragInitialPosition;
        private double _dragInitialOffsetToBar;

        // Constructors
        public MonoGameConsoleVerticalScrollBar(MonoGameInstance mgi, Vec position, Vec dimensions, int viewSize, int totalSize)
        {
            _rectangle = new Rectangle(position, dimensions);
            _detector = new ConsoleClick.Detector(_rectangle);

            _viewSize = viewSize;
            _totalSize = totalSize;

            _targetValue = 0;

            _needsInvalidate = true;

            IsHidden = false;

            _dragInitialPosition = double.NaN;
            _dragInitialOffsetToBar = 0;
        }

        // Methods
        private void _invalidate()
        {
            _totalSize = Math.Max(_totalSize, 1);
            _viewSize = Math.Clamp(_viewSize, 1, _totalSize);

            _idealBarHeight = (double)Dimensions.Y * (double)_viewSize / (double)_totalSize;
            _barHeight = Math.Clamp((int)Math.Round(_idealBarHeight, 0, MidpointRounding.AwayFromZero), 1, Dimensions.Y);

            _targetValue = Math.Clamp(_targetValue, 0, _totalSize - _viewSize);

            _idealBarPosition = _targetValue * (double)Dimensions.Y / _totalSize;

            _barPosition = (int)Math.Round(_idealBarPosition, 0, MidpointRounding.AwayFromZero);
        }

        public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            _detector.Update(mgi, mgc);

            IsClicked = !IsHidden && _detector.CurrentClickTick.ClickMode == ConsoleClick.Mode.Left;

            if (_needsInvalidate && !IsHidden)
            {
                _invalidate();
                _needsInvalidate = false;
            }

            if (IsClicked)
            {
                double clickedPosition = (double)_detector.CurrentClickTick.LastCell.Y + (double)_detector.CurrentClickTick.LastCellRelative.Y;
                double relativeClickedPosition = clickedPosition - Vec.GetXY(_rectangle).Y;

                if (_detector.CurrentClickTick.TickAge == 0)
                {
                    if (relativeClickedPosition >= _idealBarPosition && relativeClickedPosition < _idealBarPosition + _idealBarHeight)
                    {

                        _dragInitialPosition = relativeClickedPosition;
                        _dragInitialOffsetToBar = relativeClickedPosition - (double)_idealBarPosition;
                    }
                    else
                    {
                        _dragInitialPosition = double.NaN;
                    }
                }

                if (double.IsNaN(_dragInitialPosition))
                {
                    double centralToBarPosition = relativeClickedPosition - _idealBarHeight / 2;
                    _targetValue = (centralToBarPosition / _rectangle.Height) * _totalSize;
                }
                else
                {
                    double centralToBarPosition = relativeClickedPosition - _dragInitialOffsetToBar;
                    _targetValue = (centralToBarPosition / _rectangle.Height) * _totalSize;
                }
                
                _invalidate();
            }
            else
            {
                _dragInitialPosition = double.NaN;
            }
        }
        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            if (IsHidden) return;

            if (_needsInvalidate)
            {
                _invalidate();
                _needsInvalidate = false;
            }

            mgc.Fill(_rectangle, Ch.BlockMedium, Color.Black, new Color(123, 123, 123));
            mgc.Fill(new Rectangle(_rectangle.X, _rectangle.Y + _barPosition, _rectangle.Width, _barHeight), Ch.BlockFull, Color.DarkGray);
        }
    }
}
