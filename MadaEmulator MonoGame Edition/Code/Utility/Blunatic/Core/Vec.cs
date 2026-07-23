using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Blunatic.Mathematics;

namespace Blunatic.Core
{
    public struct Vec : IEnumerable<int>, IEquatable<Vec>, IComparable<Vec>, ICloneable
    {
        // Static Properties
        public static Vec Origin { get { return new Vec(0); } }
        public static Vec Zero { get { return new Vec(0); } }
        public static Vec North { get { return new Vec(0, -1); } }
        public static Vec East { get { return new Vec(1, 0); } }
        public static Vec South { get { return new Vec(0, 1); } }
        public static Vec West { get { return new Vec(-1, 0); } }

        // Properties
        /// <summary>
        /// The X ordinate.
        /// </summary>
        public int X { get; set; }
        /// <summary>
        /// The Y ordinate.
        /// </summary>
        public int Y { get; set; }

        // Constructors
        /// <summary>
        /// Creates a <see cref="Vec"/> instance with both ordinates set to 0.
        /// </summary>
        public Vec() { X = 0; Y = 0; }
        /// <summary>
        /// Creates a <see cref="Vec"/> instance with both ordinates set to the value given as <paramref name="dimensions"/>.
        /// </summary>
        /// <param name="dimensions">The value to set both X and Y ordinates to.</param>
        public Vec(int dimensions) { X = dimensions; Y = dimensions; }
        /// <summary>
        /// Creates a <see cref="Vec"/> instance with ordinates initialised to the values given as <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The value to set the X ordinate to.</param>
        /// <param name="y">The value to set the Y ordinate to.</param>
        public Vec(int x, int y) { X = x; Y = y; }
        public Vec(Vec other) { X = other.X; Y = other.Y; }
        public Vec(Microsoft.Xna.Framework.Point other) { X = other.X; Y = other.Y; }
        public Vec(Vector2 other) { X = (int)Math.Round(other.X, MidpointRounding.ToNegativeInfinity); Y = (int)Math.Round(other.Y, MidpointRounding.ToNegativeInfinity); }

        // Static Methods
        public static implicit operator Vector2(Vec input)
        {
            return input.ToVector2();
        }
        public static implicit operator Microsoft.Xna.Framework.Point(Vec input)
        {
            return input.ToPoint();
        }

