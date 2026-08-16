using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public static class Ch
    {
        public const byte Space = 0x00;
        public const byte Smiley = 0x01;
        public const byte Box_s = 0x02;
        public const byte Heart = 0x03;
        public const byte Diamond = 0x04;
        public const byte Club = 0x05;
        public const byte Spade = 0x06;
        public const byte Bullet = 0x07;
        public const byte Box_W = 0x08;
        public const byte Circle = 0x09;
        public const byte Box_E = 0x0A;
        public const byte Male = 0x0B;
        public const byte Female = 0x0C;
        public const byte Quaver = 0x0D;
        public const byte QuaverDouble = 0x0E;
        public const byte Solar = 0x0F;
        public const byte TriangleRight = 0x10;
        public const byte TriangleLeft = 0x11;
        public const byte ArrowUpDown = 0x12;
        public const byte ExclamationMarkDouble = 0x13;
        public const byte Pilcrow = 0x14;
        public const byte Section = 0x15;
        public const byte RectangleHorizontal = 0x16;
        public const byte Box_S = 0x17;
        public const byte ArrowUp = 0x18;
        public const byte ArrowDown = 0x19;
        public const byte ArrowRight = 0x1A;
        public const byte ArrowLeft = 0x1B;
        public const byte RightAngle = 0x1C;
        public const byte ArrowLeftRight = 0x1D;
        public const byte TriangleUp = 0x1E;
        public const byte TriangleDown = 0x1F;
        public const byte Union = 0x20;
        public const byte ExclamationMark = 0x21;
        public const byte QuotationMark = 0x22;
        public const byte Number = 0x23;
        public const byte Dollar = 0x24;
        public const byte Percent = 0x25;
        public const byte Ampersand = 0x26;
        public const byte Apostrophe = 0x27;
        public const byte ParenthesisOpen = 0x28;
        public const byte ParenthesisClose = 0x29;
        public const byte Asterisk = 0x2A;
        public const byte Plus = 0x2B;
        public const byte Comma = 0x2C;
        public const byte Minus = 0x2D;
        public const byte Period = 0x2E;
        public const byte Slash = 0x2F;
        public const byte Zero = 0x30;
        public const byte One = 0x31;
        public const byte Two = 0x32;
        public const byte Three = 0x33;
        public const byte Four = 0x34;
        public const byte Five = 0x35;
        public const byte Six = 0x36;
        public const byte Seven = 0x37;
        public const byte Eight = 0x38;
        public const byte Nine = 0x39;
        public const byte Colon = 0x3A;
        public const byte Semicolon = 0x3B;
        public const byte LessThan = 0x3C;
        public const byte EqualTo = 0x3D;
        public const byte GreaterThan = 0x3E;
        public const byte QuestionMark = 0x3F;
        public const byte At = 0x40;
        public const byte A = 0x41;
        public const byte B = 0x42;
        public const byte C = 0x43;
        public const byte D = 0x44;
        public const byte E = 0x45;
        public const byte F = 0x46;
        public const byte G = 0x47;
        public const byte H = 0x48;
        public const byte I = 0x49;
        public const byte J = 0x4A;
        public const byte K = 0x4B;
        public const byte L = 0x4C;
        public const byte M = 0x4D;
        public const byte N = 0x4E;
        public const byte O = 0x4F;
        public const byte P = 0x50;
        public const byte Q = 0x51;
        public const byte R = 0x52;
        public const byte S = 0x53;
        public const byte T = 0x54;
        public const byte U = 0x55;
        public const byte V = 0x56;
        public const byte W = 0x57;
        public const byte X = 0x58;
        public const byte Y = 0x59;
        public const byte Z = 0x5A;
        public const byte BracketOpen = 0x5B;
        public const byte BackSlash = 0x5C;
        public const byte BracketClose = 0x5D;
        public const byte Caret = 0x5E;
        public const byte Underscore = 0x5F;
        public const byte Grave = 0x60;
        public const byte ALower = 0x61;
        public const byte BLower = 0x62;
        public const byte CLower = 0x63;
        public const byte DLower = 0x64;
        public const byte ELower = 0x65;
        public const byte FLower = 0x66;
        public const byte GLower = 0x67;
        public const byte HLower = 0x68;
        public const byte ILower = 0x69;
        public const byte JLower = 0x6A;
        public const byte KLower = 0x6B;
        public const byte LLower = 0x6C;
        public const byte MLower = 0x6D;
        public const byte NLower = 0x6E;
        public const byte OLower = 0x6F;
        public const byte PLower = 0x70;
        public const byte QLower = 0x71;
        public const byte RLower = 0x72;
        public const byte SLower = 0x73;
        public const byte TLower = 0x74;
        public const byte ULower = 0x75;
        public const byte VLower = 0x76;
        public const byte WLower = 0x77;
        public const byte XLower = 0x78;
        public const byte YLower = 0x79;
        public const byte ZLower = 0x7A;
        public const byte BraceOpen = 0x7B;
        public const byte Pipe = 0x7C;
        public const byte BraceClose = 0x7D;
        public const byte Tilde = 0x7E;
        public const byte House = 0x7F;
        public const byte CCedilla = 0x80;
        public const byte ULowerDiaeresis = 0x81;
        public const byte ELowerAcute = 0x82;
        public const byte ALowerCaret = 0x83;
        public const byte ALowerDiaeresis = 0x84;
        public const byte ALowerGrave = 0x85;
        public const byte ALowerRing = 0x86;
        public const byte CLowerCedilla = 0x87;
        public const byte ELowerCaret = 0x88;
        public const byte ELowerDiaeresis = 0x89;
        public const byte ELowerGrave = 0x8A;
        public const byte ILowerDiaeresis = 0x8B;
        public const byte ILowerCaret = 0x8C;
        public const byte ILowerGrave = 0x8D;
        public const byte ADiaeresis = 0x8E;
        public const byte ARing = 0x8F;
        public const byte EAcute = 0x90;
        public const byte AELower = 0x91;
        public const byte AE = 0x92;
        public const byte OLowerCaret = 0x93;
        public const byte OLowerDiaeresis = 0x94;
        public const byte OLowerGrave = 0x95;
        public const byte ULowerCaret = 0x96;
        public const byte ULowerGrave = 0x97;
        public const byte YLowerDiaeresis = 0x98;
        public const byte ODiaeresis = 0x99;
        public const byte UDiaeresis = 0x9A;
        public const byte Cent = 0x9B;
        public const byte Pound = 0x9C;
        public const byte Yen = 0x9D;
        public const byte Points = 0x9E;
        public const byte FLowerHook = 0x9F;
        public const byte ALowerAcute = 0xA0;
        public const byte ILowerAcute = 0xA1;
        public const byte OLowerAcute = 0xA2;
        public const byte ULowerAcute = 0xA3;
        public const byte NLowerTilde = 0xA4;
        public const byte NTilde = 0xA5;
        public const byte OrdinalFem = 0xA6;
        public const byte OrdinalMasc = 0xA7;
        public const byte QuestionMarkFlipped = 0xA8;
        public const byte Box_N = 0xA9;
        public const byte Box_n = 0xAA;
        public const byte Half = 0xAB;
        public const byte Quarter = 0xAC;
        public const byte ExclamationMarkFlipped = 0xAD;
        public const byte AngleBraceOpen = 0xAE;
        public const byte AngleBraceClose = 0xAF;
        public const byte BlockLight = 0xB0;
        public const byte BlockMedium = 0xB1;
        public const byte BlockHeavy = 0xB2;
        private const byte Box_ns = 0xB3;
        private const byte Box_nws = 0xB4;
        private const byte Box_nWs = 0xB5;
        private const byte Box_NwS = 0xB6;
        private const byte Box_wS = 0xB7;
        private const byte Box_Ws = 0xB8;
        private const byte Box_NWS = 0xB9;
        private const byte Box_NS = 0xBA;
        private const byte Box_WS = 0xBB;
        private const byte Box_NW = 0xBC;
        private const byte Box_Nw = 0xBD;
        private const byte Box_nW = 0xBE;
        private const byte Box_ws = 0xBF;
        private const byte Box_ne = 0xC0;
        private const byte Box_new = 0xC1;
        private const byte Box_ews = 0xC2;
        private const byte Box_nes = 0xC3;
        private const byte Box_ew = 0xC4;
        private const byte Box_news = 0xC5;
        private const byte Box_nEs = 0xC6;
        private const byte Box_NeS = 0xC7;
        private const byte Box_NE = 0xC8;
        private const byte Box_ES = 0xC9;
        private const byte Box_NEW = 0xCA;
        private const byte Box_EWS = 0xCB;
        private const byte Box_NES = 0xCC;
        private const byte Box_EW = 0xCD;
        private const byte Box_NEWS = 0xCE;
        private const byte Box_nEW = 0xCF;
        private const byte Box_New = 0xD0;
        private const byte Box_EWs = 0xD1;
        private const byte Box_ewS = 0xD2;
        private const byte Box_Ne = 0xD3;
        private const byte Box_nE = 0xD4;
        private const byte Box_Es = 0xD5;
        private const byte Box_eS = 0xD6;
        private const byte Box_NewS = 0xD7;
        private const byte Box_nEWs = 0xD8;
        private const byte Box_nw = 0xD9;
        private const byte Box_es = 0xDA;
        public const byte BlockFull = 0xDB;
        public const byte BlockHalfBottom = 0xDC;
        public const byte BlockHalfLeft = 0xDD;
        public const byte BlockHalfRight = 0xDE;
        public const byte BlockHalfTop = 0xDF;
        public const byte Alpha = 0xE0;
        public const byte Beta = 0xE1;
        public const byte Gamma = 0xE2;
        public const byte Pi = 0xE3;
        public const byte Sigma = 0xE4;
        public const byte SigmaLower = 0xE5;
        public const byte Mu = 0xE6;
        public const byte Tau = 0xE7;
        public const byte Phi = 0xE8;
        public const byte Theta = 0xE9;
        public const byte Omega = 0xEA;
        public const byte Delta = 0xEB;
        public const byte Lemniscate = 0xEC;
        public const byte PhiLower = 0xED;
        public const byte Box_w = 0xEE;
        public const byte Intersect = 0xEF;
        public const byte Tribar = 0xF0;
        public const byte PlusMinus = 0xF1;
        public const byte GreaterThanOrEqualTo = 0xF2;
        public const byte LessThanOrEqualTo = 0xF3;
        public const byte IntegralTop = 0xF4;
        public const byte IntegralBottom = 0xF5;
        public const byte Divide = 0xF6;
        public const byte EqualToRoughly = 0xF7;
        public const byte Degrees = 0xF8;
        public const byte BulletSmall = 0xF9;
        public const byte BulletTiny = 0xFA;
        public const byte Root = 0xFB;
        public const byte QuestionMarkInverted = 0xFC;
        public const byte Box_e = 0xFD;
        public const byte RectangleVertical = 0xFE;
        public const byte Overscore = 0xFF;

        public static class Border
        {
            public static class n0
            {
                public static class e0
                {
                    public static class s0
                    {
                        public const byte w1 = Box_w;
                        public const byte w2 = Box_W;
                    }
                    public static class s1
                    {
                        public const byte w0 = Box_s;
                        public const byte w1 = Box_ws;
                        public const byte w2 = Box_Ws;
                    }
                    public static class s2
                    {
                        public const byte w0 = Box_S;
                        public const byte w1 = Box_wS;
                        public const byte w2 = Box_WS;
                    }
                }
                public static class e1
                {
                    public static class s0
                    {
                        public const byte w0 = Box_e;
                        public const byte w1 = Box_ew;
                    }
                    public static class s1
                    {
                        public const byte w0 = Box_es;
                        public const byte w1 = Box_ews;
                    }
                    public static class s2
                    {
                        public const byte w0 = Box_eS;
                        public const byte w1 = Box_ewS;
                    }
                }
                public static class e2
                {
                    public static class s0
                    {
                        public const byte w0 = Box_E;
                        public const byte w2 = Box_EW;
                    }
                    public static class s1
                    {
                        public const byte w0 = Box_Es;
                        public const byte w2 = Box_EWs;
                    }
                    public static class s2
                    {
                        public const byte w0 = Box_ES;
                        public const byte w2 = Box_EWS;
                    }
                }
            }
            public static class n1
            {
                public static class e0
                {
                    public static class s0
                    {
                        public const byte w0 = Box_n;
                        public const byte w1 = Box_nw;
                        public const byte w2 = Box_nW;
                    }
                    public static class s1
                    {
                        public const byte w0 = Box_ns;
                        public const byte w1 = Box_nws;
                        public const byte w2 = Box_nWs;
                    }
                }
                public static class e1
                {
                    public static class s0
                    {
                        public const byte w0 = Box_ne;
                        public const byte w1 = Box_new;
                    }
                    public static class s1
                    {
                        public const byte w0 = Box_nes;
                        public const byte w1 = Box_news;
                    }
                }
                public static class e2
                {
                    public static class s0
                    {
                        public const byte w0 = Box_nE;
                        public const byte w2 = Box_nEW;
                    }
                    public static class s1
                    {
                        public const byte w0 = Box_nEs;
                        public const byte w2 = Box_nEWs;
                    }
                }
            }
            public static class n2
            {
                public static class e0
                {
                    public static class s0
                    {
                        public const byte w0 = Box_N;
                        public const byte w1 = Box_Nw;
                        public const byte w2 = Box_NW;
                    }
                    public static class s2
                    {
                        public const byte w0 = Box_NS;
                        public const byte w1 = Box_NwS;
                        public const byte w2 = Box_NWS;
                    }
                }
                public static class e1
                {
                    public static class s0
                    {
                        public const byte w0 = Box_Ne;
                        public const byte w1 = Box_New;
                    }
                    public static class s2
                    {
                        public const byte w0 = Box_NeS;
                        public const byte w1 = Box_NewS;
                    }
                }
                public static class e2
                {
                    public static class s0
                    {
                        public const byte w0 = Box_NE;
                        public const byte w2 = Box_NEW;
                    }
                    public static class s2
                    {
                        public const byte w0 = Box_NES;
                        public const byte w2 = Box_NEWS;
                    }
                }
            }

            private static Dictionary<string, byte> _borderStringToByte;
            private static Dictionary<byte, string> _byteToBorderString;

            public static bool TryGetBorder(byte north, byte east, byte south, byte west, out byte glyph)
            {
                return _borderStringToByte.TryGetValue($"{north}{east}{south}{west}", out glyph);
            }
            public static bool TryGetBorder(bool north, bool east, bool south, bool west, out byte glyph, bool doubleBorder = false, bool allowIslandSubstitutes = true)
            {
                if (allowIslandSubstitutes && !north && !east && !south && !west)
                {
                    if (doubleBorder) glyph = Circle;
                    else glyph = BulletTiny;
                    return true;
                }
                else if (doubleBorder) return _borderStringToByte.TryGetValue($"{(north ? 2 : 0)}{(east ? 2 : 0)}{(south ? 2 : 0)}{(west ? 2 : 0)}", out glyph);
                else return _borderStringToByte.TryGetValue($"{(north ? 1 : 0)}{(east ? 1 : 0)}{(south ? 1 : 0)}{(west ? 1 : 0)}", out glyph);
            }

            public static void Initialise()
            {
                _borderStringToByte = new Dictionary<string, byte>()
                {
                    {"0001", n0.e0.s0.w1},
                    {"0002", n0.e0.s0.w2},
                    {"0010", n0.e0.s1.w0},
                    {"0011", n0.e0.s1.w1},
                    {"0012", n0.e0.s1.w2},
                    {"0020", n0.e0.s2.w0},
                    {"0021", n0.e0.s2.w1},
                    {"0022", n0.e0.s2.w2},
                    {"0100", n0.e1.s0.w0},
                    {"0101", n0.e1.s0.w1},
                    {"0110", n0.e1.s1.w0},
                    {"0111", n0.e1.s1.w1},
                    {"0120", n0.e1.s2.w0},
                    {"0121", n0.e1.s2.w1},
                    {"0200", n0.e2.s0.w0},
                    {"0202", n0.e2.s0.w2},
                    {"0210", n0.e2.s1.w0},
                    {"0212", n0.e2.s1.w2},
                    {"0220", n0.e2.s2.w0},
                    {"0222", n0.e2.s2.w2},
                    {"1000", n1.e0.s0.w0},
                    {"1001", n1.e0.s0.w1},
                    {"1002", n1.e0.s0.w2},
                    {"1010", n1.e0.s1.w0},
                    {"1011", n1.e0.s1.w1},
                    {"1012", n1.e0.s1.w2},
                    {"1100", n1.e1.s0.w0},
                    {"1101", n1.e1.s0.w1},
                    {"1110", n1.e1.s1.w0},
                    {"1111", n1.e1.s1.w1},
                    {"1200", n1.e2.s0.w0},
                    {"1202", n1.e2.s0.w2},
                    {"1210", n1.e2.s1.w0},
                    {"1212", n1.e2.s1.w2},
                    {"2000", n2.e0.s0.w0},
                    {"2001", n2.e0.s0.w1},
                    {"2002", n2.e0.s0.w2},
                    {"2020", n2.e0.s2.w0},
                    {"2021", n2.e0.s2.w1},
                    {"2022", n2.e0.s2.w2},
                    {"2100", n2.e1.s0.w0},
                    {"2101", n2.e1.s0.w1},
                    {"2120", n2.e1.s2.w0},
                    {"2121", n2.e1.s2.w1},
                    {"2200", n2.e2.s0.w0},
                    {"2202", n2.e2.s0.w2},
                    {"2220", n2.e2.s2.w0},
                    {"2222", n2.e2.s2.w2},
                };
                _byteToBorderString = new Dictionary<byte, string>();
                foreach (KeyValuePair<string, byte> kvp in _borderStringToByte)
                {
                    _byteToBorderString.Add(kvp.Value, kvp.Key);
                }
            }

            public static bool IsBorder(byte glyph)
            {
                return _byteToBorderString.ContainsKey(glyph);
            }
            public static byte RotateIfIs(byte glyph, bool clockwise)
            {
                if (!_byteToBorderString.TryGetValue(glyph, out string before)) return glyph;
                string after;
                if (clockwise)
                {
                    after = $"{before[3]}{before[0]}{before[1]}{before[2]}";
                }
                else
                {
                    after = $"{before[1]}{before[2]}{before[3]}{before[0]}";
                }
                return _borderStringToByte[after];
            }
            public static byte FullRotateIfIs(byte glyph)
            {
                return RotateIfIs(RotateIfIs(glyph, true), true);
            }
            public static byte FlipHorizontallyIfIs(byte glyph)
            {
                if (!_byteToBorderString.TryGetValue(glyph, out string before)) return glyph;
                return _borderStringToByte[$"{before[0]}{before[3]}{before[2]}{before[1]}"];
            }
            public static byte FlipVerticallyIfIs(byte glyph)
            {
                if (!_byteToBorderString.TryGetValue(glyph, out string before)) return glyph;
                return _borderStringToByte[$"{before[2]}{before[1]}{before[0]}{before[3]}"];
            }
        }
        public class Symmetric4WayCellMap
        {
            private Dictionary<Vec, byte> _directionToGlyph;
            private Dictionary<byte, Vec> _glyphToDirection;

            public Symmetric4WayCellMap(byte up, byte right, byte down, byte left)
            {
                _directionToGlyph = new Dictionary<Vec, byte>()
                {
                    {Vec.North, up },
                    {Vec.East, right },
                    {Vec.South, down },
                    {Vec.West, left },
                };
                _glyphToDirection = new Dictionary<byte, Vec>();
                foreach (KeyValuePair<Vec, byte> kvp in _directionToGlyph)
                {
                    _glyphToDirection.Add(kvp.Value, kvp.Key);
                }
            }

            public bool Is(byte glyph)
            {
                return _glyphToDirection.ContainsKey(glyph);
            }
            public byte RotateIfIs(byte glyph, bool clockwise)
            {
                if (!_glyphToDirection.TryGetValue(glyph, out Vec value)) return glyph;
                return _directionToGlyph[clockwise ? Vec.Rotate90ClockwiseAboutOrigin(value) : Vec.Rotate90AnticlockwiseAboutOrigin(value)];
            }
            public byte FlipHorizontallyIfIs(byte glyph)
            {
                if (!_glyphToDirection.TryGetValue(glyph, out Vec value)) return glyph;
                return value.Y == 0 ? _directionToGlyph[-value] : glyph;
            }
            public byte FlipVerticallyIfIs(byte glyph)
            {
                if (!_glyphToDirection.TryGetValue(glyph, out Vec value)) return glyph;
                return value.X == 0 ? _directionToGlyph[-value] : glyph;
            }
            public byte FullRotateIfIs(byte glyph)
            {
                if (!_glyphToDirection.TryGetValue(glyph, out Vec value)) return glyph;
                return _directionToGlyph[-value];
            }
        }

        private static Dictionary<char, byte> _charToByte;
        public static char[] _byteToChar;

        private static Dictionary<byte, byte> _twoStageQuarterRotations;
        private static Dictionary<byte, byte> _horizontalFlips;
        private static Dictionary<byte, byte> _verticalFlips;
        private static Dictionary<byte, byte> _fullRotations;

        public static Texture2D CompiledGlyphTexture { get; private set; }

        public static int CharWidth { get; private set; }
        public static int CharHeight { get; private set; }

        private static Symmetric4WayCellMap[] _symmetric4WayCellMaps = new Symmetric4WayCellMap[]
        {
            new(ArrowUp, ArrowRight, ArrowDown, ArrowLeft),
            new(TriangleUp, TriangleRight, TriangleDown, TriangleLeft),
            new(BlockHalfTop, BlockHalfRight, BlockHalfBottom, BlockHalfLeft),
            new(Caret, GreaterThan, VLower, LessThan),
        };

        // Methods
        private static void _addTwoStage(Dictionary<byte, byte> dictionary, byte first, byte second)
        {
            dictionary.Add(first, second);
            dictionary.Add(second, first);
        }
        public static void Initialise(MonoGameInstance mgi, Texture2D glyphTexture, int glyphWidthInPixels, int glyphHeightInPixels)
        {
            CharWidth = glyphWidthInPixels;
            CharHeight = glyphHeightInPixels;

            Color[] sourceArray = new Color[glyphTexture.Width * glyphTexture.Height];

            glyphTexture.GetData<Color>(sourceArray);

            Color[] destinationArray = new Color[glyphTexture.Width * glyphTexture.Height];

            Vec sourceGlyphGridDimensions = new Vec(glyphTexture.Width / glyphWidthInPixels, glyphTexture.Height / glyphHeightInPixels);
            Vec destinationGlyphGridDimensions = new Vec(1, sourceGlyphGridDimensions.X * sourceGlyphGridDimensions.Y);

            for (int s = 0; s < glyphTexture.Width * glyphTexture.Height; s++) // The index of this pixel on the source image
            {
                int sx = s % glyphTexture.Width; // The x position of this pixel on the source image
                int sy = s / glyphTexture.Width; // The y position of this pixel on the source image

                int sggx = sx / glyphWidthInPixels; // The x position of this glyph on the source glyph grid
                int sggy = sy / glyphHeightInPixels; // The y position of this glyph on the source glyph grid

                int i = sggx + sggy * sourceGlyphGridDimensions.X; // The index of this glyph on any glyph grid

                int dggx = i % destinationGlyphGridDimensions.X; // The x position of this glyph on the destination glyph grid
                int dggy = i / destinationGlyphGridDimensions.X; // The y position of this glyph on the destination glyph grid

                int sgpx = sggx * glyphWidthInPixels; // The x position of this glyph on the source image
                int sgpy = sggy * glyphHeightInPixels; // The y position of this glyph on the source image

                int dgpx = dggx * glyphWidthInPixels; // The x position of this glyph on the destination image
                int dgpy = dggy * glyphHeightInPixels; // The y position of this glyph on the destination image

                int dx = sx - sgpx + dgpx; // The x position of this pixel on the destination image
                int dy = sy - sgpy + dgpy; // The y position of this pixel on the destination image

                int d = dx + dy * glyphWidthInPixels * destinationGlyphGridDimensions.X; // The index of this pixel on the destination image

                destinationArray[d] = sourceArray[s];
            }

            Texture2D compiled = new Texture2D(mgi.GraphicsDevice, glyphWidthInPixels * destinationGlyphGridDimensions.X, glyphHeightInPixels * destinationGlyphGridDimensions.Y);

            compiled.SetData<Color>(destinationArray);

            CompiledGlyphTexture = compiled;

            Border.Initialise();

            _charToByte = new Dictionary<char, byte>
            {
                {' ', Space},
                {'\u00A0', Space},
                {'☺', Smiley},
                {'╷', Border.n0.e0.s1.w0},
                {'♥', Heart},
                {'♦', Diamond},
                {'♣', Club},
                {'♠', Spade},
                {'\u2022', Bullet},
                {'╸', Border.n0.e0.s0.w2},
                {'○', Circle},
                {'╺', Border.n0.e2.s0.w0},
                {'♂', Male},
                {'♀', Female},
                {'♪', Quaver},
                {'♫', QuaverDouble},
                {'☼', Solar},
                {'►', TriangleRight},
                {'◄', TriangleLeft},
                {'↕', ArrowUpDown},
                {'‼', ExclamationMarkDouble},
                {'¶', Pilcrow},
                {'§', Section},
                {'▬', RectangleHorizontal},
                {'╻', Border.n0.e0.s2.w0},
                {'↑', ArrowUp},
                {'↓', ArrowDown},
                {'→', ArrowRight},
                {'←', ArrowLeft},
                {'∟', RightAngle},
                {'↔', ArrowLeftRight},
                {'▲', TriangleUp},
                {'▼', TriangleDown},
                {'∪', Union},
                {'!', ExclamationMark},
                {'\"', QuotationMark},
                {'#', Number},
                {'$', Dollar},
                {'%', Percent},
                {'&', Ampersand},
                {'\'', Apostrophe},
                {'(', ParenthesisOpen},
                {')', ParenthesisClose},
                {'*', Asterisk},
                {'+', Plus},
                {',', Comma},
                {'-', Minus},
                {'.', Period},
                {'/', Slash},
                {'0', Zero},
                {'1', One},
                {'2', Two},
                {'3', Three},
                {'4', Four},
                {'5', Five},
                {'6', Six},
                {'7', Seven},
                {'8', Eight},
                {'9', Nine},
                {':', Colon},
                {';', Semicolon},
                {'<', LessThan},
                {'=', EqualTo},
                {'>', GreaterThan},
                {'?', QuestionMark},
                {'@', At},
                {'A', A},
                {'B', B},
                {'C', C},
                {'D', D},
                {'E', E},
                {'F', F},
                {'G', G},
                {'H', H},
                {'I', I},
                {'J', J},
                {'K', K},
                {'L', L},
                {'M', M},
                {'N', N},
                {'O', O},
                {'P', P},
                {'Q', Q},
                {'R', R},
                {'S', S},
                {'T', T},
                {'U', U},
                {'V', V},
                {'W', W},
                {'X', X},
                {'Y', Y},
                {'Z', Z},
                {'[', BracketOpen},
                {'\\', BackSlash},
                {']', BracketClose},
                {'^', Caret},
                {'_', Underscore},
                {'`', Grave},
                {'a', ALower},
                {'b', BLower},
                {'c', CLower},
                {'d', DLower},
                {'e', ELower},
                {'f', FLower},
                {'g', GLower},
                {'h', HLower},
                {'i', ILower},
                {'j', JLower},
                {'k', KLower},
                {'l', LLower},
                {'m', MLower},
                {'n', NLower},
                {'o', OLower},
                {'p', PLower},
                {'q', QLower},
                {'r', RLower},
                {'s', SLower},
                {'t', TLower},
                {'u', ULower},
                {'v', VLower},
                {'w', WLower},
                {'x', XLower},
                {'y', YLower},
                {'z', ZLower},
                {'{', BraceOpen},
                {'}', BraceClose},
                {'|', Pipe},
                {'¦', Pipe},
                {'–', Minus},
                {'—', Minus},
                {'~', Tilde},
                {'⌂', House},
                {'Ç', CCedilla},
                {'ü', ULowerDiaeresis},
                {'é', ELowerAcute},
                {'â', ALowerCaret},
                {'ä', ALowerDiaeresis},
                {'à', ALowerGrave},
                {'å', ALowerRing},
                {'ç', CLowerCedilla},
                {'ê', ELowerCaret},
                {'ë', ELowerDiaeresis},
                {'è', ELowerGrave},
                {'ï', ILowerDiaeresis},
                {'î', ILowerCaret},
                {'ì', ILowerGrave},
                {'Ù', ULowerGrave},
                {'Ú', ULowerAcute},
                {'Û', ULowerCaret},
                {'È', ELowerGrave},
                {'Ä', ADiaeresis},
                {'Å', ARing},
                {'É', EAcute},
                {'æ', AELower},
                {'Ê', ELowerCaret},
                {'Â', ALowerCaret},
                {'Ë', ELowerDiaeresis},
                {'Ì', ILowerGrave},
                {'Í', ILowerAcute},
                {'Î', ILowerCaret},
                {'Ï', ILowerDiaeresis},
                {'Ò', OLowerGrave},
                {'Ó', OLowerAcute},
                {'Ô', OLowerCaret},
                {'Æ', AE},
                {'ô', OLowerCaret},
                {'ö', OLowerDiaeresis},
                {'ò', OLowerGrave},
                {'û', ULowerCaret},
                {'À', ALowerGrave},
                {'Á', ALowerAcute},
                {'ù', ULowerGrave},
                {'ÿ', YLowerDiaeresis},
                {'Ö', ODiaeresis},
                {'Ü', UDiaeresis},
                {'¢', Cent},
                {'£', Pound},
                {'¥', Yen},
                {'₧', Points}, // I realise this doesn't mean 'points' but I made it that because I doubt I will ever use ₧ and a Pts for points character could be handy
                {'ƒ', FLowerHook},
                {'á', ALowerAcute},
                {'í', ILowerAcute},
                {'ó', OLowerAcute},
                {'ú', ULowerAcute},
                {'ñ', NLowerTilde},
                {'Ñ', NTilde},
                {'ª', OrdinalFem},
                {'º', OrdinalMasc},
                {'¿', QuestionMarkFlipped},
                {'╹', Border.n2.e0.s0.w0},
                {'╵', Border.n1.e0.s0.w0},
                {'½', Half},
                {'¼', Quarter},
                {'¡', ExclamationMarkFlipped},
                {'«', AngleBraceOpen},
                {'»', AngleBraceClose},
                {'░', BlockLight},
                {'▒', BlockMedium},
                {'▓', BlockHeavy},
                {'│', Border.n1.e0.s1.w0},
                {'┤', Border.n1.e0.s1.w1},
                {'╡', Border.n1.e0.s1.w2},
                {'╢', Border.n2.e0.s2.w1},
                {'╖', Border.n0.e0.s2.w1},
                {'╕', Border.n0.e0.s1.w2},
                {'╣', Border.n2.e0.s2.w2},
                {'║', Border.n2.e0.s2.w0},
                {'╗', Border.n0.e0.s2.w2},
                {'╝', Border.n2.e0.s0.w2},
                {'╜', Border.n2.e0.s0.w1},
                {'╛', Border.n1.e0.s0.w2},
                {'┐', Border.n0.e0.s1.w1},
                {'└', Border.n1.e1.s0.w0},
                {'┴', Border.n1.e1.s0.w1},
                {'┬', Border.n0.e1.s1.w1},
                {'├', Border.n1.e1.s1.w0},
                {'─', Border.n0.e1.s0.w1},
                {'┼', Border.n1.e1.s1.w1},
                {'╞', Border.n1.e2.s1.w0},
                {'╟', Border.n2.e1.s2.w0},
                {'╚', Border.n2.e2.s0.w0},
                {'╔', Border.n0.e2.s2.w0},
                {'╩', Border.n2.e2.s0.w2},
                {'╦', Border.n0.e2.s2.w2},
                {'╠', Border.n2.e2.s2.w0},
                {'═', Border.n0.e2.s0.w2},
                {'╬', Border.n2.e2.s2.w2},
                {'╧', Border.n1.e2.s0.w2},
                {'╨', Border.n2.e1.s0.w1},
                {'╤', Border.n0.e2.s1.w2},
                {'╥', Border.n0.e1.s2.w1},
                {'╙', Border.n2.e1.s0.w0},
                {'╘', Border.n1.e2.s0.w0},
                {'╒', Border.n0.e2.s1.w0},
                {'╓', Border.n0.e1.s2.w0},
                {'╫', Border.n2.e1.s2.w1},
                {'╪', Border.n1.e2.s1.w2},
                {'┘', Border.n1.e0.s0.w1},
                {'┌', Border.n0.e1.s1.w0},
                {'█', BlockFull},
                {'▄', BlockHalfBottom},
                {'▌', BlockHalfLeft},
                {'▐', BlockHalfRight},
                {'▀', BlockHalfTop},
                {'α', Alpha},
                {'ß', Beta},
                {'Γ', Gamma},
                {'π', Pi},
                {'Σ', Sigma},
                {'σ', SigmaLower},
                {'µ', Mu},
                {'τ', Tau},
                {'Φ', Phi},
                {'Θ', Theta},
                {'Ω', Omega},
                {'δ', Delta},
                {'∞', Lemniscate},
                {'φ', PhiLower},
                {'╴', Border.n0.e0.s0.w1},
                {'∩', Intersect},
                {'≡', Tribar},
                {'±', PlusMinus},
                {'≥', GreaterThanOrEqualTo},
                {'≤', LessThanOrEqualTo},
                {'⌠', IntegralTop},
                {'⌡', IntegralBottom},
                {'÷', Divide},
                {'≈', EqualToRoughly},
                {'°', Degrees},
                {'\u2219', BulletSmall},
                {'·', BulletTiny},
                {'√', Root},
                {'�', QuestionMarkInverted},
                {'╶', Border.n0.e1.s0.w0},
                {'■', RectangleVertical},
                {'‾', Overscore},
                {'¯', Overscore},
            };

            _byteToChar = new char[256];
            foreach (KeyValuePair<char, byte> kvp in _charToByte)
            {
                _byteToChar[kvp.Value] = kvp.Key;
            }

            _twoStageQuarterRotations = new Dictionary<byte, byte>();
            _addTwoStage(_twoStageQuarterRotations, BackSlash, Slash);
            _addTwoStage(_twoStageQuarterRotations, ArrowLeftRight, ArrowUpDown);
            _addTwoStage(_twoStageQuarterRotations, Lemniscate, Eight);
            _addTwoStage(_twoStageQuarterRotations, Z, N);
            _addTwoStage(_twoStageQuarterRotations, Minus, Pipe);

            _horizontalFlips = new Dictionary<byte, byte>();
            _addTwoStage(_horizontalFlips, BracketOpen, BracketClose);
            _addTwoStage(_horizontalFlips, BraceOpen, BraceClose);
            _addTwoStage(_horizontalFlips, AngleBraceOpen, AngleBraceClose);
            _addTwoStage(_horizontalFlips, ParenthesisOpen, ParenthesisClose);
            _addTwoStage(_horizontalFlips, ZLower, SLower);
            _addTwoStage(_horizontalFlips, LessThanOrEqualTo, GreaterThanOrEqualTo);
            _addTwoStage(_horizontalFlips, P, Pilcrow);
            _addTwoStage(_horizontalFlips, BLower, DLower);
            _addTwoStage(_horizontalFlips, PLower, QLower);
            _addTwoStage(_horizontalFlips, E, Three);
            _addTwoStage(_horizontalFlips, BackSlash, Slash);

            _verticalFlips = new Dictionary<byte, byte>();
            _addTwoStage(_verticalFlips, Underscore, Overscore);
            _addTwoStage(_verticalFlips, M, W);
            _addTwoStage(_verticalFlips, Intersect, Union);
            _addTwoStage(_verticalFlips, Comma, Apostrophe);
            _addTwoStage(_verticalFlips, MLower, WLower);
            _addTwoStage(_verticalFlips, BackSlash, Slash);
            _addTwoStage(_verticalFlips, ExclamationMark, ExclamationMarkFlipped);

            _fullRotations = new Dictionary<byte, byte>(); // fine if it doesn't include ones already part of a quarter
            _addTwoStage(_fullRotations, IntegralBottom, IntegralTop);
            _addTwoStage(_fullRotations, QuestionMark, QuestionMarkFlipped);
            _addTwoStage(_fullRotations, NLower, ULower);
            _addTwoStage(_fullRotations, Six, Nine);
            _addTwoStage(_fullRotations, Seven, L);
            _addTwoStage(_fullRotations, HLower, YLower);

            _addTwoStage(_fullRotations, Underscore, Overscore);
            _addTwoStage(_fullRotations, M, W);
            _addTwoStage(_fullRotations, MLower, WLower);
            _addTwoStage(_fullRotations, Intersect, Union);
            _addTwoStage(_fullRotations, Comma, Apostrophe);

            _addTwoStage(_fullRotations, BracketOpen, BracketClose);
            _addTwoStage(_fullRotations, BraceOpen, BraceClose);
            _addTwoStage(_fullRotations, AngleBraceOpen, AngleBraceClose);
            _addTwoStage(_fullRotations, ParenthesisOpen, ParenthesisClose);
            _addTwoStage(_fullRotations, ExclamationMarkFlipped, ExclamationMark);
        }
        public static byte MatchChar(char character)
        {
            if (_charToByte.TryGetValue(character, out byte value))
            {
                return value;
            }
            return QuestionMarkInverted;
        }
        public static char AsChar(byte glyph) => _byteToChar[glyph];
        public static Rectangle GetRectangle(byte glyph)
        {
            return GetRectangle(glyph, 1);
        }
        public static Rectangle GetRectangle(byte glyph, int repetitions)
        {
            return new Rectangle(0, CharHeight * glyph, CharWidth * repetitions, CharHeight);
        }
        private static bool _attemptSwap(Dictionary<byte, byte> dictionary, byte glyph, out byte result)
        {
            if (dictionary.TryGetValue(glyph, out result)) return true;
            result = glyph;
            return false;
        }
        public static byte AttemptRotate(byte glyph, bool clockwise)
        {
            if (Border.IsBorder(glyph)) return Border.RotateIfIs(glyph, clockwise);
            foreach (Symmetric4WayCellMap s4wcm in _symmetric4WayCellMaps) if (s4wcm.Is(glyph)) return s4wcm.RotateIfIs(glyph, clockwise);
            if (_attemptSwap(_twoStageQuarterRotations, glyph, out glyph)) return glyph;

            return glyph;
        }
        public static byte AttemptFullRotate(byte glyph)
        {
            if (Border.IsBorder(glyph)) return Border.FullRotateIfIs(glyph);
            foreach (Symmetric4WayCellMap s4wcm in _symmetric4WayCellMaps) if (s4wcm.Is(glyph)) return s4wcm.FullRotateIfIs(glyph);
            if (_attemptSwap(_fullRotations, glyph, out glyph)) return glyph;

            return glyph;
        }
        public static byte AttemptHorizontalFlip(byte glyph)
        {
            if (Border.IsBorder(glyph)) return Border.FlipHorizontallyIfIs(glyph);
            foreach (Symmetric4WayCellMap s4wcm in _symmetric4WayCellMaps) if (s4wcm.Is(glyph)) return s4wcm.FlipHorizontallyIfIs(glyph);
            if (_attemptSwap(_horizontalFlips, glyph, out glyph)) return glyph;

            return glyph;
        }
        public static byte AttemptVerticalFlip(byte glyph)
        {
            if (Border.IsBorder(glyph)) return Border.FlipVerticallyIfIs(glyph);
            foreach (Symmetric4WayCellMap s4wcm in _symmetric4WayCellMaps) if (s4wcm.Is(glyph)) return s4wcm.FlipVerticallyIfIs(glyph);
            if (_attemptSwap(_verticalFlips, glyph, out glyph)) return glyph;

            return glyph;
        }
    }
}
