using Blunatic.Core;
using Blunatic.Mgc;
using Blunatic.Parsing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public class CheatSheetMasterTableElement : IMonoGameConsoleElement
    {
        // Constants
        private static readonly string DESCRIPTION_HEADER = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("8E7CC3"));
        private static readonly string DESCRIPTION_BODY = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("B4A7D6"));

        private static readonly string NAME_HEADER = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("6FA8DC"));
        private static readonly string NAME_BODY = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("9FC5E8"));

        private static readonly string OPCODE_HEADER = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("F6B26B"));
        private static readonly string OPCODE_BODY = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("FFFFFF"));

        private static readonly string OPERANDS_HEADER = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("93C47D"));

        private static readonly string FLAGS_HEADER = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("6D9EEB"));

        private static readonly string BYTES_HEADER = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("FFE599"));

        private static readonly string COND_BODY = Fm.Col(Hex.GetColor("000000"), Hex.GetColor("F9CB9C"));

        // Properties
        public Vec Position { get; set; }
        public Vec Dimensions => new Vec(0, 0);
        public bool CapturingControls => false;

        // Constructors
        public CheatSheetMasterTableElement(Vec position)
        {
            Position = position;
        }

        // Methods
        public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            
        }
        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            void write(Vec pos, string formatting, string text)
            {
                mgc.WriteString(mgi, Position + pos, $"{formatting}{text}");
            }

            int table(Vec pos, string headerFormatting, string bodyFormatting, params string[] text)
            {
                int pad = text[0].Length;

                write(pos, headerFormatting, text[0].PadLeft(pad));

                for (int i = 1; i < text.Length; i++)
                {
                    write(new Vec(pos.X, pos.Y + i), bodyFormatting, text[i].PadLeft(pad));
                }

                return pad;
            }

            int xshift = 0;
            xshift += table(new Vec(xshift, 2), DESCRIPTION_HEADER, DESCRIPTION_BODY, 
                "  Description  ", 
                "No operation", 
                "Halt", 
                "Addition", 
                "Subtraction", 
                "Bitwise NOR", 
                "Bitwise AND", 
                "Bitwise XOR", 
                "Bitshift right", 
                "Load immediate", 
                "Add immediate", 
                "Jump", 
                "Branch", 
                "Call", 
                "Return", 
                "Load memory", 
                "Store memory"
            );

            xshift += table(new Vec(xshift, 2), NAME_HEADER, NAME_BODY,
                " Name",
                "NOP",
                "HLT",
                "ADD",
                "SUB",
                "NOR",
                "AND",
                "XOR",
                "RSH",
                "LDI",
                "ADI",
                "JMP",
                "BNC",
                "CAL",
                "RET",
                "LOD",
                "STR"
            );

            write(new Vec(xshift, 1), OPCODE_HEADER, "     Opcode     ");
            write(new Vec(xshift, 0), BYTES_HEADER, $"             Byte 1             {Fm.Bg(Hex.GetColor("F2D891"))}             Byte 2             ");
            xshift += table(new Vec(xshift, 2), OPCODE_HEADER, OPCODE_BODY,
                " 15 ",
                "0",
                "0",
                "0",
                "0",
                "0",
                "0",
                "0",
                "0",
                "1",
                "1",
                "1",
                "1",
                "1",
                "1",
                "1",
                "1"
            );
            xshift += table(new Vec(xshift, 2), OPCODE_HEADER, OPCODE_BODY,
                " 14 ",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   1"
            );
            xshift += table(new Vec(xshift, 2), OPCODE_HEADER, OPCODE_BODY,
                " 13 ",
                "0",
                "0",
                "1",
                "1",
                "0",
                "0",
                "1",
                "1",
                "0",
                "0",
                "1",
                "1",
                "0",
                "0",
                "1",
                "1"
            );
            xshift += table(new Vec(xshift, 2), OPCODE_HEADER, OPCODE_BODY,
                " 12 ",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1",
                $"{Fm.Bg(Color.LightGray)}   0",
                $"{Fm.Bg(Color.LightGray)}   1"
            );

            write(new Vec(xshift, 1), OPERANDS_HEADER, "                    Operands                    ");
            xshift += table(new Vec(xshift, 2), OPERANDS_HEADER, OPCODE_BODY,
                " 11  10   9   8 ",
                $"{Fm.Bg(Color.DarkGray)}                ",
                $"{Fm.Bg(Color.DarkGray)}                ",
                "    Address 1   ",
                "    Address 1   ",
                "    Address 1   ",
                "    Address 1   ",
                "    Address 1   ",
                "    Address 1   ",
                "    Address 1   ",
                "    Address 1   ",
                $"{Fm.Bg(Color.DarkGray)}      Anything      ",
                $"  Cond{Ch.AsChar(Ch.Female)} {Fm.Bg(Color.DarkGray)}  Anything  ",
                $"{Fm.Bg(Color.DarkGray)}      Anything      ",
                $"{Fm.Bg(Color.DarkGray)}                ",
                " Pointer Address",
                " Pointer Address"
            );

            xshift += table(new Vec(xshift, 2), OPERANDS_HEADER, OPCODE_BODY,
                "  7   6   5   4 ",
                $"{Fm.Bg(Color.DarkGray)}     Anything   ",
                $"{Fm.Bg(Color.DarkGray)}     Anything   ",
                $"{Fm.Bg(Color.LightGray)}    Address 2   ",
                $"{Fm.Bg(Color.LightGray)}    Address 2   ",
                $"{Fm.Bg(Color.LightGray)}    Address 2   ",
                $"{Fm.Bg(Color.LightGray)}    Address 2   ",
                $"{Fm.Bg(Color.LightGray)}    Address 2   ",
                $"{Fm.Bg(Color.LightGray)} Plus Address*^ ",
                $"{Fm.Bg(Color.LightGray)}            Imme",
                $"{Fm.Bg(Color.LightGray)}            Imme",
                $"{Fm.Bg(Color.DarkGray)}    {OPCODE_BODY}       Progr",
                $"{Fm.Bg(Color.DarkGray)}ng  {OPCODE_BODY}       Progr",
                $"{Fm.Bg(Color.DarkGray)}    {OPCODE_BODY}       Progr",
                $"{Fm.Bg(Color.DarkGray)}     Anything   ",
                $"{Fm.Bg(Color.LightGray)} Offset Address*",
                $"{Fm.Bg(Color.LightGray)}  Value Address "
            );

            xshift += table(new Vec(xshift, 2), OPERANDS_HEADER, OPCODE_BODY,
                "  3   2   1   0 ",
                $"{Fm.Bg(Color.DarkGray)}                ",
                $"{Fm.Bg(Color.DarkGray)}                ",
                "    Address 3   ",
                "    Address 3   ",
                "    Address 3   ",
                "    Address 3   ",
                "    Address 3   ",
                "    Address 3   ",
                $"{Fm.Bg(Color.LightGray)}diate           ",
                $"{Fm.Bg(Color.LightGray)}diate           ",
                "am Address      ",
                "am Address      ",
                "am Address      ",
                $"{Fm.Bg(Color.DarkGray)}                ",
                " Put In Address ",
                $"   Offset*{Ch.AsChar(Ch.OrdinalMasc)}    "
            );

            table(new Vec(xshift, 2), FLAGS_HEADER, OPCODE_BODY,
                " Set ALU flags?",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightGreen)}            Yes",
                $"{Fm.Bg(Color.LightGreen)}            Yes",
                $"{Fm.Bg(Color.LightGreen)}            Yes",
                $"{Fm.Bg(Color.LightGreen)}            Yes",
                $"{Fm.Bg(Color.LightGreen)}            Yes",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightGreen)}            Yes",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No",
                $"{Fm.Bg(Color.LightCoral)}             No"
            );

            mgc.WriteString(mgi, Position + new Vec(0, 20), "*Can be ommitted        ^Contents of this register are added to the other contents before shift");
            mgc.WriteString(mgi, Position + new Vec(0, 22), $"{Ch.AsChar(Ch.Female)}Conditions are:        {Ch.AsChar(Ch.OrdinalMasc)}A 4-bit immediate value");
            xshift = 0;
            xshift += table(new Vec(xshift, 23), OPCODE_HEADER, COND_BODY, " Cond", "  00", "  01", "  10", "  11");
            xshift += table(new Vec(xshift, 23), NAME_HEADER, NAME_BODY, " Flag", "   C", "  NC", "   Z", "  NZ");
            xshift += table(new Vec(xshift, 23), NAME_HEADER, NAME_BODY, " Description", "Carry", "Not Carry", "Zero", "Not Zero");
        }

    }
}
