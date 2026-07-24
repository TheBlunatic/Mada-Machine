using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public class Emulator
    {
        // Classes
        public class ProgramLine
        {
            public Opcode Opcode {  get; set; }
            public byte[] Operands { get; set; }

            public ProgramLine(Opcode opcode, string[] operands)
            {
                Opcode = opcode;
                Operands = operands.Select((i) => { return byte.Parse(i); }).ToArray();
            }
        }
        public class RunImage
        {
            public byte[] Registers { get; set; }
            public Stack<byte> CallStack { get; set; }
            public byte ProgramCounter { get; set; }
            public int InstructionCounter { get; set; }
            public Dictionary<Condition, bool> Flags { get; set; }
            public bool Halted { get; set; }
            public byte[] Memory { get; set; }

            public RunImage(byte[] registers, Stack<byte> callStack, byte programCounter, int instructionCounter, Dictionary<Condition, bool> flags, bool halted, byte[] memory)
            {
                Registers = new byte[registers.Length];
                for (int i = 0; i < registers.Length; i++)
                {
                    Registers[i] = registers[i];
                }

                Stack<byte> tempStack = new Stack<byte>();
                while (callStack.Count > 0)
                {
                    tempStack.Push(callStack.Pop());
                }
                CallStack = new Stack<byte>();
                while (tempStack.Count > 0)
                {
                    callStack.Push(tempStack.Peek());
                    CallStack.Push(tempStack.Pop());
                }
                ProgramCounter = programCounter;
                InstructionCounter = instructionCounter;
                Flags = new Dictionary<Condition, bool>();
                foreach (KeyValuePair<Condition, bool> kvp in flags)
                {
                    Flags.Add(kvp.Key, kvp.Value);
                }
                Halted = halted;

                Memory = new byte[memory.Length];
                for (int i = 0; i < memory.Length; i++)
                {
                    Memory[i] = memory[i];
                }
            }
        }

        // Enums
        public enum Opcode : byte
        {
            NOP,
            HLT,
            ADD,
            SUB,
            NOR,
            AND,
            XOR,
            RSH,
            LDI,
            ADI,
            JMP,
            BNC,
            CAL,
            RET,
            LOD,
            STR
        }
        public enum Condition : byte
        {
            Z = 0b00000000,
            NZ = 0b00000100,
            C = 0b00001000,
            NC = 0b00001100,
        }
        public enum Token
        {
            Label,
            Opcode,
            Register,
            Value4,
            Value7,
            Value8,
            Condition,
        }

        // Properties
        public byte[] Registers { get; private set; }
        public Stack<byte> CallStack { get; private set; }
        public byte ProgramCounter { get; private set; }
        public int InstructionCounter { get; private set; }
        public bool IsHalted { get; private set; }
        public byte[] Memory { get; private set; }

        public Dictionary<Condition, bool> Flags { get; private set; }

        public string[] ProgramText { get; private set; }
        public List<ProgramLine> Program { get; private set; }
        public string[] ProgramBinary { get; private set; }

        public Stack<RunImage> History { get; private set; }

        // Constructors
        public Emulator(string path)
        {
            Reset();
            Program = new List<ProgramLine>();
            LoadProgram(path);
        }

        // Methods
        public void Reset()
        {
            Registers = new byte[16];
            CallStack = new Stack<byte>();
            ProgramCounter = 0;
            InstructionCounter = 0;
            IsHalted = false;
            Memory = new byte[256];
            Flags = new Dictionary<Condition, bool>()
            {
                {Condition.Z, false },
                {Condition.NZ, false },
                {Condition.C, false },
                {Condition.NC, false },
            };
            History = new Stack<RunImage>();
        }
        public void UpdateDisplay()
        {
            void writeLineAtX(int x, string s)
            {
                Console.CursorLeft = x;
                Console.WriteLine(s);
            }
            void writeAtX(int x, string s)
            {
                Console.CursorLeft = x;
                Console.Write(s);
            }
            Console.Clear();
            writeLineAtX(30, $"PROGRAM COUNTER: {Convert.ToString(ProgramCounter, 2).PadLeft(8, '0')} ({ProgramCounter})");
            writeLineAtX(30, string.Empty);
            writeLineAtX(30, $"INSTRUCTION COUNTER: {InstructionCounter}");
            writeLineAtX(30, string.Empty);
            writeLineAtX(30, $"IS HALTED: {IsHalted}");
            writeLineAtX(30, string.Empty); 
            writeLineAtX(30, $"FLAGS:");
            foreach (KeyValuePair<Condition, bool> kvp in Flags)
            {
                writeLineAtX(30, $"{kvp.Key} : {kvp.Value}");
            }
            writeLineAtX(30, string.Empty);
            writeLineAtX(30, $"CALL STACK:");
            foreach (byte c in CallStack)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                writeLineAtX(30, $"0b{Convert.ToString(c, 2).PadLeft(8, '0')} ({c})");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            if (CallStack.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                writeLineAtX(30, $"EMPTY");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.CursorTop = 0;
            writeLineAtX(68, $"PROGRAM:");
            for (int i = 0; i < ProgramText.Length; i++)
            {
                if (i == ProgramCounter)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    writeLineAtX(68, $"{ProgramText[i]}");
                }
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.CursorTop = 0;
            writeLineAtX(0, $"REGISTERS:");
            for (byte i = 0; i < Registers.Length; i++)
            {
                writeLineAtX(0, $"{$"r{i}".PadLeft(3, ' ')} : 0b{Convert.ToString(GetRegister(i), 2).PadLeft(8, '0')} ({GetRegister(i)})");
            }
            writeLineAtX(0, string.Empty);
            writeLineAtX(0, $"MEMORY:");
            for (int y = 0; y < 16; y++)
            {
                byte[] array = new byte[16];
                Array.ConstrainedCopy(Memory, y * 16, array, 0, 16);
                writeAtX(0, $"{$"{y * 16}".PadLeft(3, ' ')} - {$"{y * 16 + 15}".PadLeft(3, ' ')}: ");
                string[] toPrint = BitConverter.ToString(array).Split('-');
                foreach (string value in toPrint)
                {
                    switch (value)
                    {
                        case "00":
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.White;
                            break;
                    }
                    Console.Write(value + " ");
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }
        }
        public void RestoreImage(RunImage runImage)
        {
            Registers = runImage.Registers;
            CallStack = runImage.CallStack;
            ProgramCounter = runImage.ProgramCounter;
            InstructionCounter = runImage.InstructionCounter;
            Flags = runImage.Flags;
            IsHalted = runImage.Halted;
            Memory = runImage.Memory;
        }
        public void PushImage()
        {
            History.Push(new RunImage(Registers, CallStack, ProgramCounter, InstructionCounter, Flags, IsHalted, Memory));
        }

        public byte Add(byte x, byte y)
        {
            int resultInt = (int)x + (int)y;
            byte result = (byte)(x + y);
            Flags[Condition.Z] = result == 0;
            Flags[Condition.NZ] = !Flags[Condition.Z];

            Flags[Condition.C] = resultInt > byte.MaxValue;
            Flags[Condition.NC] = !Flags[Condition.C];

            return result;
        }
        public byte Sub(byte x, byte y)
        {
            y = (byte)(~y);
            y++;
            return Add(x, y);
        }
        public byte Nor(byte x, byte y)
        {
            byte result = (byte)~(x | y);
            Flags[Condition.Z] = result == 0;
            Flags[Condition.NZ] = !Flags[Condition.Z];

            Flags[Condition.C] = false;
            Flags[Condition.NC] = !Flags[Condition.C];

            return result;
        }
        public byte And(byte x, byte y)
        {
            byte result = (byte)(x & y);
            Flags[Condition.Z] = result == 0;
            Flags[Condition.NZ] = !Flags[Condition.Z];

            Flags[Condition.C] = false;
            Flags[Condition.NC] = !Flags[Condition.C];

            return result;
        }
        public byte Xor(byte x, byte y)
        {
            byte result = (byte)(x ^ y);
            Flags[Condition.Z] = result == 0;
            Flags[Condition.NZ] = !Flags[Condition.Z];

            Flags[Condition.C] = false;
            Flags[Condition.NC] = !Flags[Condition.C];

            return result;
        }
        public byte Rsh(byte x)
        {
            byte result = (byte)(x >> 1);
            return result;
        }
        public byte Rsh(byte x, byte y)
        {
            byte result = (byte)((x + y) >> 1);
            return result;
        }

        public void RunProgram(bool step)
        {
            InstructionCounter = 0;
            IsHalted = false;
            History.Clear();
            PushImage();
            for (ProgramCounter = 0; ProgramCounter < Program.Count;)
            {
                if (IsHalted)
                {
                    step = true;
                }
                bool skipStep = IsHalted;
                if (step)
                {
                    UpdateDisplay();
                    string key = Console.ReadKey().Key.ToString();
                    switch (key)
                    {
                        case "A":
                            skipStep = true;
                            if (History.Count > 1)
                            {
                                History.Pop();
                                RestoreImage(History.Pop());
                                PushImage();
                                step = true;
                            }
                            else if (History.Count == 1)
                            {
                                RestoreImage(History.Pop());
                                PushImage();
                                step = true;
                            }
                            break;
                        case "D":
                            break;
                        case "R":
                            skipStep = true;
                            if (History.Count > 0)
                            {
                                while (History.Count > 1)
                                {
                                    History.Pop();
                                }
                                RestoreImage(History.Pop());
                                PushImage();
                                step = true;
                            }
                            break;
                        case "Q":
                            step = false;
                            break;
                        case "X":
                            UpdateDisplay();
                            return;
                        default:
                            skipStep = true;
                            break;
                    }
                }
                if (skipStep) continue;
                Step();
                if (IsHalted)
                {
                    step = true;
                }
            }
            UpdateDisplay();
        }
        public void Step()
        {
            switch (Program[ProgramCounter].Opcode)
            {
                case Opcode.NOP:
                    ProgramCounter++;
                    break;
                case Opcode.HLT:
                    IsHalted = true;
                    break;
                case Opcode.ADD:
                    SetRegister(Program[ProgramCounter].Operands[2], Add(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    ProgramCounter++;
                    break;
                case Opcode.SUB:
                    SetRegister(Program[ProgramCounter].Operands[2], Sub(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    ProgramCounter++;
                    break;
                case Opcode.NOR:
                    SetRegister(Program[ProgramCounter].Operands[2], Nor(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    ProgramCounter++;
                    break;
                case Opcode.AND:
                    SetRegister(Program[ProgramCounter].Operands[2], And(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    ProgramCounter++;
                    break;
                case Opcode.XOR:
                    SetRegister(Program[ProgramCounter].Operands[2], Xor(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    ProgramCounter++;
                    break;
                case Opcode.RSH:
                    if (Program[ProgramCounter].Operands.Length == 2)
                    {
                        SetRegister(Program[ProgramCounter].Operands[1], Rsh(GetRegister(Program[ProgramCounter].Operands[0])));
                    }
                    else
                    {
                        SetRegister(Program[ProgramCounter].Operands[1], Rsh(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    }
                    ProgramCounter++;
                    break;
                case Opcode.LDI:
                    SetRegister(Program[ProgramCounter].Operands[0], Program[ProgramCounter].Operands[1]);
                    ProgramCounter++;
                    break;
                case Opcode.ADI:
                    SetRegister(Program[ProgramCounter].Operands[0], Add(GetRegister(Program[ProgramCounter].Operands[0]), Program[ProgramCounter].Operands[1]));
                    ProgramCounter++;
                    break;
                case Opcode.JMP:
                    ProgramCounter = Program[ProgramCounter].Operands[0];
                    break;
                case Opcode.BNC:
                    if (Flags[(Condition)Program[ProgramCounter].Operands[0]])
                    {
                        ProgramCounter = Program[ProgramCounter].Operands[1];
                    }
                    else
                    {
                        ProgramCounter++;
                    }
                    break;
                case Opcode.CAL:
                    PushCallStack(Program[ProgramCounter].Operands[0]);
                    break;
                case Opcode.RET:
                    PopCallStack();
                    break;
                case Opcode.LOD:
                    if (Program[ProgramCounter].Operands.Length == 2)
                    {
                        SetRegister(Program[ProgramCounter].Operands[1], GetMemory(GetRegister(Program[ProgramCounter].Operands[0])));
                    }
                    else
                    {
                        SetRegister(Program[ProgramCounter].Operands[2], GetMemory((byte)(GetRegister(Program[ProgramCounter].Operands[0]) + GetRegister(Program[ProgramCounter].Operands[1]))));
                    }
                    ProgramCounter++;
                    break;
                case Opcode.STR:
                    if (Program[ProgramCounter].Operands.Length == 2)
                    {
                        SetMemory(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1]));
                    }
                    else
                    {
                        SetMemory((byte)(GetRegister(Program[ProgramCounter].Operands[0]) + Program[ProgramCounter].Operands[2]), GetRegister(Program[ProgramCounter].Operands[1]));
                    }
                    ProgramCounter++;
                    break;
                default:
                    throw new Exception();
            }
            InstructionCounter++;
            PushImage();
        }

        public void LoadProgram(string programPath)
        {
            Program.Clear();
            Dictionary<string, byte> labels = new Dictionary<string, byte>();
            string[] program = File.ReadAllLines(programPath);
            for (int lineIndex = 0; lineIndex < program.Length; lineIndex++)
            {
                program[lineIndex] = program[lineIndex].Trim(' ');
            }
            (Token, string)[][] programTokens = new (Token, string)[program.Length][];

            for (byte lineIndex = 0; lineIndex < program.Length; lineIndex++)
            {
                List<(Token, string)> list = new List<(Token, string)>();
                string line = program[lineIndex];
                {
                    int index = Array.IndexOf(line.ToCharArray(), '/');
                    if (index != -1)
                    {
                        line = line.Substring(0, index);
                    }
                }
                string[] splitLine = line.Split(' ').Where((i) => { return i != string.Empty; }).ToArray();
                foreach (string token in splitLine)
                {
                    if (token[0] == '.')
                    {
                        list.Add((Token.Label, token));
                        continue;
                    }

                    if (token.ToUpper() != token.ToLower() && Enum.TryParse<Opcode>(token, out Opcode opcodeResult))
                    {
                        list.Add((Token.Opcode, token));
                        continue;
                    }

                    if (token[0] == 'r')
                    {
                        if (int.TryParse($"{token.Substring(1)}", out int resultInt) && resultInt >= 0 && resultInt < 16)
                        {
                            list.Add((Token.Register, token.Substring(1)));
                            continue;
                        }
                        throw new FormatException($"Line {lineIndex}: Token '{token}' was not in a valid format for a register.");
                    }

                    if (token.Length > 2 && token[0] == '0' && token[1] == 'b')
                    {
                        if (token.Length > 10)
                        {
                            throw new FormatException($"Line {lineIndex}: Token '{token}' was not in a valid format for a binary value.");
                        }
                        byte baseTenToken = 0;
                        byte mult = 1;
                        for (int i = token.Length - 1; i >= 2; i--)
                        {
                            if (token[i] == '1')
                            {
                                baseTenToken += mult;
                            }
                            else if (token[i] != '0')
                            {
                                throw new FormatException($"Line {lineIndex}: Token '{token}' was not in a valid format for a binary value.");
                            }
                            mult *= 2;
                        }
                        if (baseTenToken < 0b00010000)
                        {
                            list.Add((Token.Value4, $"{baseTenToken}"));
                        }
                        else if (baseTenToken < 0b10000000)
                        {
                            list.Add((Token.Value7, $"{baseTenToken}"));
                        }
                        else
                        {
                            list.Add((Token.Value8, $"{baseTenToken}"));
                        }
                        continue;
                    }

                    if (token.ToUpper() != token.ToLower() && Enum.TryParse<Condition>(token, out Condition conditionResult))
                    {
                        list.Add((Token.Condition, $"{(byte)conditionResult}"));
                        continue;
                    }

                    if (byte.TryParse(token, out byte byteResult))
                    {
                        if (byteResult < 0b00010000)
                        {
                            list.Add((Token.Value4, token));
                        }
                        else if (byteResult < 0b10000000)
                        {
                            list.Add((Token.Value7, token));
                        }
                        else
                        {
                            list.Add((Token.Value8, token));
                        }
                        continue;
                    }

                    throw new FormatException($"Line {lineIndex}: Token '{token}' was not recognised.");
                }
                programTokens[lineIndex] = list.ToArray();
            }
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                (Token, string)[] lineTokens = programTokens[lineIndex];
                int labelCount = 0;
                for (int i = 0; i < lineTokens.Length; i++)
                {
                    if (lineTokens[i].Item1 == Token.Label)
                    {
                        labels.Add(lineTokens[i].Item2, lineIndex);
                        labelCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (labelCount > 0)
                {
                    (Token, string)[] newLineTokens = new (Token, string)[lineTokens.Length - labelCount];
                    for (int i = labelCount; i < lineTokens.Length; i++)
                    {
                        newLineTokens[i - labelCount] = lineTokens[i];
                    }
                    programTokens[lineIndex] = newLineTokens;
                    lineTokens = newLineTokens;
                }

                if (lineTokens[0].Item1 != Token.Opcode || lineTokens.Where((x) => { return x.Item1 == Token.Opcode; }).ToArray().Length != 1)
                {
                    throw new FormatException($"Line {lineIndex}: Line must have a singular leading opcode.");
                }

                if (lineTokens.Length > 4)
                {
                    throw new FormatException($"Line {lineIndex}: Line cannot have more than three operands.");
                }
            }
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                (Token, string)[] lineTokens = programTokens[lineIndex];
                for (int i = 0; i < lineTokens.Length; i++)
                {
                    if (lineTokens[i].Item1 == Token.Label)
                    {
                        try
                        {
                            lineTokens[i].Item2 = $"{labels[lineTokens[i].Item2]}";
                        }
                        catch (KeyNotFoundException)
                        {
                            throw new FormatException($"Line {lineIndex}: Label '{lineTokens[i].Item2}' doesn't point anywhere.");
                        }
                        lineTokens[i].Item1 = Token.Value7;
                    }
                }
            }

            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                (Token, string)[] lineTokens = programTokens[lineIndex];

                Opcode opcode = (Opcode)Enum.Parse(typeof(Opcode), lineTokens[0].Item2);

                Token[][] expectedMatches;

                switch (opcode)
                {
                    case Opcode.NOP:
                    case Opcode.HLT:
                    case Opcode.RET:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {

                            },
                        };
                        break;
                    case Opcode.ADD:
                    case Opcode.SUB:
                    case Opcode.NOR:
                    case Opcode.AND:
                    case Opcode.XOR:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {
                                Token.Register,
                                Token.Register,
                                Token.Register,
                            },
                        };
                        break;
                    case Opcode.STR:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {
                                Token.Register,
                                Token.Register,
                            },
                            new Token[]
                            {
                                Token.Register,
                                Token.Register,
                                Token.Value4,
                            },
                        };
                        break;
                    case Opcode.LOD:
                    case Opcode.RSH:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {
                                Token.Register,
                                Token.Register,
                            },
                            new Token[]
                            {
                                Token.Register,
                                Token.Register,
                                Token.Register,
                            },
                        };
                        break;
                    case Opcode.LDI:
                    case Opcode.ADI:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {
                                Token.Register,
                                Token.Value8,
                            },
                        };
                        break;
                    case Opcode.BNC:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {
                                Token.Condition,
                                Token.Value7,
                            },
                        };
                        break;
                    case Opcode.JMP:
                    case Opcode.CAL:
                        expectedMatches = new Token[][]
                        {
                            new Token[]
                            {
                                Token.Value7,
                            },
                        };
                        break;
                    default:
                        throw new NotImplementedException();

                }

                bool success = false;
                List<string> values = new List<string>();
                foreach (Token[] match in expectedMatches)
                {
                    values.Clear();
                    if (match.Length != lineTokens.Length - 1)
                    {
                        continue;
                    }
                    success = true;
                    for (int i = 0; i < match.Length; i++)
                    {
                        if (match[i] != lineTokens[i + 1].Item1)
                        {
                            if 
                            (!(
                                match[i] == Token.Value7 && lineTokens[i + 1].Item1 == Token.Value4 ||
                                match[i] == Token.Value8 && lineTokens[i + 1].Item1 == Token.Value7 ||
                                match[i] == Token.Value8 && lineTokens[i + 1].Item1 == Token.Value4
                            ))
                            {

                                success = false;
                                break;
                            }
                        }
                        values.Add(lineTokens[i + 1].Item2);
                    }
                    if (success)
                    {
                        break;
                    }
                }
                if (!success)
                {
                    throw new FormatException($"Line {lineIndex}: Opcode '{opcode}' cannot accept these operands.");
                }

                Program.Add(new ProgramLine(opcode, values.ToArray()));
                ProgramText = program;
            }

            ProgramBinary = new string[Program.Count];
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                ProgramLine line = Program[lineIndex];

                string opcode()
                {
                    return get4((byte)line.Opcode);
                }
                string get8(byte value)
                {
                    string b = Convert.ToString(value, 2).PadLeft(8, '0');
                    return b.Substring(0, 4) + " " + b.Substring(4);
                }
                string get7(byte value)
                {
                    string b = Convert.ToString(value, 2).PadLeft(8, '0');
                    return "0" + b.Substring(1, 3) + " " + b.Substring(4);
                }
                string get4(byte value)
                {
                    string b = Convert.ToString(value, 2).PadLeft(8, '0');
                    return b.Substring(4);
                }
                string combine(params string[] parts)
                {
                    string s = parts.Aggregate(string.Empty, (i, x) => $"{i} {x}");
                    return s.Substring(1);
                }
                string blank = "0000";
                switch (line.Opcode)
                {
                    case Opcode.NOP:
                    case Opcode.RET:
                    case Opcode.HLT:
                        ProgramBinary[lineIndex] = combine(opcode(), blank, blank, blank);
                        break;
                    case Opcode.ADD:
                    case Opcode.SUB:
                    case Opcode.NOR:
                    case Opcode.AND:
                    case Opcode.XOR:
                        ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), get4(line.Operands[2]));
                        break;
                    case Opcode.LDI:
                    case Opcode.ADI:
                        ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get8(line.Operands[1]));
                        break;
                    case Opcode.JMP:
                    case Opcode.CAL:
                        ProgramBinary[lineIndex] = combine(opcode(), blank, get7(line.Operands[0]));
                        break;
                    case Opcode.BNC:
                        ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get7(line.Operands[1]));
                        break;
                    case Opcode.RSH:
                    case Opcode.LOD:
                        if (line.Operands.Length == 2)
                        {
                            ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), blank, get4(line.Operands[1]));
                        }
                        else
                        {
                            ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), get4(line.Operands[2]));
                        }
                        break;
                    case Opcode.STR:
                        if (line.Operands.Length == 2)
                        {
                            ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), blank);
                        }
                        else
                        {
                            ProgramBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), get4(line.Operands[2]));
                        }
                        break;
                }
            }
        }

        public void PushCallStack(byte newAddress)
        {
            byte push = (byte)(ProgramCounter);
            CallStack.Push(push);
            ProgramCounter = newAddress;
            if (CallStack.Count > 8)
            {
                Stack<byte> temp = new Stack<byte>();
                for (int i = 0; i < 8; i++)
                {
                    temp.Push(CallStack.Pop());
                }
                CallStack.Clear();
                while (temp.Count > 0)
                {
                    CallStack.Push(temp.Pop());
                }
            }
        }
        public void PopCallStack()
        {
            ProgramCounter = (byte)(CallStack.Pop() + 1);
        }
        public byte GetMemory(byte index)
        {
            return Memory[index];
        }
        public void SetMemory(byte index, byte value)
        {
            Memory[index] = value;
        }
        public byte GetRegister(byte index)
        {
            if (index < 0 || index >= Registers.Length) throw new IndexOutOfRangeException();
            return Registers[index];
        }
        public void SetRegister(byte index, byte value)
        {
            if (index < 0 || index >= Registers.Length) throw new IndexOutOfRangeException();
            if (index == 0) return;
            Registers[index] = value;
        }
    }
}
