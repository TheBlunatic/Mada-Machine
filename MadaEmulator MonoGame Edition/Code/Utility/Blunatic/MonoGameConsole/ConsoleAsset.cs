using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blunatic.Parsing;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public class ConsoleAsset
    {
        // Structs
        public struct Cell : IEquatable<Cell>
        {
            public const int SERIALIZED_LENGTH = 15;

            public byte Glyph;
            public Color ForegroundColor;
            public Color BackgroundColor;
            public bool Mask;

            public Cell()
            {
                Glyph = Ch.Space;
                ForegroundColor = Color.White;
                BackgroundColor = Color.Black;
                Mask = false;
            }
            public Cell(byte glyph, Color foregroundColor, Color backgroundColor, bool mask)
            {
                Glyph = glyph;
                ForegroundColor = foregroundColor;
                BackgroundColor = backgroundColor;
                Mask = mask;
            }
            public Cell(string serializedCellInfo)
            {
                string glyphPart = serializedCellInfo.Substring(0, 2);
                string foregroundColorPart = serializedCellInfo.Substring(2, 6);
                string backgroundColorPart = serializedCellInfo.Substring(8, 6);
                string maskPart = serializedCellInfo.Substring(14, 1);

                Glyph = Hex.GetBytes(glyphPart)[0];
                ForegroundColor = Hex.GetColor(foregroundColorPart);
                BackgroundColor = Hex.GetColor(backgroundColorPart);
                Mask = maskPart == "1";
            }

            public static bool operator ==(Cell a, Cell b)
            {
                return a.Glyph == b.Glyph && a.ForegroundColor == b.ForegroundColor && a.Mask == b.Mask && a.BackgroundColor == b.BackgroundColor;
            }
            public static bool operator !=(Cell a, Cell b) => !(a == b);
            public readonly bool Equals(Cell other) => this == other;
            public override readonly bool Equals(object o) => o is Cell ci && this == ci;
            public readonly override int GetHashCode() => HashCode.Combine(Glyph, Mask, ForegroundColor, BackgroundColor);

            public readonly bool EqualsExcludingMask(Cell other)
            {
                return this.Glyph == other.Glyph && this.ForegroundColor == other.ForegroundColor && this.BackgroundColor == other.BackgroundColor;
            }

            public string Serialize()
            {
                return $"{Hex.GetString(this.Glyph)}{Hex.GetString(this.ForegroundColor)}{Hex.GetString(this.BackgroundColor)}{(this.Mask ? "1" : "0")}";
            }
        }

        // Fields
        public Cell[,] Canvas;
        public string Name;

        // Constructors
        public ConsoleAsset(Vec dimensions)
        {
            Canvas = new Cell[dimensions.X, dimensions.Y];
            Vec.SetAll(Canvas, new Cell());
            Name = "asset.mgca";
        }
        public ConsoleAsset(string blueprint)
        {
            Canvas = new Cell[Hex.GetBytes(blueprint.Substring(0, 2))[0], (blueprint.Length - 2) / (Cell.SERIALIZED_LENGTH * Hex.GetBytes(blueprint.Substring(0, 2))[0])];
            int i = 0;
            foreach (Vec v in Vec.IterateOverAll(Canvas))
            {
                v.SetAt(Canvas, new Cell(blueprint.Substring(2 + i * Cell.SERIALIZED_LENGTH, Cell.SERIALIZED_LENGTH)));
                i++;
            }
            Name = blueprint.Substring(Cell.SERIALIZED_LENGTH * Canvas.Length + 2);
        }
        public ConsoleAsset(ConsoleAsset selectFrom, Rectangle selection)
        {
            Canvas = new Cell[selection.Width, selection.Height];
            Vec.SetAll(Canvas, new Cell());
            Name = $"{selectFrom}({selection.X},{selection.Y},{selection.Width},{selection.Height})";
            foreach (Vec v in Vec.IterateOverValid(selectFrom.Canvas, Vec.IterateOverAll(selection)))
            {
                (v - Vec.GetXY(selection)).SetAt(Canvas, v.GetAt(selectFrom.Canvas));
            }
        }

        // Methods
        public string Serialize()
        {
            string output = string.Empty;

            output += Hex.GetString((byte)Canvas.GetLength(0));

            foreach (Vec v in Vec.IterateOverAll(Canvas))
            {
                output += v.GetAt(Canvas).Serialize();
            }

            output += Name;

            return output;
        }
        public void Draw(MonoGameConsole mgc, Vec location, bool applyMask = true)
        {
            Draw(mgc, location, new Rectangle(0, 0, int.MaxValue, int.MaxValue));
        }
        public void Draw(MonoGameConsole mgc, Vec location, Rectangle bounds, bool applyMask = true)
        {
            foreach (Vec v in Vec.IterateOverAll(Canvas))
            {
                Vec screenLocation = v + location;
                if (!screenLocation.IsInBounds(bounds)) continue;
                if (!applyMask || !v.GetAt(Canvas).Mask)
                {
                    mgc.SetCell(screenLocation, v.GetAt(Canvas).Glyph, v.GetAt(Canvas).ForegroundColor, v.GetAt(Canvas).BackgroundColor);
                }
            }
        }
    }
}