        /// <summary>
        /// Returns an enumerator that iterates through <see cref="Vec"/> instances representing one movement in the directions North, East, South, West.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through <see cref="Vec"/> instances representing one movement in the directions North, East, South, West</returns>
        public static IEnumerable<Vec> IterateOverSurroundings4()
        {
            yield return new Vec(0, -1);
            yield return new Vec(1, 0);
            yield return new Vec(0, 1);
            yield return new Vec(-1, 0);
        }
        public static IEnumerable<Vec> IterateOverSurroundings8()
        {
            yield return new Vec(0, -1);
            yield return new Vec(1, -1);
            yield return new Vec(1, 0);
            yield return new Vec(1, 1);
            yield return new Vec(0, 1);
            yield return new Vec(-1, 1);
            yield return new Vec(-1, 0);
            yield return new Vec(-1, -1);
        }
        public static IEnumerable<Vec> IterateWithTranslation(Vec translation, IEnumerable<Vec> enumerable)
        {
            foreach (Vec v in enumerable) yield return v + translation;
        }
        public static IEnumerable<Vec> IterateOverAll<T>(T[,] array)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                for (int x = 0; x < array.GetLength(0); x++)
                {
                    yield return new Vec(x, y);
                }
            }
        }
        public static IEnumerable<Vec> IterateOverAll(Microsoft.Xna.Framework.Rectangle rectangle)
        {
            Vec topLeft = GetXY(rectangle);
            for (int y = 0; y < rectangle.Height; y++)
            {
                for (int x = 0; x < rectangle.Width; x++)
                {
                    yield return new Vec(x, y) + topLeft;
                }
            }
        }
        public static IEnumerable<Vec> IterateOverValid<T>(T[,] array, IEnumerable<Vec> enumerable)
        {
            foreach (Vec here in enumerable) if (here.IsInBounds(array)) yield return here;
        }
        public static IEnumerable<Vec> IterateOverValid(Microsoft.Xna.Framework.Rectangle rectangle, IEnumerable<Vec> enumerable)
        {
            foreach (Vec here in enumerable) if (here.IsInBounds(rectangle)) yield return here;
        }
        public static IEnumerable<Vec> IterateOverAdjacent4(Vec centre)
        {
            foreach (Vec v in IterateOverSurroundings4())
            {
                yield return centre + v;
            }
        }
        public static IEnumerable<Vec> IterateWithWrap(Microsoft.Xna.Framework.Rectangle rectangle, IEnumerable<Vec> enumerable)
        {
            Vec xy = GetXY(rectangle);
            Vec dimensions = GetDimensions(rectangle);

            foreach (Vec here in IterateWithTranslation(xy, enumerable)) { yield return (WrapMod(here, dimensions)) - xy; }
        }
        public static IEnumerable<Vec> IterateWithWrap<T>(T[,] array, IEnumerable<Vec> enumerable)
        {
            Vec dimensions = GetDimensions(array);

            foreach (Vec here in enumerable) { yield return (WrapMod(here, dimensions)); }
        }
        public static IEnumerable<Vec> IterateOverAdjacent8(Vec centre)
        {
            foreach (Vec v in IterateOverSurroundings8())
            {
                yield return centre + v;
            }
        }
        public static IEnumerable<Vec> IterateInRange(Vec from, double radius)
        {
            int radiusInt = (int)radius;

            int minX = from.X - radiusInt;
            int maxX = from.X + radiusInt;
            int minY = from.Y - radiusInt;
            int maxY = from.Y + radiusInt;

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vec to = new Vec(x, y);
                    if (DistanceBetween(from, to) <= radius)
                    {
                        yield return to;
                    }
                }
            }
        }
        public static IEnumerable<Vec> IterateInLine(Vec from, Vec to)
        {
            Vector2 diff = to - from;
            //double dist = DistanceBetween(from, to);

            if (diff == Zero)
            {
                yield return to;
            }
            else
            {
                VecRay vecRay = new VecRay(from, new Vector2(0.5f), diff);

                while (vecRay.GridPosition != to)
                {
                    yield return new Vec(vecRay.GridPosition);
                    vecRay = vecRay.Next();
                }
                yield return new Vec(vecRay.GridPosition);
            }
        }
        public static Microsoft.Xna.Framework.Rectangle GetRectangle(Vec corner1, Vec corner2)
        {
            int minX, minY, maxX, maxY;

            if (corner1.X < corner2.X)
            {
                minX = corner1.X;
                maxX = corner2.X;
            }
            else
            {
                minX = corner2.X;
                maxX = corner1.X;
            }

            if (corner1.Y < corner2.Y)
            {
                minY = corner1.Y;
                maxY = corner2.Y;
            }
            else
            {
                minY = corner2.Y;
                maxY = corner1.Y;
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            return new Rectangle(minX, minY, width, height);
        }
        public static Microsoft.Xna.Framework.Rectangle GetRectangle<T>(T[,] array)
        {
            return new Microsoft.Xna.Framework.Rectangle(0, 0, array.GetLength(0), array.GetLength(1));
        }
        public static Microsoft.Xna.Framework.Rectangle ConfineRectangle<T>(Microsoft.Xna.Framework.Rectangle rectangle, T[,] to)
        {
            return ConfineRectangle(rectangle, GetRectangle(to));
        }
        public static Microsoft.Xna.Framework.Rectangle ConfineRectangle(Microsoft.Xna.Framework.Rectangle rectangle, Microsoft.Xna.Framework.Rectangle to)
        {
            Vec rectTopLeft = GetXY(rectangle);
            Vec rectBottomRight = rectTopLeft + GetDimensions(rectangle);

            Vec toTopLeft = GetXY(to);
            Vec toBottomRight = toTopLeft + GetDimensions(to);

            rectTopLeft.X = Math.Max(rectTopLeft.X, toTopLeft.X);
            rectTopLeft.Y = Math.Max(rectTopLeft.Y, toTopLeft.Y);

            rectBottomRight.X = Math.Min(rectBottomRight.X, toBottomRight.X);
            rectBottomRight.Y = Math.Min(rectBottomRight.Y, toBottomRight.Y);

            if (rectTopLeft.X > rectBottomRight.X)
            {
                (rectTopLeft.X, rectBottomRight.X) = (rectBottomRight.X, rectTopLeft.X);
            }
            if (rectTopLeft.Y > rectBottomRight.Y)
            {
                (rectTopLeft.Y, rectBottomRight.Y) = (rectBottomRight.Y, rectTopLeft.Y);
            }

            Microsoft.Xna.Framework.Rectangle output = new Microsoft.Xna.Framework.Rectangle(rectTopLeft, rectBottomRight - rectTopLeft);

            return output;
        }
        public static void SetAll<T>(T[,] array, T value)
        {
            foreach (Vec v in IterateOverAll(array)) v.SetAt(array, value);
        }
        public static void SetAll<T>(T[,] array, Func<T> func)
        {
            foreach (Vec v in IterateOverAll(array)) v.SetAt(array, func());
        }
        public static T[,] InitialiseArray<T>(int width, int height, T initialValue)
        {
            T[,] array = new T[width, height];
            SetAll(array, initialValue);
            return array;
        }
        public static T[,] InitialiseArray<T>(int width, int height, Func<T> initialValueFunc)
        {
            T[,] array = new T[width, height];
            SetAll(array, initialValueFunc);
            return array;
        }
        public static T[,] InitialiseArray<T>(Vec dimensions, T initialValue)
        {
            return InitialiseArray(dimensions.X, dimensions.Y, initialValue);
        }
        public static T[,] InitialiseArray<T>(Vec dimensions, Func<T> initialValueFunc)
        {
            return InitialiseArray(dimensions.X, dimensions.Y, initialValueFunc);
        }
        /// <summary>
        /// Returns a new instance of the given <see cref="Vec"/> instance. No changes are made.
        /// </summary>
        /// <param name="vec">The given <see cref="Vec"/> instance.</param>
        /// <returns>A new instance of the given <see cref="Vec"/> instance.</returns>
        public static Vec operator +(Vec vec) => vec;
        /// <summary>
        /// Returns a new <see cref="Vec"/> instance representing the additive inverse of the given <see cref="Vec"/> instance.
        /// </summary>
        /// <param name="vec">The given <see cref="Vec"/> instance.</param>
        /// <returns>A new <see cref="Vec"/> instance representing the additive inverse of the given <see cref="Vec"/> instance.</returns>
        public static Vec operator -(Vec vec) => -1 * vec;
        /// <summary>
        /// Returns a new <see cref="Vec"/> instance representing the sum of the two given <see cref="Vec"/> instances.
        /// </summary>
        /// <param name="a">The first of the given <see cref="Vec"/> instances.</param>
        /// <param name="b">The second of the given <see cref="Vec"/> instances.</param>
        /// <returns>A new <see cref="Vec"/> instance representing the sum of the two given <see cref="Vec"/> instances.</returns>
        public static Vec operator +(Vec a, Vec b) => new Vec(a.X + b.X, a.Y + b.Y);
        /// <summary>
        /// Returns a new <see cref="Vec"/> instance representing the second given <see cref="Vec"/> instance (<paramref name="b"/>) subtracted from the first (<paramref name="a"/>).
        /// </summary>
        /// <param name="a">The first given <see cref="Vec"/> instance, which is subtracted from.</param>
        /// <param name="b">The second given <see cref="Vec"/> instance, which is subtracted from the first (<paramref name="a"/>).</param>
        /// <returns></returns>
        public static Vec operator -(Vec a, Vec b) => new Vec(a.X - b.X, a.Y - b.Y);
        /// <summary>
        /// Returns a new <see cref="Vec"/> instance representing the sum of the given <see cref="Vec"/> instance (<paramref name="a"/>) and a <see cref="Vec"/> with both ordinates set to the value given as <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The given <see cref="Vec"/> instance.</param>
        /// <param name="b">The value of each ordinate in the <see cref="Vec"/> being added to <paramref name="a"/>.</param>
        /// <returns>A new <see cref="Vec"/> instance representing the sum of the given <see cref="Vec"/> instance (<paramref name="a"/>) and a <see cref="Vec"/> with both ordinates set to the value given as <paramref name="b"/>.</returns>
        public static Vec operator +(Vec a, int b) => new Vec(a.X + b, a.Y + b);
        /// <summary>
        /// Returns a new <see cref="Vec"/> instance representing a <see cref="Vec"/> with both ordinates set to the value given as <paramref name="b"/> subtracted from the given <see cref="Vec"/> instance (<paramref name="a"/>).
        /// </summary>
        /// <param name="a">The first given <see cref="Vec"/> instance, which is subtracted from.</param>
        /// <param name="b">The value of each ordinate in the <see cref="Vec"/> subtracted from <paramref name="a"/>.</param>
        /// <returns>A new <see cref="Vec"/> instance representing a <see cref="Vec"/> with both ordinates set to the value given as <paramref name="b"/> subtracted from the given <see cref="Vec"/> instance (<paramref name="a"/>).</returns>
        public static Vec operator -(Vec a, int b) => new Vec(a.X - b, a.Y - b);
        public static Vec operator +(int a, Vec b) => new Vec(a + b.X, a + b.Y);
        public static Vec operator -(int a, Vec b) => new Vec(a - b.X, a - b.Y);
        public static Vec operator *(Vec a, Vec b) => new Vec(a.X * b.X, a.Y * b.Y);
        public static Vec operator *(int a, Vec b) => new Vec(a * b.X, a * b.Y);
        public static Vec operator *(Vec a, int b) => new Vec(a.X * b, a.Y * b);
        public static Vec operator /(Vec a, Vec b) => new Vec(a.X / b.X, a.Y / b.Y);
        public static Vec operator /(Vec a, int b) => new Vec(a.X / b, a.Y / b);
        public static Vec operator /(int a, Vec b) => new Vec(a / b.X, a / b.Y);
        public static Vec operator %(Vec a, Vec b) => new Vec(a.X % b.X, a.Y % b.Y);
        public static Vec operator %(Vec a, int b) => new Vec(a.X % b, a.Y % b);
        public static Vec operator %(int a, Vec b) => new Vec(a % b.X, a % b.Y);
        public static Vec operator *(double a, Vec b) => new Vec((int)(a * b.X), (int)(a * b.Y));
        public static Vec operator *(Vec a, double b) => new Vec((int)(a.X * b), (int)(a.Y * b));
        public static Vec operator /(Vec a, double b) => new Vec((int)(a.X / b), (int)(a.Y / b));
        public static Vec operator /(double a, Vec b) => new Vec((int)(a / b.X), (int)(a / b.Y));
        public static bool operator ==(Vec a, Vec b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vec a, Vec b) => !(a == b);

        public static Vec ToNorth(Vec vec) => ToNorth(vec, 1);
        public static Vec ToSouth(Vec vec) => ToSouth(vec, 1);
        public static Vec ToWest(Vec vec) => ToWest(vec, 1);
        public static Vec ToEast(Vec vec) => ToEast(vec, 1);
        public static Vec ToNorth(Vec vec, int amount) => vec + new Vec(0, -amount);
        public static Vec ToSouth(Vec vec, int amount) => vec + new Vec(0, amount);
        public static Vec ToWest(Vec vec, int amount) => vec + new Vec(-amount, 0);
        public static Vec ToEast(Vec vec, int amount) => vec + new Vec(amount, 0);
        public static Vec GetXY(Microsoft.Xna.Framework.Rectangle rectangle) => new Vec(rectangle.X, rectangle.Y);
        public static Vec GetDimensions(Microsoft.Xna.Framework.Rectangle rectangle) => new Vec(rectangle.Width, rectangle.Height);
        public static Vec GetDimensions<T>(T[,] array) => new Vec(array.GetLength(0), array.GetLength(1));
        public static int Dot(Vec a, Vec b) => a.X * b.X + a.Y * b.Y;
        public static Vec WrapMod(Vec a, int b) => new Vec(Calc.WrapMod(a.X, b), Calc.WrapMod(a.Y, b));
        public static Vec WrapMod(int a, Vec b) => new Vec(Calc.WrapMod(a, b.X), Calc.WrapMod(a, b.Y));
        public static Vec WrapMod(Vec a, Vec b) => new Vec(Calc.WrapMod(a.X, b.X), Calc.WrapMod(a.Y, b.Y));
        public static double Magnitude(Vec vec) => Math.Sqrt(Math.Pow(vec.X, 2) + Math.Pow(vec.Y, 2));
        public static double DistanceBetween(Vec a, Vec b) => Magnitude(OffsetBetween(a, b));
        public static Vec ConvexCombine(Vec a, Vec b, double t) => b * t + a * (1 - t);
        public static int ManhattanDistanceBetween(Vec a, Vec b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        public static int LargestOrdinateMagnitude(Vec vec) => Math.Max(Math.Abs(vec.X), Math.Abs(vec.Y));
        public static double AngleBetween(Vec a, Vec b) => Math.Acos(Dot(a, b) / (Magnitude(a) * Magnitude(b)));
        public static Vec OffsetBetween(Vec a, Vec b) => b - a;
        public static Vec Unitize(Vec vec) => new Vec(Math.Max(Math.Min(1, vec.X), -1), Math.Max(Math.Min(1, vec.Y), -1));
        public static Vec Rotate90AnticlockwiseAboutOrigin(Vec vec) => new Vec(vec.Y, -vec.X);
        public static Vec Rotate90ClockwiseAboutOrigin(Vec vec) => new Vec(-vec.Y, vec.X);
        public static Vec ConstrainTo(Vec vec, int width, int height) => new Vec(Math.Max(Math.Min(vec.X, width - 1), 0), Math.Max(Math.Min(vec.Y, height - 1), 0));
        public static Vec ConstrainTo<T>(Vec vec, T[,] array) => new Vec(Math.Max(Math.Min(vec.X, array.GetLength(0) - 1), 0), Math.Max(Math.Min(vec.Y, array.GetLength(1) - 1), 0));
        public static Vec Clone(Vec vec) => (Vec)vec.Clone();

        // Methods
        public readonly override string ToString() => $"({X}, {Y})";
        public readonly bool Equals(Vec vector) => this == vector;
        public override readonly bool Equals(object o) => o is Vec v && this == v;
        public readonly override int GetHashCode() => HashCode.Combine(X, Y);
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public readonly IEnumerator<int> GetEnumerator()
        {
            yield return X;
            yield return Y;
        }
        public readonly int CompareTo(Vec other) => Magnitude().CompareTo(other.Magnitude());
        public readonly object Clone() => new Vec(X, Y);

        /// <summary>
        /// Retrieves the value at the position in <paramref name="array"/> described by the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the contents of <paramref name="array"/>.</typeparam>
        /// <param name="array">The two-dimensional array to retrieve a value from.</param>
        /// <returns>The value at the position in <paramref name="array"/> described by the current instance.</returns>
        public readonly T GetAt<T>(T[,] array) => array[X, Y];
        public readonly void SetAt<T>(T[,] array, T value) => array[X, Y] = value;
        /// <summary>
        /// Retrieves the value at the position in <paramref name="array"/> described by the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the contents of <paramref name="array"/>.</typeparam>
        /// <param name="array">The nested array to retrieve a value from.</param>
        /// <returns>The value at the position in <paramref name="array"/> described by the current instance.</returns>
        public readonly T GetAt<T>(T[][] array) => array[X][Y];
        public readonly void SetAt<T>(T[][] array, T value) => array[X][Y] = value;
        /// <summary>
        /// Retrieves the value at the position in <paramref name="array"/> described by the current instance, by accessing the string from the array with the Y ordinate, and the position in the string with the X ordinate.
        /// </summary>
        /// <param name="array">The array of strings to retrieve a <see langword="char"/> from.</param>
        /// <returns>The value at the position in <paramref name="array"/> described by the current instance.</returns>
        public readonly char GetAt(string[] array) => array[Y][X];
        public readonly bool IsInBounds<T>(T[,] array) => X >= 0 && Y >= 0 && X < array.GetLength(0) && Y < array.GetLength(1);
        public readonly bool IsInBounds(Vec dimensions) => IsInBounds(dimensions.X, dimensions.Y);
        public readonly bool IsInBounds(int x, int y) => Y >= 0 && X >= 0 && Y < y && X < x;
        public readonly bool IsInBounds(Microsoft.Xna.Framework.Rectangle rectangle) => rectangle.Contains(ToPoint());
        public readonly int DistanceFromEdge<T>(T[,] array) => Math.Min(Math.Min(X, Y), Math.Min(array.GetLength(0) - X - 1, array.GetLength(1) - Y - 1));
        public readonly bool IsOrthogonal() => X == 0 || Y == 0;
        /// <summary>
        /// Indicates whether the current instance's ordinates are of the same magnitude.
        /// </summary>
        /// <returns><see langword="true"/> if the current instance's ordinates are of the same magnitude; otherwise <see langword="false"/>.</returns>
        public readonly bool IsDiagonal() => Math.Abs(X) == Math.Abs(Y);
        /// <summary>
        /// Returns the magnitude of the current instance, using Pythagoras' Theorem.
        /// </summary>
        /// <returns>The magnitude of the current instance.</returns>
        public readonly double Magnitude() => Magnitude(this);
        /// <summary>
        /// Returns the magnitude of the largest ordinate in the current instance.
        /// </summary>
        /// <returns>The magnitude of the largest ordinate in the current instance.</returns>
        public readonly int GetLargestOrdinateMagnitude() => LargestOrdinateMagnitude(this);
        public readonly Vector2 ToVector2() => new Vector2(X, Y);
        public readonly Microsoft.Xna.Framework.Point ToPoint() => new Microsoft.Xna.Framework.Point(X, Y);
        public readonly bool IsAdjacentTo<T>(T[,] array, T value)
        {
            foreach (Vec adj in IterateOverValid(array, (IterateOverAdjacent4(this))))
            {
                if (value.Equals(adj.GetAt(array))) return true;
            }
            return false;
        }
        public readonly bool IsOnOrAdjacentTo<T>(T[,] array, T value)
        {
            if (GetAt(array).Equals(value)) return true;
            foreach (Vec adj in IterateOverValid(array, (IterateOverAdjacent4(this))))
            {
                if (value.Equals(adj.GetAt(array))) return true;
            }
            return false;
        }
        public readonly bool IsOnBorder<T>(T[,] array) => IsOnBorder(GetRectangle(array));
        public readonly bool IsOnBorder(Microsoft.Xna.Framework.Rectangle rectangle) => X == rectangle.X || Y == rectangle.Y || X == rectangle.Right - 1 || Y == rectangle.Bottom - 1;
    }
}
