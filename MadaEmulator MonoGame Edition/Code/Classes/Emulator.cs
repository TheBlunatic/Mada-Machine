using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public interface IPeripheral
    {
        public void InputToEmulator(Emulator emulator);
        public void OutputFromEmulator(Emulator emulator);
        public IPeripheral Clone();
    }
    public class Emulator
    {
        // Constants
        public const byte MEMORY_IN_RANDOMIZER = 159;
        public const byte MEMORY_IN_CONTROLLER_BYTE = 255;
        public const byte MEMORY_IN_CONTROLLER_BUTTONS = 191; // Red Green Blue Enter - - - -

        public static HashSet<byte> MEMORY_IN_ALL = 
            new HashSet<byte>() { MEMORY_IN_RANDOMIZER, MEMORY_IN_CONTROLLER_BYTE, MEMORY_IN_CONTROLLER_BUTTONS }
            .ToHashSet();

        public static readonly HashSet<byte> MEMORY_OUT_SCREEN = new HashSet<byte> { 246, 247, 248, 249, 250, 251, 252, 253 };
        public const byte MEMORY_OUT_FLAGS = 245; // - - - - - ScreenOn ByteAcknowledge ColourAcknowledge

        public static HashSet<byte> MEMORY_OUT_ALL = 
            new HashSet<byte>() { MEMORY_OUT_FLAGS }
            .Union(MEMORY_OUT_SCREEN)
            .ToHashSet();

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
            public bool IsHalted { get; set; }
            public byte[] Memory { get; set; }
            public int RandomSeed { get; set; }

            public Controller Controller { get; set; }
            public Screen Screen { get; set; }

            public RunImage(byte[] registers, Stack<byte> callStack, byte programCounter, int instructionCounter, Dictionary<Condition, bool> flags, bool halted, byte[] memory, int randomSeed, Controller controller, Screen screen)
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
                IsHalted = halted;

                Memory = new byte[memory.Length];
                for (int i = 0; i < memory.Length; i++)
                {
                    Memory[i] = memory[i];
                }

                RandomSeed = randomSeed;

                Controller = (Controller)controller.Clone();
                Screen = (Screen)screen.Clone();
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
        public int RandomSeed { get; private set; }

        public Dictionary<Condition, bool> Flags { get; private set; }

        public Controller Controller;
        public Screen Screen;

        public string[] ProgramText { get; private set; }
        public List<ProgramLine> Program { get; private set; }
        public string[] ProgramBinary { get; private set; }

        public Stack<RunImage> History { get; private set; }

        // Constructors
        public Emulator(string path)
        {
            Program = new List<ProgramLine>();
            LoadProgram(path);

            Reset();
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

            Controller = new Controller(this);
            Screen = new Screen(this);

            RandomSeed = new Random().Next();
            Memory[MEMORY_IN_RANDOMIZER] = (byte)RandomSeed;

            History = new Stack<RunImage>();
            PushImage();
        }
        public void RestoreImage(RunImage runImage)
        {
            Registers = runImage.Registers;
            CallStack = runImage.CallStack;
            ProgramCounter = runImage.ProgramCounter;
            InstructionCounter = runImage.InstructionCounter;
            Flags = runImage.Flags;
            IsHalted = runImage.IsHalted;
            Memory = runImage.Memory;
            RandomSeed = runImage.RandomSeed;
            Controller = runImage.Controller;
            Screen = runImage.Screen;
        }
        public void PushImage()
        {
            History.Push(new RunImage(Registers, CallStack, ProgramCounter, InstructionCounter, Flags, IsHalted, Memory, RandomSeed, Controller, Screen));
        }

        public byte Add(byte x, byte y)
        {
            int resultInt = (int)x + (int)y;
            byte result = (byte)(x + y);
            SetFlag(Condition.Z, result == 0);
            SetFlag(Condition.NZ, !GetFlag(Condition.Z));

            SetFlag(Condition.C, resultInt > byte.MaxValue);
            SetFlag(Condition.NC, !GetFlag(Condition.C));

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
            SetFlag(Condition.Z, result == 0);
            SetFlag(Condition.NZ, !GetFlag(Condition.Z));

            SetFlag(Condition.C, false);
            SetFlag(Condition.NC, !GetFlag(Condition.C));

            return result;
        }
        public byte And(byte x, byte y)
        {
            byte result = (byte)(x & y);
            SetFlag(Condition.Z, result == 0);
            SetFlag(Condition.NZ, !GetFlag(Condition.Z));

            SetFlag(Condition.C, false);
            SetFlag(Condition.NC, !GetFlag(Condition.C));

            return result;
        }
        public byte Xor(byte x, byte y)
        {
            byte result = (byte)(x ^ y);
            SetFlag(Condition.Z, result == 0);
            SetFlag(Condition.NZ, !GetFlag(Condition.Z));

            SetFlag(Condition.C, false);
            SetFlag(Condition.NC, !GetFlag(Condition.C));

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

        public void Rewind()
        {
            if (History.Count > 1)
            {
                History.Pop();
                RestoreImage(History.Pop());
                PushImage();
            }
        }
        public void Step()
        {
            if (IsHalted) return;

            Controller.InputToEmulator(this);
            Screen.InputToEmulator(this);

            switch (Program[ProgramCounter].Opcode)
            {
                case Opcode.NOP:
                    IncrementProgramCounter();
                    break;
                case Opcode.ADD:
                    SetRegister(Program[ProgramCounter].Operands[2], Add(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    IncrementProgramCounter();
                    break;
                case Opcode.SUB:
                    SetRegister(Program[ProgramCounter].Operands[2], Sub(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    IncrementProgramCounter();
                    break;
                case Opcode.NOR:
                    SetRegister(Program[ProgramCounter].Operands[2], Nor(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    IncrementProgramCounter();
                    break;
                case Opcode.AND:
                    SetRegister(Program[ProgramCounter].Operands[2], And(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    IncrementProgramCounter();
                    break;
                case Opcode.XOR:
                    SetRegister(Program[ProgramCounter].Operands[2], Xor(GetRegister(Program[ProgramCounter].Operands[0]), GetRegister(Program[ProgramCounter].Operands[1])));
                    IncrementProgramCounter();
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
                    IncrementProgramCounter();
                    break;
                case Opcode.LDI:
                    SetRegister(Program[ProgramCounter].Operands[0], Program[ProgramCounter].Operands[1]);
                    IncrementProgramCounter();
                    break;
                case Opcode.ADI:
                    SetRegister(Program[ProgramCounter].Operands[0], Add(GetRegister(Program[ProgramCounter].Operands[0]), Program[ProgramCounter].Operands[1]));
                    IncrementProgramCounter();
                    break;
                case Opcode.JMP:
                    SetProgramCounter(Program[ProgramCounter].Operands[0]);
                    break;
                case Opcode.BNC:
                    if (Flags[(Condition)Program[ProgramCounter].Operands[0]])
                    {
                        SetProgramCounter(Program[ProgramCounter].Operands[1]);
                    }
                    else
                    {
                        IncrementProgramCounter();
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
                    IncrementProgramCounter();
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
                    IncrementProgramCounter();
                    break;
                default:
                    throw new Exception();
            }

            InstructionCounter++;

            RandomSeed = new Random(RandomSeed).Next();
            Memory[MEMORY_IN_RANDOMIZER] = (byte)RandomSeed;

            Controller.OutputFromEmulator(this);
            Screen.OutputFromEmulator(this);

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

        public void IncrementProgramCounter()
        {
            SetProgramCounter((byte)(ProgramCounter + 1));
        }
        public void SetProgramCounter(byte value)
        {
            ProgramCounter = value;

            if (Program[ProgramCounter].Opcode == Opcode.HLT)
            {
                IsHalted = true;
                ProgramCounter = 0;
            }
        }

        public void PushCallStack(byte newAddress)
        {
            byte push = (byte)(ProgramCounter);
            CallStack.Push(push);
            SetProgramCounter(newAddress);
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
            SetProgramCounter((byte)(CallStack.Pop() + 1));
        }

        public byte GetMemory(byte index)
        {
            if (MEMORY_OUT_ALL.Contains(index)) return 0;
            return Memory[index];
        }
        public void SetMemory(byte index, byte value)
        {
            if (MEMORY_IN_ALL.Contains(index)) return;
            Memory[index] = value;
        }

        public bool GetFlag(Condition condition)
        {
            return Flags[condition];
        }
        public void SetFlag(Condition condition, bool value)
        {
            Flags[condition] = value;
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
