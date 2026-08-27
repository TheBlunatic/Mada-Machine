using Blunatic.Core;
using Blunatic.Mgc;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MadaEmulator_MonoGame_Edition.Emulator;

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
        public const byte MEMORY_IN_NIBBLE_SWAPPER = 63;

        public static readonly HashSet<byte> MEMORY_OUT_SCREEN = new HashSet<byte> { 246, 247, 248, 249, 250, 251, 252, 253 };
        public const byte MEMORY_OUT_FLAGS = 245; // - - - - - ScreenOn ByteAcknowledge ColourAcknowledge
        public const byte MEMORY_OUT_NIBBLE_SWAPPER = 31;

        public static readonly string[] MEMORY_RESERVATION_IDENTIFIERS = new string[256];
        public static readonly string[] MEMORY_RESERVATION_DESCRIPTIONS = new string[256];
        public static readonly bool[] INPUT_ONLY_MEMORY = new bool[256];
        public static readonly bool[] OUTPUT_ONLY_MEMORY = new bool[256];

        static Emulator()
        {
            for (int i = 0; i < MEMORY_RESERVATION_IDENTIFIERS.Length; i++)
            {
                MEMORY_RESERVATION_IDENTIFIERS[i] = "Standard Memory";
            }

            void register(string identifier, bool isInput, params byte[] addresses)
            {
                registerWithDescription(identifier, null, isInput, addresses);
            }
            void registerWithDescription(string identifier, string description, bool isInput, params byte[] addresses)
            {
                bool[] record = isInput ? INPUT_ONLY_MEMORY : OUTPUT_ONLY_MEMORY;
                foreach (byte address in addresses)
                {
                    MEMORY_RESERVATION_IDENTIFIERS[address] = identifier;
                    MEMORY_RESERVATION_DESCRIPTIONS[address] = description;
                    record[address] = true;
                }
            }

            register("Randomiser Input", true, 159);
            register("Controller Byte Input", true, 255);
            registerWithDescription("Controller Buttons Input RGBE----", "R: Red Button State\nG: Green Button State\nB: Blue Button State\nE: Enter Button State", true, 191);
            register("Nibble Swapper Return", true, 63);

            register("Screen Lights Output", false, 246, 247, 248, 249, 250, 251, 252, 253);
            registerWithDescription("Misc Flags Output -----SBC", "S: Screen Off\nB: Controller Enter Acknowledge\nC: Controller Colours Acknowledge", false, 245);
            register("Nibble Swapper Take", false, 31);
        }

        public static Dictionary<Token, string> PROGRAM_TOKEN_FORMAT_HEADERS = new Dictionary<Token, string>()
        {
            { Token.Value8, Fm.Fg(Color.White) },
            { Token.Value7, Fm.Fg(Color.DarkGray) },
            { Token.Value4, Fm.Fg(Color.White) },
            { Token.Opcode, Fm.Fg(Color.Yellow) },
            { Token.Label, Fm.Fg(Color.DarkGray) },
            { Token.Condition, Fm.Fg(Color.Orange) },
            { Token.Register, Fm.Fg(Color.LightBlue) },
        };

        // Structs
        public struct ProgramToken
        {
            public string InternalString { get; set; }
            public string DisplayString { get; set; }
            public Token Token { get; set; }

            public ProgramToken(Token token, string internalString) : this(token, internalString, internalString)
            {

            }
            public ProgramToken(Token token, string internalString, string displayString)
            {
                InternalString = internalString;
                Token = token;
                DisplayString = displayString;
            }

            public override string ToString()
            {
                return DisplayString;
            }
            public string GetFormattedString(MonoGameInstance mgi)
            {
                if (!PROGRAM_TOKEN_FORMAT_HEADERS.TryGetValue(Token, out string header))
                {
                    header = $"{Fm.Fg(Color.White)}";
                }
                return $"{header}{DisplayString}";
            }
        }

        // Classes
        public class ProgramLine
        {
            public Opcode Opcode {  get; set; }
            public byte[] Operands { get; set; }
            public ProgramToken[] Tokens { get; set; }
            public int[] PrecedingWhitespace { get; set; }
            public string Comment { get; set; }
            public string MachineCode { get; set; }

            public ProgramLine(Opcode opcode, string[] operands, ProgramToken[] tokens, int[] precedingWhitespace, string comment = null)
            {
                Opcode = opcode;
                Operands = operands.Select((i) => { return byte.Parse(i); }).ToArray();
                Tokens = tokens;
                Comment = comment;
                PrecedingWhitespace = precedingWhitespace;
                MachineCode = ConvertProgramLineToMachineCode();
            }
            public ProgramLine(Random rng) : this([(byte)rng.Next(), (byte)rng.Next()])
            {

            }
            public ProgramLine(byte[] machineCodeBytes) : this(machineCodeBytes.Aggregate(string.Empty, (i, x) => $"{i}{Convert.ToString(x, 2).PadLeft(8, '0')}"))
            {

            }
            public ProgramLine(string machineCode)
            {
                if (!Regex.IsMatch(machineCode, "^( )*((0|1)( )*){16}$")) throw new FormatException($"Cannot parse the following into machine code: '{machineCode}'");
                string compactMachineCode = machineCode.Split(' ').Where((x) => x.Length != 0).Aggregate(string.Empty, (i, x) => $"{i}{x}");

                Comment = null;
                MachineCode = $"{compactMachineCode.Substring(0, 4)} {compactMachineCode.Substring(4, 4)} {compactMachineCode.Substring(8, 4)} {compactMachineCode.Substring(12, 4)}";

                Opcode = (Opcode)Convert.ToByte(compactMachineCode.Substring(0, 4), 2);

                List<ProgramToken> tokens = new List<ProgramToken>() { new ProgramToken(Token.Opcode, $"{Opcode}", $"{Opcode}") };

                byte addRegister(int index)
                {
                    byte returner = Convert.ToByte(compactMachineCode.Substring(index, 4), 2);
                    tokens.Add(new ProgramToken(Token.Register, $"{returner}", $"r{returner}"));
                    return returner;
                }
                byte addValue8(int index)
                {
                    byte returner = Convert.ToByte(compactMachineCode.Substring(index, 8), 2);
                    tokens.Add(new ProgramToken(Token.Value8, $"{returner}"));
                    return returner;
                }
                byte addValue7(int index)
                {
                    byte returner = Convert.ToByte(compactMachineCode.Substring(index, 7), 2);
                    tokens.Add(new ProgramToken(Token.Value7, $"{returner}"));
                    return returner;
                }
                byte addValue4(int index)
                {
                    byte returner = Convert.ToByte(compactMachineCode.Substring(index, 4), 2);
                    tokens.Add(new ProgramToken(Token.Value4, $"{returner}"));
                    return returner;
                }
                byte addCondition(int index)
                {
                    byte returner = (byte)(Convert.ToByte(compactMachineCode.Substring(index, 4), 2) & 0b00001100);
                    tokens.Add(new ProgramToken(Token.Condition, $"{returner}", $"{(Condition)returner}"));
                    return returner;
                }

                switch (Opcode)
                {
                    case Opcode.NOP:
                    case Opcode.RET:
                    case Opcode.HLT:
                        {
                            Operands = [];
                        }
                        break;
                    case Opcode.ADD:
                    case Opcode.SUB:
                    case Opcode.NOR:
                    case Opcode.AND:
                    case Opcode.XOR:
                    case Opcode.RSH:
                    case Opcode.LOD:
                        {
                            Operands = [addRegister(4), addRegister(8), addRegister(12)];
                        }
                        break;
                    case Opcode.LDI:
                    case Opcode.ADI:
                        {
                            Operands = [addRegister(4), addValue8(8)];
                        }
                        break;
                    case Opcode.JMP:
                    case Opcode.CAL:
                        {
                            Operands = [addValue7(9)];
                        }
                        break;
                    case Opcode.BNC:
                        {
                            Operands = [addCondition(4), addValue7(9)];
                        }
                        break;
                    case Opcode.STR:
                        {
                            Operands = [addRegister(4), addRegister(8), addValue4(12)];
                        }
                        break;
                }

                Tokens = tokens.ToArray();
                PrecedingWhitespace = new int[tokens.Count + 1];
                for (int i = 1; i < PrecedingWhitespace.Length; i++) PrecedingWhitespace[i] = 1;
            }

            public string ConvertProgramLineToMachineCode()
            {
                string opcode()
                {
                    return get4((byte)Opcode);
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
                switch (Opcode)
                {
                    case Opcode.NOP:
                    case Opcode.RET:
                    case Opcode.HLT:
                        return (combine(opcode(), blank, blank, blank));
                    case Opcode.ADD:
                    case Opcode.SUB:
                    case Opcode.NOR:
                    case Opcode.AND:
                    case Opcode.XOR:
                        return (combine(opcode(), get4(Operands[0]), get4(Operands[1]), get4(Operands[2])));
                    case Opcode.LDI:
                    case Opcode.ADI:
                        return (combine(opcode(), get4(Operands[0]), get8(Operands[1])));
                    case Opcode.JMP:
                    case Opcode.CAL:
                        return (combine(opcode(), blank, get7(Operands[0])));
                    case Opcode.BNC:
                        return (combine(opcode(), get4(Operands[0]), get7(Operands[1])));
                    case Opcode.RSH:
                    case Opcode.LOD:
                        if (Operands.Length == 2)
                        {
                            return (combine(opcode(), get4(Operands[0]), blank, get4(Operands[1])));
                        }
                        else
                        {
                            return (combine(opcode(), get4(Operands[0]), get4(Operands[1]), get4(Operands[2])));
                        }
                    case Opcode.STR:
                        if (Operands.Length == 2)
                        {
                            return (combine(opcode(), get4(Operands[0]), get4(Operands[1]), blank));
                        }
                        else
                        {
                            return (combine(opcode(), get4(Operands[0]), get4(Operands[1]), get4(Operands[2])));
                        }
                }
                throw new Exception("Failed to generate machine code.");
            }

            public override string ToString()
            {
                IEnumerator<int> whitespaceEnumerator = PrecedingWhitespace.AsEnumerable().GetEnumerator();
                string getNextWhitespace()
                {
                    whitespaceEnumerator.MoveNext();
                    return new string(' ', whitespaceEnumerator.Current);
                }
                return Tokens.Aggregate(string.Empty, (i, x) => 
                {
                    return $"{i}{getNextWhitespace()}{x}";
                }) + (Comment == null ? string.Empty : $"{getNextWhitespace()}{Comment}");
            }
            public string ToFormattedString(MonoGameInstance mgi)
            {
                IEnumerator<int> whitespaceEnumerator = PrecedingWhitespace.AsEnumerable().GetEnumerator();
                string getNextWhitespace()
                {
                    whitespaceEnumerator.MoveNext();
                    return new string(' ', whitespaceEnumerator.Current);
                }
                return Tokens.Aggregate(string.Empty, (i, x) =>
                {
                    return $"{i}{getNextWhitespace()}{x.GetFormattedString(mgi)}";
                }) + (Comment == null ? string.Empty : $"{getNextWhitespace()}{Fm.Fg(Color.DarkGreen)}{Comment}");
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
            C = 0b00000000,
            NC = 0b00000100,
            Z = 0b00001000,
            NZ = 0b00001100,
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

        public bool RandomiseStoredMemoryOnReset { get; set; }
        public bool GenerateRandomProgramLinesWhenOutOfBounds { get; set; }

        public Dictionary<Condition, bool> Flags { get; private set; }

        public Controller Controller;
        public Screen Screen;

        public List<ProgramLine> Program { get; private set; }

        public Stack<RunImage> History { get; private set; }

        // Constructors
        public Emulator(string path)
        {
            GenerateRandomProgramLinesWhenOutOfBounds = false;
            RandomiseStoredMemoryOnReset = false;

            Program = new List<ProgramLine>();

            LoadProgram(path);

            Reset();
        }
        public Emulator(Random rng)
        {
            GenerateRandomProgramLinesWhenOutOfBounds = false;
            RandomiseStoredMemoryOnReset = false;

            Program = new List<ProgramLine>();

            for (int i = 0; i < 128; i++)
            {
                Program.Add(new ProgramLine(rng));
            }

            Reset();
        }

        // Methods
        public void Reset()
        {
            Registers = new byte[16];
            IsHalted = false;
            CallStack = new Stack<byte>();
            InstructionCounter = 0;
            Memory = new byte[256];

            Flags = new Dictionary<Condition, bool>()
            {
                {Condition.Z, false },
                {Condition.NZ, true },
                {Condition.C, false },
                {Condition.NC, true },
            };

            if (RandomiseStoredMemoryOnReset) RandomiseStoredValues(new Random());

            Controller = new Controller(this);
            Screen = new Screen(this);

            RandomSeed = new Random().Next();
            Memory[MEMORY_IN_RANDOMIZER] = (byte)RandomSeed;

            SetProgramCounter(0);

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

        public void RandomiseStoredValues(Random rng)
        {
            for (int i = 0; i < Registers.Length; i++)
            {
                SetRegister((byte)i, (byte)rng.Next());
            }

            for (int i = 0; i < Memory.Length; i++)
            {
                SetMemory((byte)i, (byte)rng.Next());
            }

            Add((byte)rng.Next(), (byte)rng.Next());

            for (int i = rng.Next(0, 9); i > 0; i--)
            {
                CallStack.Push((byte)(rng.Next() & 0b01111111));
            }
        }

        public byte Add(byte x, byte y, bool cin = false)
        {
            int resultInt = (int)x + (int)y + (cin ? 1 : 0);
            byte result = (byte)(resultInt);
            SetFlag(Condition.Z, result == 0);
            SetFlag(Condition.NZ, !GetFlag(Condition.Z));

            SetFlag(Condition.C, resultInt > byte.MaxValue);
            SetFlag(Condition.NC, !GetFlag(Condition.C));

            return result;
        }
        public byte Sub(byte x, byte y)
        {
            y = (byte)(~y);
            return Add(x, y, true);
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

        public Token[][] GetExpectedMatchesForOpcode(Opcode opcode)
        {
            return opcode switch
            {
                Opcode.NOP or Opcode.HLT or Opcode.RET => 
                [
                    [
                    ],
                ],
                Opcode.ADD or Opcode.SUB or Opcode.NOR or Opcode.AND or Opcode.XOR => 
                [
                    [
                        Token.Register,
                        Token.Register,
                        Token.Register,
                    ],
                ],
                Opcode.STR => 
                [
                    [
                        Token.Register,
                        Token.Register,
                    ],
                    [
                        Token.Register,
                        Token.Register,
                        Token.Value4,
                    ],
                ],
                Opcode.LOD or Opcode.RSH => 
                [
                    [
                        Token.Register,
                        Token.Register,
                    ],
                    [
                        Token.Register,
                        Token.Register,
                        Token.Register,
                    ],
                ],
                Opcode.LDI or Opcode.ADI => 
                [
                    [
                        Token.Register,
                        Token.Value8,
                    ],
                ],
                Opcode.BNC => 
                [
                    [
                        Token.Condition,
                        Token.Value7,
                    ],
                ],
                Opcode.JMP or Opcode.CAL => 
                [
                    [
                        Token.Value7,
                    ],
                ],
                _ => throw new NotImplementedException(),
            };
        }
        public Token[] GetPossibleParsesForToken(Token token)
        {
            return token switch
            {
                Token.Value7 =>
                [
                    Token.Value7,
                    Token.Value8,
                ],
                Token.Value4 =>
                [
                    Token.Value4,
                    Token.Value7,
                    Token.Value8,
                ],
                _ =>
                [
                    token
                ]
            };
        }
        public bool CanFirstTokenBeParsedAsOther(Token canThis, Token beParsedAsThis)
        {
            return GetPossibleParsesForToken(canThis).Contains(beParsedAsThis);
        }
        public void LoadProgram(string programPath)
        {
            List<string> programText = File.ReadAllLines(programPath).ToList();
            List<ProgramLine> program = new List<ProgramLine>();
            string[] programComments = new string[128];
            int[][] precedingWhitespace = new int[128][];

            Dictionary<string, byte> labels = new Dictionary<string, byte>();

            if (programText.Count > 128)
            {
                throw new Exception($"The instruction count ({programText.Count}) exceeds the maximum of 128.");
            }

            for (int lineIndex = 0; lineIndex < programText.Count; lineIndex++)
            {
                List<int> whitespaceList = new List<int>();
                int runLength = 0;
                bool counting = true;
                for (int i = 0; i < programText[lineIndex].Length && programText[lineIndex][i] != '/'; i++)
                {
                    if (programText[lineIndex][i] == ' ')
                    {
                        runLength++;
                        counting = true;
                    }
                    else if (counting)
                    {
                        whitespaceList.Add(runLength);
                        runLength = 0;
                        counting = false;
                    }
                }
                whitespaceList.Add(runLength);
                precedingWhitespace[lineIndex] = whitespaceList.ToArray();
                programText[lineIndex] = programText[lineIndex].Trim(' ');
            }

            ProgramToken[][] programTokens = new ProgramToken[programText.Count][];

            // Divide and categorise tokens
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                List<ProgramToken> list = new List<ProgramToken>();
                string line = programText[lineIndex];
                {
                    int index = Array.IndexOf(line.ToCharArray(), '/');
                    if (index != -1)
                    {
                        programComments[lineIndex] = line.Substring(index);
                        line = line.Substring(0, index);
                    }
                }
                string[] splitLine = line.Split(' ').Where((i) => { return i != string.Empty; }).ToArray();
                foreach (string token in splitLine)
                {
                    if (token[0] == '.')
                    {
                        list.Add(new ProgramToken(Token.Label, token, token));
                        continue;
                    }

                    if (token.ToUpper() != token.ToLower() && Enum.TryParse<Opcode>(token, out Opcode opcodeResult))
                    {
                        list.Add(new ProgramToken(Token.Opcode, token, token));
                        continue;
                    }

                    if (token[0] == 'r')
                    {
                        if (int.TryParse($"{token.Substring(1)}", out int resultInt) && resultInt >= 0 && resultInt < 16)
                        {
                            list.Add(new ProgramToken(Token.Register, token.Substring(1), token));
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
                            list.Add(new ProgramToken(Token.Value4, $"{baseTenToken}", token));
                        }
                        else if (baseTenToken < 0b10000000)
                        {
                            list.Add(new ProgramToken(Token.Value7, $"{baseTenToken}", token));
                        }
                        else
                        {
                            list.Add(new ProgramToken(Token.Value8, $"{baseTenToken}", token));
                        }
                        continue;
                    }

                    if (token.ToUpper() != token.ToLower() && Enum.TryParse<Condition>(token, out Condition conditionResult))
                    {
                        list.Add(new ProgramToken(Token.Condition, $"{(byte)conditionResult}", token));
                        continue;
                    }

                    if (byte.TryParse(token, out byte byteResult))
                    {
                        if (byteResult < 0b00010000)
                        {
                            list.Add(new ProgramToken(Token.Value4, token, token));
                        }
                        else if (byteResult < 0b10000000)
                        {
                            list.Add(new ProgramToken(Token.Value7, token, token));
                        }
                        else
                        {
                            list.Add(new ProgramToken(Token.Value8, token, token));
                        }
                        continue;
                    }

                    throw new FormatException($"Line {lineIndex}: Token '{token}' was not recognised.");
                }
                programTokens[lineIndex] = list.ToArray();
            }

            // Register label headers
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                ProgramToken[] lineTokens = programTokens[lineIndex];
                int labelCount = 0;
                for (int i = 0; i < lineTokens.Length; i++)
                {
                    if (lineTokens[i].Token == Token.Label)
                    {
                        if (labels.ContainsKey(lineTokens[i].InternalString))
                        {
                            throw new FormatException($"Line {lineIndex}: The label '{lineTokens[i].InternalString}' has already been defined at line {labels[lineTokens[i].InternalString]}.");
                        }
                        labels.Add(lineTokens[i].InternalString, lineIndex);
                        labelCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lineTokens.Length == labelCount)
                {
                    throw new FormatException($"Line {lineIndex}: Empty lines are not supported.");
                }

                if (lineTokens[labelCount].Token != Token.Opcode || lineTokens.Where((x) => { return x.Token == Token.Opcode; }).ToArray().Length != 1)
                {
                    throw new FormatException($"Line {lineIndex}: Line must have a singular leading opcode.");
                }

                if (lineTokens.Length > 4 + labelCount)
                {
                    throw new FormatException($"Line {lineIndex}: Line cannot have more than three operands.");
                }
            }

            // Replace label pointers with line values
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                ProgramToken[] lineTokens = programTokens[lineIndex];
                bool hitOpcode = false;
                for (int i = 0; i < lineTokens.Length; i++)
                {
                    if (lineTokens[i].Token == Token.Opcode)
                    {
                        hitOpcode = true;
                    }
                    if (!hitOpcode) continue;
                    if (lineTokens[i].Token == Token.Label)
                    {
                        try
                        {
                            lineTokens[i].InternalString = $"{labels[lineTokens[i].InternalString]}";
                        }
                        catch (KeyNotFoundException)
                        {
                            throw new FormatException($"Line {lineIndex}: Label '{lineTokens[i].InternalString}' doesn't point anywhere.");
                        }
                        lineTokens[i].Token = Token.Value7;
                    }
                }
            }

            // Form ProgramLines
            for (byte lineIndex = 0; lineIndex < programTokens.Length; lineIndex++)
            {
                ProgramToken[] lineTokens = programTokens[lineIndex];

                ProgramToken opcodeToken = lineTokens.Where((x) => x.Token == Token.Opcode).First();
                int opcodeIndex = Array.IndexOf(lineTokens, opcodeToken);
                Opcode opcode = (Opcode)Enum.Parse(typeof(Opcode), opcodeToken.InternalString);

                Token[][] expectedMatches = GetExpectedMatchesForOpcode(opcode);

                bool success = false;
                List<string> values = new List<string>();
                List<Token> actualTokens = new List<Token>();
                foreach (Token[] match in expectedMatches)
                {
                    if (match.Length != lineTokens.Length - opcodeIndex - 1)
                    {
                        continue;
                    }
                    values.Clear();
                    actualTokens.Clear();
                    success = true;
                    int positionInLine = opcodeIndex + 1;
                    for (int i = 0; i < match.Length;)
                    {
                        if (match[i] != lineTokens[positionInLine].Token && !CanFirstTokenBeParsedAsOther(lineTokens[positionInLine].Token, match[i]))
                        {
                            success = false;
                            break;
                        }
                        values.Add(lineTokens[positionInLine].InternalString);
                        actualTokens.Add(match[i]);
                        i++;
                        positionInLine++;
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

                int properIndex = opcodeIndex + 1;
                for (int i = 0; i < actualTokens.Count;)
                {
                    lineTokens[properIndex].Token = actualTokens[i];
                    i++;
                    properIndex++;
                }

                program.Add(new ProgramLine(opcode, values.ToArray(), lineTokens, precedingWhitespace[lineIndex], programComments[lineIndex]));
            }

            Program = program;
        }

        public void IncrementProgramCounter()
        {
            SetProgramCounter((byte)(ProgramCounter + 1));
        }
        public void SetProgramCounter(byte value)
        {
            ProgramCounter = value;
            if (ProgramCounter > 127)
            {
                ProgramCounter -= 128;
            }

            while (ProgramCounter >= Program.Count)
            {
                ProgramLine randomProgramLine;
                if (GenerateRandomProgramLinesWhenOutOfBounds)
                {
                    randomProgramLine = new ProgramLine(new Random());
                }
                else
                {
                    randomProgramLine = new ProgramLine("0000 0000 0000 0000");
                }
                randomProgramLine.Comment = "// ! AUTOGENERATED !";
                Program.Add(randomProgramLine);
            }

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
            if (CallStack.Count == 0)
            {
                SetProgramCounter(1);
                return;
            }
            SetProgramCounter((byte)(CallStack.Pop() + 1));
        }

        public byte GetMemory(byte index)
        {
            if (OUTPUT_ONLY_MEMORY[index]) return 0;
            return Memory[index];
        }
        public void SetMemory(byte index, byte value)
        {
            if (INPUT_ONLY_MEMORY[index]) return;
            Memory[index] = value;
            if (index == MEMORY_OUT_NIBBLE_SWAPPER)
            {
                Memory[MEMORY_IN_NIBBLE_SWAPPER] = (byte)((byte)(value << 4) + (byte)(value >> 4));
            }
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
