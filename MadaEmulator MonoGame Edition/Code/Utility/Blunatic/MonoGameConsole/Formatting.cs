using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;
using Blunatic.Parsing;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public static class Fm
    {
        public const char INITIALISER = '£';

        private static string _getCodeDetectionRegex(string regex)
        {
            // Match with start but not end (^ but not $)
            return $"^{INITIALISER}{regex}";
        }
        private static string MODIFY_FOREGROUND_COLOR_REGEX = _getCodeDetectionRegex(@"CF[0-9a-fA-F]{6}");
        private static string MODIFY_BACKGROUND_COLOR_REGEX = _getCodeDetectionRegex(@"CB[0-9a-fA-F]{6}");
        private static string INSERT_ANIMATED_CELL_REGEX = _getCodeDetectionRegex(@"A[0-9]{5}");

        public enum AnimatedCell : short
        {
            CycleAnticlockwise,
            CycleClockwise,
            StarPulse,
            WaveLeft,
            WaveRight,
            PlayerYou,
            PulseX,
            PulseDiamond,
            WaveO1,
            WaveO2,
            WaveO3,
            WaveO4,
            BracketRotateOpen,
            BracketRotateClosed,
        }

        private static Dictionary<short, (int, string)> _animatedCells = new Dictionary<short, (int, string)>()
        {
            {(short)AnimatedCell.CycleAnticlockwise, (60, @"-/|\")},
            {(short)AnimatedCell.CycleClockwise, (60, @"-\|/")},
            {(short)AnimatedCell.StarPulse, (60, @"·*☼☼☼☼☼*")},
            {(short)AnimatedCell.WaveLeft, (60, @"\|")},
            {(short)AnimatedCell.WaveRight, (60, @"/|")},
            {(short)AnimatedCell.PlayerYou, (240, @"☺☺☺☺☺☺☺☺☺YOU")},
            {(short)AnimatedCell.PulseX, (60, @"xX")},
            {(short)AnimatedCell.PulseDiamond, (60, @"•♦")},
            {(short)AnimatedCell.WaveO1, (60, @"Oooo")},
            {(short)AnimatedCell.WaveO2, (60, @"oOoo")},
            {(short)AnimatedCell.WaveO3, (60, @"ooOo")},
            {(short)AnimatedCell.WaveO4, (60, @"oooO")},
            {(short)AnimatedCell.BracketRotateOpen, (60, @"[[[[╺=-=╺")},
            {(short)AnimatedCell.BracketRotateClosed, (60, @"]]]]╸=-=╸")},
        };

        public enum CodeCheckResult
        {
            None,
            ModifyForegroundColor,
            ModifyBackgroundColor,
            InsertAnimatedCell,
        }

        private static Dictionary<string, CodeCheckResult> _codeCheckRelations = new Dictionary<string, CodeCheckResult>()
        {
            {MODIFY_FOREGROUND_COLOR_REGEX, CodeCheckResult.ModifyForegroundColor },
            {MODIFY_BACKGROUND_COLOR_REGEX, CodeCheckResult.ModifyBackgroundColor },
            {INSERT_ANIMATED_CELL_REGEX, CodeCheckResult.InsertAnimatedCell },
        };

        private static Dictionary<CodeCheckResult, int> _codeLength = new Dictionary<CodeCheckResult, int>()
        {
            {CodeCheckResult.None, 1},
            {CodeCheckResult.ModifyForegroundColor, 9},
            {CodeCheckResult.ModifyBackgroundColor, 9},
            {CodeCheckResult.InsertAnimatedCell, 7},
        };

        private static Dictionary<CodeCheckResult, int> _consequenceLength = new Dictionary<CodeCheckResult, int>()
        {
            {CodeCheckResult.None, 1},
            {CodeCheckResult.ModifyForegroundColor, 0},
            {CodeCheckResult.ModifyBackgroundColor, 0},
            {CodeCheckResult.InsertAnimatedCell, 1},
        };

        public static string Col(Color foregroundColor, Color backgroundColor)
        {
            return $"{Fg(foregroundColor)}{Bg(backgroundColor)}";
        }

        public static string Fg(Color color)
        {
            return $"{INITIALISER}CF{Hex.GetString(color)}";
        }
        public static Color ReadModifyForegroundColorCode(string text, int initialiserIndex)
        {
            string hexPayload = text.Substring(initialiserIndex + 3, 6);
            return Hex.GetColor(hexPayload);
        }

        public static string Bg(Color color)
        {
            return $"{INITIALISER}CB{Hex.GetString(color)}";
        }
        public static Color ReadModifyBackgroundColorCode(string text, int initialiserIndex)
        {
            string hexPayload = text.Substring(initialiserIndex + 3, 6);
            return Hex.GetColor(hexPayload);
        }

        public static string An(AnimatedCell animatedCell)
        {
            return $"{INITIALISER}A{$"{(short)animatedCell}".PadLeft(5, '0')}";
        }
        public static string An(params AnimatedCell[] animatedCells)
        {
            string toReturn = string.Empty;
            foreach (AnimatedCell a in animatedCells)
            {
                toReturn += An(a);
            }
            return toReturn;
        }
        public static short ReadAnimatedCellValueFromInsertAnimatedCellCode(string text, int initialiserIndex) => short.Parse(text.Substring(initialiserIndex + 2, 5));
        public static char ReadInsertAnimatedCellCode(MonoGameInstance mgi, string text, int initialiserIndex) => GetCurrentCharacterInAnimatedCell(mgi, ReadAnimatedCellValueFromInsertAnimatedCellCode(text, initialiserIndex));
        public static char GetCurrentCharacterInAnimatedCell(MonoGameInstance mgi, AnimatedCell animatedCell) => GetCurrentCharacterInAnimatedCell(mgi, (short)animatedCell);
        public static char GetCurrentCharacterInAnimatedCell(MonoGameInstance mgi, short animatedCellValue)
        {
            int cycleLength;
            string pattern;
            (cycleLength, pattern) = _animatedCells[animatedCellValue];
            float currentCycleTick = mgi.Ticks % cycleLength;
            int currentFrame = (int)(currentCycleTick * pattern.Length / cycleLength);
            return pattern[currentFrame];
        }

        public static CodeCheckResult CheckForCode(string text, int initialiserIndex)
        {
            string remaining = text.Substring(initialiserIndex);

            foreach (KeyValuePair<string, CodeCheckResult> kvp in _codeCheckRelations)
            {
                if (Regex.IsMatch(remaining, kvp.Key)) return kvp.Value;
            }

            return CodeCheckResult.None;
        }
        public static int GetCodeLength(CodeCheckResult codeCheckResult)
        {
            return _codeLength[codeCheckResult];
        }
        public static int GetConsequenceLength(CodeCheckResult codeCheckResult)
        {
            return _consequenceLength[codeCheckResult];
        }
    }
}
