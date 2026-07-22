using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator
{
    internal class Program
    {
        // Classes
        class ProgramLine
        {
            public Opcode Opcode {  get; set; }
            public byte[] Operands { get; set; }

            public ProgramLine(Opcode opcode, string[] operands)
            {
                Opcode = opcode;
                Operands = operands.Select((i) => { return byte.Parse(i); }).ToArray();
            }
        }
        class RunImage
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
        enum Opcode : byte
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
        enum Condition : byte
        {
            Z = 0b00000000,
            NZ = 0b00000100,
            C = 0b00001000,
            NC = 0b00001100,
        }
        enum Token
        {
            Label,
            Opcode,
            Register,
            Value4,
            Value7,
            Value8,
            Condition,
        }

        // Fields
        static byte[] _registers = new byte[16];
        static Stack<byte> _callStack = new Stack<byte>();
        static byte _programCounter = 0;
        static int _instructionCounter = 0;
        static bool _halted = false;
        static byte[] _memory = new byte[256];
        static bool _writeMachineCode = false;

        static Dictionary<Condition, bool> _flags = new Dictionary<Condition, bool>()
        {
            {Condition.Z, false },
            {Condition.NZ, false },
            {Condition.C, false },
            {Condition.NC, false },
        };

        static string[] _programText;
        static List<ProgramLine> _program = new List<ProgramLine>();
        static string[] _programBinary;

        static Stack<RunImage> _history = new Stack<RunImage>();

        // Methods
        static void Main(string[] args)
        {
            Console.WindowHeight = 40;
            bool debug = true;
            if (debug)
            {

                Console.WriteLine("Enter program name:");
                LoadProgram(Console.ReadLine());
                RunProgram(true);
                Console.WriteLine("Program ended. Any input will close the program.");
                Console.ReadKey();
            }
            else
            {

                Console.WriteLine("Enter program name:");
                try
                {
                    LoadProgram(Console.ReadLine());
                    RunProgram(true);
                }
                catch (Exception e)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("ERROR");
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.WriteLine("Program ended. Any input will close the program.");
                Console.ReadKey();
            }
        }
        static void UpdateDisplay()
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
            writeLineAtX(30, $"PROGRAM COUNTER: {Convert.ToString(_programCounter, 2).PadLeft(8, '0')} ({_programCounter})");
            writeLineAtX(30, string.Empty);
            writeLineAtX(30, $"INSTRUCTION COUNTER: {_instructionCounter}");
            writeLineAtX(30, string.Empty);
            writeLineAtX(30, $"IS HALTED: {_halted}");
            writeLineAtX(30, string.Empty); 
            writeLineAtX(30, $"FLAGS:");
            foreach (KeyValuePair<Condition, bool> kvp in _flags)
            {
                writeLineAtX(30, $"{kvp.Key} : {kvp.Value}");
            }
            writeLineAtX(30, string.Empty);
            writeLineAtX(30, $"CALL STACK:");
            foreach (byte c in _callStack)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                writeLineAtX(30, $"0b{Convert.ToString(c, 2).PadLeft(8, '0')} ({c})");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            if (_callStack.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                writeLineAtX(30, $"EMPTY");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.CursorTop = 0;
            writeLineAtX(68, $"PROGRAM:");
            for (int i = 0; i < _programText.Length; i++)
            {
                if (i == _programCounter)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                if (_writeMachineCode)
                {
                    writeLineAtX(68, $"{_programBinary[i]}");
                }
                else
                {
                    writeLineAtX(68, $"{_programText[i]}");
                }
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            Console.CursorTop = 0;
            writeLineAtX(0, $"REGISTERS:");
            for (byte i = 0; i < _registers.Length; i++)
            {
                writeLineAtX(0, $"{$"r{i}".PadLeft(3, ' ')} : 0b{Convert.ToString(GetRegister(i), 2).PadLeft(8, '0')} ({GetRegister(i)})");
            }
            writeLineAtX(0, string.Empty);
            writeLineAtX(0, $"MEMORY:");
            for (int y = 0; y < 16; y++)
            {
                byte[] array = new byte[16];
                Array.ConstrainedCopy(_memory, y * 16, array, 0, 16);
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
        static void RestoreImage(RunImage runImage)
        {
            _registers = runImage.Registers;
            _callStack = runImage.CallStack;
            _programCounter = runImage.ProgramCounter;
            _instructionCounter = runImage.InstructionCounter;
            _flags = runImage.Flags;
            _halted = runImage.Halted;
            _memory = runImage.Memory;
        }
        static void PushImage()
        {
            _history.Push(new RunImage(_registers, _callStack, _programCounter, _instructionCounter, _flags, _halted, _memory));
        }

        static byte Add(byte x, byte y)
        {
            int resultInt = (int)x + (int)y;
            byte result = (byte)(x + y);
            _flags[Condition.Z] = result == 0;
            _flags[Condition.NZ] = !_flags[Condition.Z];

            _flags[Condition.C] = resultInt > byte.MaxValue;
            _flags[Condition.NC] = !_flags[Condition.C];

            return result;
        }
        static byte Sub(byte x, byte y)
        {
            y = (byte)(~y);
            y++;
            return Add(x, y);
        }
        static byte Nor(byte x, byte y)
        {
            byte result = (byte)~(x | y);
            _flags[Condition.Z] = result == 0;
            _flags[Condition.NZ] = !_flags[Condition.Z];

            _flags[Condition.C] = false;
            _flags[Condition.NC] = !_flags[Condition.C];

            return result;
        }
        static byte And(byte x, byte y)
        {
            byte result = (byte)(x & y);
            _flags[Condition.Z] = result == 0;
            _flags[Condition.NZ] = !_flags[Condition.Z];

            _flags[Condition.C] = false;
            _flags[Condition.NC] = !_flags[Condition.C];

            return result;
        }
        static byte Xor(byte x, byte y)
        {
            byte result = (byte)(x ^ y);
            _flags[Condition.Z] = result == 0;
            _flags[Condition.NZ] = !_flags[Condition.Z];

            _flags[Condition.C] = false;
            _flags[Condition.NC] = !_flags[Condition.C];

            return result;
        }
        static byte Rsh(byte x)
        {
            byte result = (byte)(x >> 1);
            return result;
        }
        static byte Rsh(byte x, byte y)
        {
            byte result = (byte)((x + y) >> 1);
            return result;
        }

        static void RunProgram(bool step)
        {
            _instructionCounter = 0;
            _halted = false;
            _history.Clear();
            PushImage();
            for (_programCounter = 0; _programCounter < _program.Count;)
            {
                if (_halted)
                {
                    step = true;
                }
                bool skipStep = _halted;
                if (step)
                {
                    UpdateDisplay();
                    string key = Console.ReadKey().Key.ToString();
                    switch (key)
                    {
                        case "A":
                            skipStep = true;
                            if (_history.Count > 1)
                            {
                                _history.Pop();
                                RestoreImage(_history.Pop());
                                PushImage();
                                step = true;
                            }
                            else if (_history.Count == 1)
                            {
                                RestoreImage(_history.Pop());
                                PushImage();
                                step = true;
                            }
                            break;
                        case "D":
                            break;
                        case "S":
                            _writeMachineCode = !_writeMachineCode;
                            skipStep = true;
                            break;
                        case "R":
                            skipStep = true;
                            if (_history.Count > 0)
                            {
                                while (_history.Count > 1)
                                {
                                    _history.Pop();
                                }
                                RestoreImage(_history.Pop());
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
                if (_halted)
                {
                    step = true;
                }
            }
            UpdateDisplay();
        }
        static void Step()
        {
            switch (_program[_programCounter].Opcode)
            {
                case Opcode.NOP:
                    _programCounter++;
                    break;
                case Opcode.HLT:
                    _halted = true;
                    break;
                case Opcode.ADD:
                    SetRegister(_program[_programCounter].Operands[2], Add(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1])));
                    _programCounter++;
                    break;
                case Opcode.SUB:
                    SetRegister(_program[_programCounter].Operands[2], Sub(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1])));
                    _programCounter++;
                    break;
                case Opcode.NOR:
                    SetRegister(_program[_programCounter].Operands[2], Nor(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1])));
                    _programCounter++;
                    break;
                case Opcode.AND:
                    SetRegister(_program[_programCounter].Operands[2], And(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1])));
                    _programCounter++;
                    break;
                case Opcode.XOR:
                    SetRegister(_program[_programCounter].Operands[2], Xor(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1])));
                    _programCounter++;
                    break;
                case Opcode.RSH:
                    if (_program[_programCounter].Operands.Length == 2)
                    {
                        SetRegister(_program[_programCounter].Operands[1], Rsh(GetRegister(_program[_programCounter].Operands[0])));
                    }
                    else
                    {
                        SetRegister(_program[_programCounter].Operands[1], Rsh(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1])));
                    }
                    _programCounter++;
                    break;
                case Opcode.LDI:
                    SetRegister(_program[_programCounter].Operands[0], _program[_programCounter].Operands[1]);
                    _programCounter++;
                    break;
                case Opcode.ADI:
                    SetRegister(_program[_programCounter].Operands[0], Add(GetRegister(_program[_programCounter].Operands[0]), _program[_programCounter].Operands[1]));
                    _programCounter++;
                    break;
                case Opcode.JMP:
                    _programCounter = _program[_programCounter].Operands[0];
                    break;
                case Opcode.BNC:
                    if (_flags[(Condition)_program[_programCounter].Operands[0]])
                    {
                        _programCounter = _program[_programCounter].Operands[1];
                    }
                    else
                    {
                        _programCounter++;
                    }
                    break;
                case Opcode.CAL:
                    PushCallStack(_program[_programCounter].Operands[0]);
                    break;
                case Opcode.RET:
                    PopCallStack();
                    break;
                case Opcode.LOD:
                    if (_program[_programCounter].Operands.Length == 2)
                    {
                        SetRegister(_program[_programCounter].Operands[1], GetMemory(GetRegister(_program[_programCounter].Operands[0])));
                    }
                    else
                    {
                        SetRegister(_program[_programCounter].Operands[2], GetMemory((byte)(GetRegister(_program[_programCounter].Operands[0]) + GetRegister(_program[_programCounter].Operands[1]))));
                    }
                    _programCounter++;
                    break;
                case Opcode.STR:
                    if (_program[_programCounter].Operands.Length == 2)
                    {
                        SetMemory(GetRegister(_program[_programCounter].Operands[0]), GetRegister(_program[_programCounter].Operands[1]));
                    }
                    else
                    {
                        SetMemory((byte)(GetRegister(_program[_programCounter].Operands[0]) + _program[_programCounter].Operands[2]), GetRegister(_program[_programCounter].Operands[1]));
                    }
                    _programCounter++;
                    break;
                default:
                    throw new Exception();
            }
            _instructionCounter++;
            PushImage();
        }

        static void LoadProgram(string programName)
        {
            _program.Clear();
            Dictionary<string, byte> labels = new Dictionary<string, byte>();
            string[] program = File.ReadAllLines($"programs\\{programName}.txt");
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

                _program.Add(new ProgramLine(opcode, values.ToArray()));
                _programText = program;
            }

            _programBinary = new string[_program.Count];
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                ProgramLine line = _program[lineIndex];

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
                        _programBinary[lineIndex] = combine(opcode(), blank, blank, blank);
                        break;
                    case Opcode.ADD:
                    case Opcode.SUB:
                    case Opcode.NOR:
                    case Opcode.AND:
                    case Opcode.XOR:
                        _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), get4(line.Operands[2]));
                        break;
                    case Opcode.LDI:
                    case Opcode.ADI:
                        _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get8(line.Operands[1]));
                        break;
                    case Opcode.JMP:
                    case Opcode.CAL:
                        _programBinary[lineIndex] = combine(opcode(), blank, get7(line.Operands[0]));
                        break;
                    case Opcode.BNC:
                        _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get7(line.Operands[1]));
                        break;
                    case Opcode.RSH:
                    case Opcode.LOD:
                        if (line.Operands.Length == 2)
                        {
                            _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), blank, get4(line.Operands[1]));
                        }
                        else
                        {
                            _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), get4(line.Operands[2]));
                        }
                        break;
                    case Opcode.STR:
                        if (line.Operands.Length == 2)
                        {
                            _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), blank);
                        }
                        else
                        {
                            _programBinary[lineIndex] = combine(opcode(), get4(line.Operands[0]), get4(line.Operands[1]), get4(line.Operands[2]));
                        }
                        break;
                }
            }
        }

        static void PushCallStack(byte newAddress)
        {
            byte push = (byte)(_programCounter);
            _callStack.Push(push);
            _programCounter = newAddress;
            if (_callStack.Count > 8)
            {
                Stack<byte> temp = new Stack<byte>();
                for (int i = 0; i < 8; i++)
                {
                    temp.Push(_callStack.Pop());
                }
                _callStack.Clear();
                while (temp.Count > 0)
                {
                    _callStack.Push(temp.Pop());
                }
            }
        }
        static void PopCallStack()
        {
            _programCounter = (byte)(_callStack.Pop() + 1);
        }
        static byte GetMemory(byte index)
        {
            return _memory[index];
        }
        static void SetMemory(byte index, byte value)
        {
            _memory[index] = value;
        }
        static byte GetRegister(byte index)
        {
            if (index < 0 || index >= _registers.Length) throw new IndexOutOfRangeException();
            return _registers[index];
        }
        static void SetRegister(byte index, byte value)
        {
            if (index < 0 || index >= _registers.Length) throw new IndexOutOfRangeException();
            if (index == 0) return;
            _registers[index] = value;
        }
    }
}
