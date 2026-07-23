using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public interface IMonoGameConsoleElement
    {
        public Vec Position { get; }
        public Vec Dimensions { get; }
        public Rectangle Rectangle { get { return new Rectangle(Position, Dimensions); } }
        public bool CapturingControls { get; }

        public void Update(MonoGameInstance mgi, MonoGameConsole mgc);
        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc);
    }
    public class MonoGameConsole
    {
        // Constants
        public static readonly Color DEFAULT_OUT_OF_BOUNDS_COLOR = Color.Black;
        public static readonly bool DEFAULT_CLEAR_SCREEN_AFTER_DRAW = true;

        // Enums
        public enum WrapType
        {
            Cut,
            BasicWrap,
            WordWrap,
        }

        // Classes
        private class Cell
        {
            private byte characterPositionIndicator = 0x00;
            private Color foregroundColor = Color.White;
            private Color backgroundColor = Color.Black;

            public byte CharacterPositionIndicator { get { return characterPositionIndicator; } set { characterPositionIndicator = value; } }
            public Color ForegroundColor { get { return foregroundColor; } set { foregroundColor = value; } }
            public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = value; } }

            public Cell()
            {
                Reset();
            }

            public void Reset()
            {
                characterPositionIndicator = 0x00;
                foregroundColor = Color.White;
                backgroundColor = Color.Black;
            }
        }
        private static class StringDrawCache
        {
            // Constants
            private const int MAX_IDLE_TICKS = 5;
            private const int CULL_CHECK_INTERVAL = 60;

            private static Vec DEFAULT_DIMENSIONS = new Vec(ushort.MaxValue);
            private const WrapType DEFAULT_WRAP_TYPE = WrapType.BasicWrap;

            // Properties
            public static int MaxIdleTicks => MAX_IDLE_TICKS;
            public static int CullCheckInterval => CULL_CHECK_INTERVAL;
            public static Vec DefaultDimensions => DEFAULT_DIMENSIONS;
            public static WrapType DefaultWrapType => DEFAULT_WRAP_TYPE;
            public static int CachedBlueprintCount => _cacheBlueprints.Count;
            public static ulong CurrentCacheID => _globalID;
            public static int LastCulledTick => _lastCulledTick;
            public static int AmountLastCulled => _amountLastCulled;
            public static int TotalCachedBeforeLastCull => _totalCachedBeforeLastCull;

            // Cache
            private static List<IContainConsoleCellInfo> _horizontalCache = new List<IContainConsoleCellInfo>();
            private static List<IContainConsoleCellInfo> _spareCache = new List<IContainConsoleCellInfo>();
            private static List<IContainConsoleCellInfo[]> _verticalCache = new List<IContainConsoleCellInfo[]>();
            private static Queue<string> _cullQueueCache = new Queue<string>();

            // Interfaces
            private interface IContainConsoleCellInfo
            {
                public byte GetGlyph(MonoGameInstance mgi);
                public Color GetForegroundColor(MonoGameInstance mgi);
                public Color GetBackgroundColor(MonoGameInstance mgi);
            }

            // Classes
            private class StandardCharacter : IContainConsoleCellInfo
            {
                // Fields
                private byte _glyph;
                private Color _foregroundColor;
                private Color _backgroundColor;

                // Constructors
                public StandardCharacter(byte glyph, Color foregroundColor, Color backgroundColor)
                {
                    _glyph = glyph;
                    _foregroundColor = foregroundColor;
                    _backgroundColor = backgroundColor;
                }

                // Methods
                public byte GetGlyph(MonoGameInstance mgi) => _glyph;
                public Color GetForegroundColor(MonoGameInstance mgi) => _foregroundColor;
                public Color GetBackgroundColor(MonoGameInstance mgi) => _backgroundColor;
            }
            private class AnimatedCharacter : IContainConsoleCellInfo
            {
                // Fields
                private short _animatedCellValue;
                private Color _foregroundColor;
                private Color _backgroundColor;

                // Constructors
                public AnimatedCharacter(short animatedCellValue, Color foregroundColor, Color backgroundColor)
                {
                    _animatedCellValue = animatedCellValue;
                    _foregroundColor = foregroundColor;
                    _backgroundColor = backgroundColor;
                }

                // Methods
                public byte GetGlyph(MonoGameInstance mgi) => Ch.MatchChar(Fm.GetCurrentCharacterInAnimatedCell(mgi, _animatedCellValue));
                public Color GetForegroundColor(MonoGameInstance mgi) => _foregroundColor;
                public Color GetBackgroundColor(MonoGameInstance mgi) => _backgroundColor;
            }

            private static ulong _globalID = 0;

            private static Dictionary<string, ulong> _cacheStringMap = new Dictionary<string, ulong>();
            private static Dictionary<ulong, IContainConsoleCellInfo[][]> _cacheBlueprints = new Dictionary<ulong, IContainConsoleCellInfo[][]>();
            private static Dictionary<ulong, int> _cacheDuration = new Dictionary<ulong, int>();
            private static Dictionary<ulong, Vec> _cacheDimensions = new Dictionary<ulong, Vec>();

            private static int _lastCulledTick = 0;
            private static int _amountLastCulled = 0;
            private static int _totalCachedBeforeLastCull = 0;

            private static void _cullCache()
            {
                _totalCachedBeforeLastCull = _cacheStringMap.Count;

                foreach (string cached in _cacheStringMap.Keys)
                {
                    ulong id = _cacheStringMap[cached];
                    if (_lastCulledTick - _cacheDuration[id] > MAX_IDLE_TICKS)
                    {
                        _cullQueueCache.Enqueue(cached);
                    }
                }

                _amountLastCulled = _cullQueueCache.Count;

                while (_cullQueueCache.Count > 0)
                {
                    string culled = _cullQueueCache.Dequeue();
                    ulong id = _cacheStringMap[culled];
                    _cacheBlueprints.Remove(id);
                    _cacheDuration.Remove(id);
                    _cacheDimensions.Remove(id);
                    _cacheStringMap.Remove(culled);
                }
            }

            private static ulong _cache(MonoGameInstance mgi, string text, Vec maxDimensions, WrapType wrapType)
            {
                string repString = _getRepString(text, maxDimensions, wrapType);

                if (!_cacheStringMap.TryGetValue(repString, out ulong id))
                {
                    id = _globalID++;
                    IContainConsoleCellInfo[][] blueprint = _getStringInstructions(text, maxDimensions, wrapType);
                    _cacheBlueprints.Add(id, blueprint);
                    _cacheStringMap.Add(repString, id);
                    _cacheDuration.Add(id, mgi.Ticks);
                    _cacheDimensions.Add(id, new Vec(blueprint.Aggregate(0, (i, x) => int.Max(i, x.Length)), blueprint.Length));
                }

                _cacheDuration[id] = mgi.Ticks;

                if (mgi.Ticks - _lastCulledTick >= CULL_CHECK_INTERVAL)
                {
                    _lastCulledTick = mgi.Ticks;
                    _cullCache();
                }

                return id;
            }
            private static string _getRepString(string text, Vec maxDimensions, WrapType wrapType)
            {
                return $"{maxDimensions}ᙈ{wrapType}ᙈ{text}";
            }
            private static IContainConsoleCellInfo[][] _getStringInstructions(string text, Vec maxDimensions, WrapType wrapType)
            {
                if (maxDimensions.X <= 0 || maxDimensions.Y <= 0) throw new ArgumentOutOfRangeException($"Maximum string dimensions cannot be 0 or less (was {maxDimensions}).");

                List<IContainConsoleCellInfo[]> vertical = _verticalCache;
                List<IContainConsoleCellInfo> horizontal = _horizontalCache;
                List<IContainConsoleCellInfo> spare = _spareCache;

                Color foregroundColor = Color.White;
                Color backgroundColor = Color.Black;

                bool currentLineIsEligibleForBreak = false;
                bool currentLineCanHaveLeadingSpaces = true;
                int lastBreakingSpace = -1;

                void GoToNewLine()
                {
                    _verticalCache.Add(_horizontalCache.ToArray());
                    _horizontalCache.Clear();
                    lastBreakingSpace = -1;
                    currentLineIsEligibleForBreak = false;
                }

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    switch (c)
                    {
                        case Fm.INITIALISER:
                            Fm.CodeCheckResult result = Fm.CheckForCode(text, i);
                            bool gotoDefault = false;
                            switch (result)
                            {
                                case Fm.CodeCheckResult.None:
                                    gotoDefault = true;
                                    break;
                                case Fm.CodeCheckResult.ModifyForegroundColor:
                                    foregroundColor = Fm.ReadModifyForegroundColorCode(text, i);
                                    break;
                                case Fm.CodeCheckResult.ModifyBackgroundColor:
                                    backgroundColor = Fm.ReadModifyBackgroundColorCode(text, i);
                                    break;
                                case Fm.CodeCheckResult.InsertAnimatedCell:
                                    horizontal.Add(new AnimatedCharacter(Fm.ReadAnimatedCellValueFromInsertAnimatedCellCode(text, i), foregroundColor, backgroundColor));
                                    break;
                            }
                            if (gotoDefault) goto default;
                            i += Fm.GetCodeLength(result) - 1;
                            break;
                        case '\n':
                            GoToNewLine();
                            currentLineCanHaveLeadingSpaces = true;
                            break;
                        default:
                            if (c == ' ')
                            {
                                if (currentLineCanHaveLeadingSpaces || currentLineIsEligibleForBreak)
                                {
                                    horizontal.Add(new StandardCharacter(Ch.MatchChar(c), foregroundColor, backgroundColor));
                                    lastBreakingSpace = horizontal.Count;
                                }
                            }
                            else
                            {
                                currentLineIsEligibleForBreak = true;
                                if (currentLineCanHaveLeadingSpaces && wrapType == WrapType.WordWrap)
                                {
                                    currentLineCanHaveLeadingSpaces = false;
                                }
                                horizontal.Add(new StandardCharacter(Ch.MatchChar(c), foregroundColor, backgroundColor));
                            }
                            break;
                    }
                    if (horizontal.Count > maxDimensions.X)
                    {
                        switch (wrapType)
                        {
                            case WrapType.Cut:
                                horizontal.RemoveAt(horizontal.Count - 1);
                                break;
                            case WrapType.BasicWrap:
                                spare.Add(horizontal.Last());
                                horizontal.RemoveAt(horizontal.Count - 1);
                                GoToNewLine();
                                horizontal.Add(spare[0]);
                                spare.Clear();
                                break;
                            case WrapType.WordWrap:
                                if (!currentLineIsEligibleForBreak || lastBreakingSpace == -1) goto case WrapType.BasicWrap;
                                while (horizontal.Count - 1 >= lastBreakingSpace)
                                {
                                    spare.Add(horizontal.Last());
                                    horizontal.RemoveAt(horizontal.Count - 1);
                                    currentLineCanHaveLeadingSpaces = true;
                                }
                                if (lastBreakingSpace != -1) horizontal.RemoveAt(horizontal.Count - 1);
                                GoToNewLine();
                                while (spare.Count > 0)
                                {
                                    horizontal.Add(spare.Last());
                                    spare.RemoveAt(spare.Count - 1);
                                }
                                break;

                        }
                    }
                    if (i == text.Length - 1)
                    {
                        GoToNewLine();
                    }
                    if (vertical.Count > maxDimensions.Y)
                    {
                        vertical.RemoveAt(vertical.Count - 1);
                        break;
                    }
                }

                IContainConsoleCellInfo[][] toReturn = vertical.ToArray();
                _horizontalCache.Clear();
                _verticalCache.Clear();
                _spareCache.Clear();
                return toReturn;
            }
            private static void _executeDraw(MonoGameInstance mgi, MonoGameConsole mgc, IContainConsoleCellInfo[][] blueprint, Vec position)
            {
                for (int y = 0; y < blueprint.Length; y++)
                {
                    for (int x = 0; x < blueprint[y].Length; x++)
                    {
                        mgc.SetCell(position + new Vec(x, y), blueprint[y][x].GetGlyph(mgi), blueprint[y][x].GetForegroundColor(mgi), blueprint[y][x].GetBackgroundColor(mgi));
                    }
                }
            }
            public static void DrawFormattedString(MonoGameInstance mgi, MonoGameConsole mgc, string text, Vec position)
            {
                DrawFormattedString(mgi, mgc, text, position, DEFAULT_DIMENSIONS, DEFAULT_WRAP_TYPE);
            }
            public static void DrawFormattedString(MonoGameInstance mgi, MonoGameConsole mgc, string text, Vec position, Vec maxDimensions, WrapType wrapType)
            {
                ulong id = _cache(mgi, text, maxDimensions, wrapType);
                _executeDraw(mgi, mgc, _cacheBlueprints[id], position);
            }
            public static Vec GetFormattedStringDimensions(MonoGameInstance mgi, string text)
            {
                return GetFormattedStringDimensions(mgi, text, DEFAULT_DIMENSIONS, DEFAULT_WRAP_TYPE);
            }
            public static Vec GetFormattedStringDimensions(MonoGameInstance mgi, string text, Vec maxDimensions, WrapType wrapType)
            {
                ulong id = _cache(mgi, text, maxDimensions, wrapType);
                return _cacheDimensions[id];
            }
        }

        // Static Properties
        public static int MaxIdleCacheTicks => StringDrawCache.MaxIdleTicks;
        public static int CullCheckInterval => StringDrawCache.CullCheckInterval;
        public static int CachedBlueprintCount => StringDrawCache.CachedBlueprintCount;
        public static ulong CurrentCacheID => StringDrawCache.CurrentCacheID;
        public static int LastCulledTick => StringDrawCache.LastCulledTick;
        public static int AmountLastCulled => StringDrawCache.AmountLastCulled;
        public static int TotalCachedBeforeLastCull => StringDrawCache.TotalCachedBeforeLastCull;

        // Properties
        public bool ClearScreenAfterDraw { get; set; }
        public float Scale { get { return _charScale; } }
        public int TotalCharacterCount { get { return _charGrid.GetLength(0) * _charGrid.GetLength(1); } }
        public Vec Dimensions => Vec.GetDimensions(_charGrid);
        public Color OutOfBoundsColor { get; set; }

        // Fields

        private Random _internalRNG = new Random();

        private Vec _lastSeenScreenDimensions;

        private float _charScale;
        private Rectangle _rectangleOccupied;

        private Cell[,] _charGrid;

        private Rectangle _printTarget;

        private List<IMonoGameConsoleElement> _elements;

        public MonoGameConsole(MonoGameInstance mgi, Vec dimensions)
        {
            OutOfBoundsColor = DEFAULT_OUT_OF_BOUNDS_COLOR;
            ClearScreenAfterDraw = DEFAULT_CLEAR_SCREEN_AFTER_DRAW;

            _elements = new List<IMonoGameConsoleElement>();

            _charGrid = new Cell[dimensions.X, dimensions.Y];
            for (int y = 0; y < _charGrid.GetLength(1); y++)
            {
                for (int x = 0; x < _charGrid.GetLength(0); x++)
                {
                    _charGrid[x, y] = new Cell();
                }
            }
            Clear();

            _transformToFitWindow(mgi);

            ResetPrintTarget();
        }

        // Private Methods
        private void _transformToFitWindow(MonoGameInstance mgi)
        {
            _lastSeenScreenDimensions = mgi.ScreenDimensions;

            float unscaledWidth = Ch.CharWidth * _charGrid.GetLength(0);
            float unscaledHeight = Ch.CharHeight * _charGrid.GetLength(1);

            float widthScaleIdeal = _lastSeenScreenDimensions.X / unscaledWidth;
            float heightScaleIdeal = _lastSeenScreenDimensions.Y / unscaledHeight;

            _charScale = Math.Min(widthScaleIdeal, heightScaleIdeal);

            float actualWidth = unscaledWidth * _charScale;
            float actualHeight = unscaledHeight * _charScale;

            float tlx = (_lastSeenScreenDimensions.X - actualWidth) / 2;
            float tly = (_lastSeenScreenDimensions.Y - actualHeight) / 2;

            _rectangleOccupied = new Rectangle(new Vector2(tlx, tly).ToPoint(), new Vector2(actualWidth, actualHeight).ToPoint());
        }
        private Cell _getCell(Vec location)
        {
            return _charGrid[location.X, location.Y];
        }
        private Cell _getCell(Vec location, out Cell cell)
        {
            cell = _charGrid[location.X, location.Y];
            return cell;
        }
        private static Color _blendColorIntoFullColorByAlpha(Color original, Color addition)
        {
            double intensity = addition.A / 255d;
            double intensityminus = 1 - intensity;
            byte r = (byte)Math.Clamp(intensity * addition.R + intensityminus * original.R, byte.MinValue, byte.MaxValue);
            byte g = (byte)Math.Clamp(intensity * addition.G + intensityminus * original.G, byte.MinValue, byte.MaxValue);
            byte b = (byte)Math.Clamp(intensity * addition.B + intensityminus * original.B, byte.MinValue, byte.MaxValue);
            return new Color(r, g, b);
        }

        // Static Methods
        public static Color GetFurthestColor(Color from)
        {
            return new Color
            (
                from.R < 128 ? byte.MaxValue : byte.MinValue,
                from.G < 128 ? byte.MaxValue : byte.MinValue,
                from.B < 128 ? byte.MaxValue : byte.MinValue
            );
        }
        public static Color BlendColor(Color opaqueColor, Color blendColor) => _blendColorIntoFullColorByAlpha(opaqueColor, blendColor);
        public static Vec GetPrintStringDimensions(string text)
        {
            int currentLength = 0;
            int longestLength = 0;
            int lineCount = 1;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '\n':
                        longestLength = Math.Max(longestLength, currentLength);
                        currentLength = 0;
                        lineCount++;
                        break;
                    default:
                        currentLength++;
                        break;
                }
            }
            longestLength = Math.Max(longestLength, currentLength);
            return new Vec(longestLength, lineCount);
        }
        public static Vec GetWriteStringDimensions(MonoGameInstance mgi, string text)
        {
            return StringDrawCache.GetFormattedStringDimensions(mgi, text);
        }
        public static Vec GetWriteStringDimensions(MonoGameInstance mgi, string text, Vec maxDimensions, WrapType wrapType)
        {
            return StringDrawCache.GetFormattedStringDimensions(mgi, text, maxDimensions, wrapType);
        }
        public static Vec GetWriteStringDimensions(MonoGameInstance mgi, string text, int maxWidth, WrapType wrapType)
        {
            return StringDrawCache.GetFormattedStringDimensions(mgi, text, new Vec(maxWidth, short.MaxValue), wrapType);
        }
        public static string GetPlainText(string text)
        {
            string output = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case Fm.INITIALISER:
                        Fm.CodeCheckResult result = Fm.CheckForCode(text, i);
                        switch (result)
                        {
                            case Fm.CodeCheckResult.None | Fm.CodeCheckResult.InsertAnimatedCell:
                                output += c;
                                break;
                        }
                        i += Fm.GetCodeLength(result) - 1;
                        break;
                    default:
                        output += c;
                        break;
                }
            }

            return output;
        }
        public static void Initialise(MonoGameInstance mgi)
        {
            Ch.Initialise(mgi, mgi.GlyphTexture, 8, 12);
        }

        // Public Methods
        public void ResetPrintTarget() => _printTarget = new Rectangle(0, 0, _charGrid.GetLength(0), _charGrid.GetLength(1));
        public void SetPrintTarget(Rectangle rectangle) => _printTarget = Vec.ConfineRectangle(rectangle, _charGrid);

        public byte GetRandomGlyph()
        {
            return GetRandomGlyph(_internalRNG);
        }
        public byte GetRandomGlyph(Random rng)
        {
            return (byte)rng.Next(0, 256);
        }
        public Color GetRandomColor()
        {
            return GetRandomColor(_internalRNG);
        }
        public Color GetRandomColor(Random rng)
        {
            return new Color((byte)rng.Next(0, 256), (byte)rng.Next(0, 256), (byte)rng.Next(0, 256));
        }

        public int GetPositionInTickCycle(MonoGameInstance mgi, params int[] intervals)
        {
            int totalTickLength = 0;
            int[] upTos = new int[intervals.Length];
            for (int i = 0; i < upTos.Length; i++)
            {
                totalTickLength += intervals[i];
                upTos[i] = totalTickLength;
            }
            int positionInInterval = mgi.Ticks % totalTickLength;
            for (int i = 0; i < upTos.Length; i++)
            {
                if (positionInInterval < upTos[i]) return i;
            }
            throw new Exception($"Something isn't correctly written in GetPositionInTickCycle. positionInInterval = {positionInInterval}");
        }

        public bool IsCursorInConsole(MonoGameInstance mgi)
        {
            return _rectangleOccupied.Contains(mgi.CursorState.Position.ToPoint());
        }
        public Vec GetCursorHoveredCellPos(MonoGameInstance mgi)
        {
            return GetCursorPositionCellPos(mgi.CursorState.Position);
        }
        public Vec GetCursorPositionCellPos(Vec position)
        {
            Vector2 relativePosition = position - Vec.GetXY(_rectangleOccupied);
            Vec returner = new Vec(relativePosition / new Vector2(Ch.CharWidth * _charScale, Ch.CharHeight * _charScale));
            return returner;
        }

        public void AddElement(IMonoGameConsoleElement mgobj)
        {
            _elements.Add(mgobj);
        }
        public void ClearElements()
        {
            _elements.Clear();
        }
        public void RemoveElement(IMonoGameConsoleElement mgobj)
        {
            _elements.Remove(mgobj);
        }

        public void Clear()
        {
            for (int y = 0; y < _charGrid.GetLength(1); y++)
            {
                for (int x = 0; x < _charGrid.GetLength(0); x++)
                {
                    _getCell(new Vec(x, y)).Reset();
                }
            }
        }

        public void PrintString(Vec location, string text, Color? foregroundColor = null, Color? backgroundColor = null)
        {
            Vec currentLocation = Vec.Clone(location);

            foreach (char c in text)
            {
                switch (c)
                {
                    case '\n':
                        currentLocation.X = location.X;
                        currentLocation.Y++;
                        break;
                    default:
                        SetCell(currentLocation, Ch.MatchChar(c), foregroundColor, backgroundColor);
                        currentLocation.X++;
                        break;
                }
            }
        }
        public void WriteString(MonoGameInstance mgi, Vec location, string text, int maxWidth, WrapType wrapType)
        {
            StringDrawCache.DrawFormattedString(mgi, this, text, location, new Vec(maxWidth, short.MaxValue), wrapType);
        }
        public void WriteString(MonoGameInstance mgi, Vec location, string text, Vec maxDimensions, WrapType wrapType)
        {
            StringDrawCache.DrawFormattedString(mgi, this, text, location, maxDimensions, wrapType);
        }
        public void WriteString(MonoGameInstance mgi, Vec location, string text)
        {
            StringDrawCache.DrawFormattedString(mgi, this, text, location);
        }

        public void Fill(Rectangle rectangle, byte? glyph = null, Color? foregroundColor = null, Color? backgroundColor = null)
        {
            foreach (Vec v in Vec.IterateOverAll(rectangle)) SetCell(v, glyph, foregroundColor, backgroundColor);
        }
        public void SetCell(Vec location, byte? glyph = null, Color? foregroundColor = null, Color? backgroundColor = null)
        {
            if (!location.IsInBounds(_printTarget)) return;

            Cell cell = _getCell(location);

            if (glyph.HasValue) cell.CharacterPositionIndicator = glyph.Value;
            if (foregroundColor.HasValue)
            {
                if (foregroundColor.Value.A == 255)
                {
                    cell.ForegroundColor = foregroundColor.Value;
                }
                else
                {
                    cell.ForegroundColor = _blendColorIntoFullColorByAlpha(cell.ForegroundColor, foregroundColor.Value);
                }
            }
            if (backgroundColor.HasValue)
            {
                if (backgroundColor.Value.A == 255)
                {
                    cell.BackgroundColor = backgroundColor.Value;
                }
                else
                {
                    cell.BackgroundColor = _blendColorIntoFullColorByAlpha(cell.BackgroundColor, backgroundColor.Value);
                }
            }
        }
        public void Box(Rectangle rectangle, Color? foregroundColor = null, Color? backgroundColor = null, bool thickBorders = false)
        {
            Box(null, rectangle, null, foregroundColor, backgroundColor, thickBorders);
        }
        public void Box(MonoGameInstance mgi, Rectangle rectangle, string title, Color? foregroundColor = null, Color? backgroundColor = null, bool thickBorders = false)
        {
            int lx = rectangle.X;
            int ly = rectangle.Y;
            int ux = rectangle.X + rectangle.Width - 1;
            int uy = rectangle.Y + rectangle.Height - 1;

            byte TL = thickBorders ? Ch.Border.n0.e2.s2.w0 : Ch.Border.n0.e1.s1.w0;
            byte T = thickBorders ? Ch.Border.n0.e2.s0.w2 : Ch.Border.n0.e1.s0.w1;
            byte TR = thickBorders ? Ch.Border.n0.e0.s2.w2 : Ch.Border.n0.e0.s1.w1;

            byte L = thickBorders ? Ch.Border.n2.e0.s2.w0 : Ch.Border.n1.e0.s1.w0;
            byte M = Ch.Space;
            byte R = thickBorders ? Ch.Border.n2.e0.s2.w0 : Ch.Border.n1.e0.s1.w0;

            byte BL = thickBorders ? Ch.Border.n2.e2.s0.w0 : Ch.Border.n1.e1.s0.w0;
            byte B = thickBorders ? Ch.Border.n0.e2.s0.w2 : Ch.Border.n0.e1.s0.w1;
            byte BR = thickBorders ? Ch.Border.n2.e0.s0.w2 : Ch.Border.n1.e0.s0.w1;

            int spaceForTitle = Math.Max(ux - lx - 3, 0);

            Fill(Vec.GetRectangle(new Vec(lx + 1, ly + 1), new Vec(ux - 1, uy - 1)), M, backgroundColor: backgroundColor);

            for (int y = ly + 1; y < uy; y++)
            {
                SetCell(new Vec(lx, y), L, foregroundColor, backgroundColor);
                SetCell(new Vec(ux, y), R, foregroundColor, backgroundColor);
            }

            for (int x = lx + 1; x < ux; x++)
            {
                SetCell(new Vec(x, uy), B, foregroundColor, backgroundColor);
                SetCell(new Vec(x, ly), T, foregroundColor, backgroundColor);
            }

            SetCell(new Vec(lx, uy), BL, foregroundColor, backgroundColor);
            SetCell(new Vec(ux, uy), BR, foregroundColor, backgroundColor);
            SetCell(new Vec(ux, ly), TR, foregroundColor, backgroundColor);
            SetCell(new Vec(lx, ly), TL, foregroundColor, backgroundColor);

            if (title != null && spaceForTitle > 0 && title.Length > 0)
            {
                WriteString(mgi, new Vec(lx + 2, ly), $"{Fm.Fg(foregroundColor.HasValue ? foregroundColor.Value : Color.White)}{Fm.Bg(backgroundColor.HasValue ? backgroundColor.Value : Color.Black)}{title}", spaceForTitle, WrapType.Cut);
            }
        }


        // Scene Methods
        public void Update(MonoGameInstance mgi)
        {
            if (_lastSeenScreenDimensions != mgi.ScreenDimensions) _transformToFitWindow(mgi);

            foreach (IMonoGameConsoleElement b in _elements)
            {
                b.Update(mgi, this);
            }
        }
        public void Draw(MonoGameInstance mgi)
        {
            foreach (IMonoGameConsoleElement b in _elements)
            {
                b.Draw(mgi, this);
            }

            mgi.GraphicsDevice.Clear(OutOfBoundsColor);

            mgi.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, null, null);

            float actualCharWidth = _charScale * Ch.CharWidth;
            float actualCharHeight = _charScale * Ch.CharHeight;

            Vec tl = Vec.GetXY(_rectangleOccupied);

            Cell matchingCell;
            int runLength;
            Vector2 startVector;

            for (int y = 0; y < _charGrid.GetLength(1); y++)
            {
                matchingCell = _getCell(new Vec(0, y));
                runLength = 1;
                startVector = tl + new Vector2(0, y * actualCharHeight);

                for (int x = 0; x < _charGrid.GetLength(0); x++)
                {
                    Cell nextCell = null;
                    if (x == _charGrid.GetLength(0) - 1 || _getCell(new Vec(x + 1, y), out nextCell).BackgroundColor != matchingCell.BackgroundColor)
                    {
                        mgi.SpriteBatch.Draw
                        (
                            Ch.CompiledGlyphTexture,
                            startVector,
                            Ch.GetRectangle(Ch.BlockFull, runLength),
                            matchingCell.BackgroundColor,
                            0f,
                            Vector2.Zero,
                            _charScale,
                            SpriteEffects.None,
                            0f
                        );

                        runLength = 1;
                        matchingCell = nextCell;
                        startVector = tl + new Vector2((x + 1) * actualCharWidth, y * actualCharHeight);
                    }
                    else
                    {
                        if (runLength == 1)
                        {
                            startVector = tl + new Vector2(x * actualCharWidth, y * actualCharHeight);
                        }
                        runLength++;
                        matchingCell = nextCell;
                    }
                }
            }

            for (int y = 0; y < _charGrid.GetLength(1); y++)
            {
                matchingCell = _getCell(new Vec(0, y));
                runLength = 1;
                startVector = tl + new Vector2(0, y * actualCharHeight);

                for (int x = 0; x < _charGrid.GetLength(0); x++)
                {
                    Cell nextCell = null;
                    if (x == _charGrid.GetLength(0) - 1 || _getCell(new Vec(x + 1, y), out nextCell).ForegroundColor != matchingCell.ForegroundColor || nextCell.CharacterPositionIndicator != matchingCell.CharacterPositionIndicator)
                    {
                        mgi.SpriteBatch.Draw
                        (
                            Ch.CompiledGlyphTexture,
                            startVector,
                            Ch.GetRectangle(matchingCell.CharacterPositionIndicator, runLength),
                            matchingCell.ForegroundColor,
                            0f,
                            Vector2.Zero,
                            _charScale,
                            SpriteEffects.None,
                            0f
                        );

                        runLength = 1;
                        matchingCell = nextCell;
                        startVector = tl + new Vector2((x + 1) * actualCharWidth, y * actualCharHeight);
                    }
                    else
                    {
                        if (runLength == 1)
                        {
                            startVector = tl + new Vector2(x * actualCharWidth, y * actualCharHeight);
                        }
                        runLength++;
                        matchingCell = nextCell;
                    }
                }
            }

            if (ClearScreenAfterDraw) Clear();

            mgi.SpriteBatch.End();
        }
    }
}
