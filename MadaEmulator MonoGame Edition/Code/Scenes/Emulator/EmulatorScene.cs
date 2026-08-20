using Blunatic.Core;
using Blunatic.Mathematics;
using Blunatic.Mgc;
using Blunatic.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public class EmulatorScene : IScene
    {
        // Constants
        private const int PROGRAM_DRAW_VERTICAL_OFFSET = 6;
        private const int EMULATOR_VISUAL_SPLIT_LOCATION = 60;

        // Interfaces
        private interface IInfoboxReporter
        {
            public string GetTitle(MonoGameInstance mgi, Emulator emulator);
            public string GetMessage(MonoGameInstance mgi, Emulator emulator);
        }

        // Structs
        private struct TaskbarButtonEntry
        {
            public MonoGameConsoleButton Button { get; set; }
            public Func<MonoGameInstance, IEnumerable<string>> Options;
            public Action<object, DropdownScene.DropdownResultEventArgs> Callback;

            public TaskbarButtonEntry(MonoGameConsoleButton button, Func<MonoGameInstance, IEnumerable<string>> options, Action<object, DropdownScene.DropdownResultEventArgs> callback)
            {
                Button = button;
                Options = options;
                Callback = callback;
            }

            private void _insertCallback(object sender, DropdownScene.DropdownResultEventArgs args) => Callback(sender, args);

            public bool IsPressed() => Button.IsLeftClicked;
            public void Activate(MonoGameInstance mgi, MonoGameConsole _mgc)
            {
                DropdownScene dropdownScene = new DropdownScene(mgi, _mgc.Dimensions, Button.Position + Vec.East, Options(mgi).ToArray());
                dropdownScene.ResultSelected += _insertCallback;
                mgi.SceneIn(dropdownScene);
                return;
            }
        }

        // Enums
        private enum LowerLeftDisplayType
        {
            Memory,
            Peripherals,
        }
        private enum LoadedProgramType
        {
            None,
            FromFile,
            RandomlyGenerated,
        }
        private enum InfoboxStatus
        {
            Hidden,
            Fleeting,
            Persistant,
        }

        // Classes
        public class MemoryInfoboxReporter : IInfoboxReporter
        {
            private byte _index;

            public MemoryInfoboxReporter(byte index)
            {
                _index = index;
            }

            public string GetTitle(MonoGameInstance mgi, Emulator emulator)
            {
                return $"Info for Memory Address {_index}";
            }

            public string GetMessage(MonoGameInstance mgi, Emulator emulator)
            {
                return $"0b{Convert.ToString(emulator.Memory[_index], 2).PadLeft(8, '0')} ({emulator.Memory[_index]})\n{Emulator.MEMORY_RESERVATION_IDENTIFIERS[_index]}\n{(Emulator.MEMORY_RESERVATION_DESCRIPTIONS[_index] ?? "")}"; ;
            }
        }

        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => false;

        // Fields
        private MonoGameConsole _mgc;

        private LoadedProgramType _loadedProgramType;

        private TaskbarButtonEntry[] _topLevelTaskbarButtonEntries;

        private MonoGameConsoleRegion _canvasRegion;

        private MonoGameConsoleRegion _programRegion;
        private int _programOffset;
        private int _hoveredLine;

        private MonoGameConsoleRegion _controllerRedRegion;
        private MonoGameConsoleRegion _controllerGreenRegion;
        private MonoGameConsoleRegion _controllerBlueRegion;
        private MonoGameConsoleRegion _controllerShockRegion;
        private MonoGameConsoleButton _controllerEnterButton;
        private MonoGameConsoleRegion _controllerByteRegion;
        private Vec _controllerScreenOrigin;

        private MonoGameConsoleButton _infoboxButton;

        private bool[] _breakpoints;
        private bool _hoveringBreakpoint;

        private bool _showMachineCode;
        private bool _randomiseAllMemoryOnReset;
        private bool _generateRandomProgramLinesWhenOutOfBounds;

        private string _programPath;
        private string _programDirectory;
        private string _programName;

        private float _instructionsPerSecond;
        private float _instructionRuntimeDebt;
        private bool _autoRun;

        private InfoboxStatus _infoboxStatus;
        private IInfoboxReporter _infoboxReporter;

        private Emulator _emulator;

        private LowerLeftDisplayType _lowerLeftDisplay;

        // Constructors
        public EmulatorScene(MonoGameInstance mgi)
        {
            _mgc = new MonoGameConsole(mgi, new Vec(120, 45));

            _loadedProgramType = LoadedProgramType.None;

            _randomiseAllMemoryOnReset = false;
            _generateRandomProgramLinesWhenOutOfBounds = false;

            _hoveredLine = -1;
            _hoveringBreakpoint = false;

            ResetEmulatorEnvironment();

            _topLevelTaskbarButtonEntries = new TaskbarButtonEntry[]
            {
                new TaskbarButtonEntry(new MonoGameConsoleButton(mgi, new Vec(1, 1), "File"), GetFileButtonDropdownOptions, RespondToFileButtonDropdown),
                new TaskbarButtonEntry(new MonoGameConsoleButton(mgi, new Vec(10, 1), "Config"), GetConfigButtonDropdownOptions, RespondToConfigButtonDropdown),
                new TaskbarButtonEntry(new MonoGameConsoleButton(mgi, new Vec(21, 1), "Help"), GetHelpButtonDropdownOptions, RespondToHelpButtonDropdown),
            };

            Iterate.Each(_topLevelTaskbarButtonEntries, (x) => { _mgc.AddElement(x.Button); });

            _canvasRegion = new MonoGameConsoleRegion(new Rectangle(1, 3, _mgc.Dimensions.X - 2, _mgc.Dimensions.Y - 4));
            _mgc.AddElement(_canvasRegion);

            _programRegion = new MonoGameConsoleRegion(new Rectangle(_canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION + 1, PROGRAM_DRAW_VERTICAL_OFFSET), new Vec(_canvasRegion.Dimensions.X - (EMULATOR_VISUAL_SPLIT_LOCATION + 1), _canvasRegion.Dimensions.Y - PROGRAM_DRAW_VERTICAL_OFFSET)));
            _mgc.AddElement(_programRegion);

            _programPath = null;
            _programDirectory = null;
            _programName = null;
            _emulator = null;

            _instructionsPerSecond = 6;

            _lowerLeftDisplay = LowerLeftDisplayType.Memory;

            _controllerScreenOrigin = new Vec(26, 25) + _canvasRegion.Position;
            _controllerRedRegion = new MonoGameConsoleRegion(new Rectangle(_controllerScreenOrigin + new Vec(6, 7), new Vec(3, 2)));
            _controllerGreenRegion = new MonoGameConsoleRegion(new Rectangle(_controllerScreenOrigin + new Vec(11, 7), new Vec(3, 2)));
            _controllerBlueRegion = new MonoGameConsoleRegion(new Rectangle(_controllerScreenOrigin + new Vec(16, 7), new Vec(3, 2)));
            _controllerEnterButton = new MonoGameConsoleButton(mgi, _controllerScreenOrigin + new Vec(9, 12), "[Enter]", false);
            _controllerShockRegion = new MonoGameConsoleRegion(new Rectangle(_controllerScreenOrigin + new Vec(11, 3), new Vec(3, 4)));
            _controllerByteRegion = new MonoGameConsoleRegion(new Rectangle(_controllerScreenOrigin + new Vec(5, 10), new Vec(15, 1)));

            _showMachineCode = false;

            _infoboxReporter = null;
            _infoboxStatus = InfoboxStatus.Hidden;
            _infoboxButton = new MonoGameConsoleButton(mgi, new Vec(_canvasRegion.Position.X + 58, _canvasRegion.Position.Y + 16), "X", false);
            _infoboxButton.ActiveBackgroundColor = Color.Cyan;
            _infoboxButton.IsHidden = true;
            _infoboxButton.Inverted = true;
            _mgc.AddElement(_infoboxButton);
        }

        // Methods
        public IEnumerable<string> GetFileButtonDropdownOptions(MonoGameInstance mgi)
        {
            yield return "Load";

            if (_loadedProgramType == LoadedProgramType.FromFile)
            {
                yield return "Reload";
            }

            yield return "Generate Random Program";

            yield break;
        }
        public IEnumerable<string> GetConfigButtonDropdownOptions(MonoGameInstance mgi)
        {
            if (_loadedProgramType != LoadedProgramType.None)
            {
                yield return "Change Clock Speed";
                yield return "Cycle Display";
                yield return "Toggle Machine Code";
            }

            if (_randomiseAllMemoryOnReset)
            {
                yield return "Disable Memory Randomisation";
            }
            else
            {
                yield return "Enable Memory Randomisation";
            }

            if (_generateRandomProgramLinesWhenOutOfBounds)
            {
                yield return "Disable Out-Of-Bounds Program Generation";
            }
            else
            {
                yield return "Enable Out-Of-Bounds Program Generation";
            }

            yield break;
        }
        public IEnumerable<string> GetHelpButtonDropdownOptions(MonoGameInstance mgi)
        {
            yield return "View Cheat Sheet";

            yield break;
        }

        public void RespondToFileButtonDropdown(object sender, DropdownScene.DropdownResultEventArgs args)
        {
            switch (args.SelectedValue)
            {
                case "Load":
                    {
                        DoLoadMenu(args.MonoGameInstance);
                    }
                    break;
                case "Reload":
                    {
                        DoReloadMenu(args.MonoGameInstance);
                    }
                    break;
                case "Generate Random Program":
                    {
                        DoGenerateRandomProgramMenu(args.MonoGameInstance);
                    }
                    break;
            }
        }
        public void RespondToConfigButtonDropdown(object sender, DropdownScene.DropdownResultEventArgs args)
        {
            switch (args.SelectedValue)
            {
                case "Change Clock Speed":
                    {
                        ValueChangeScene valueChangeScene = new ValueChangeScene(args.MonoGameInstance, "Hertz", 7, _instructionsPerSecond.ToString().Substring(0, Math.Min(7, _instructionsPerSecond.ToString().Length)), "0123456789.");
                        valueChangeScene.ResultSelected += RespondToClockSpeedChangeMenu;
                        args.MonoGameInstance.SceneIn(valueChangeScene);
                    }
                    break;
                case "Cycle Display":
                    CycleDisplay();
                    break;
                case "Toggle Machine Code":
                    _showMachineCode = !_showMachineCode;
                    break;
                case "Disable Memory Randomisation":
                case "Enable Memory Randomisation":
                    _randomiseAllMemoryOnReset = !_randomiseAllMemoryOnReset;
                    SyncEmulatorConfig();
                    break;
                case "Disable Out-Of-Bounds Program Generation":
                case "Enable Out-Of-Bounds Program Generation":
                    _generateRandomProgramLinesWhenOutOfBounds = !_generateRandomProgramLinesWhenOutOfBounds;
                    SyncEmulatorConfig();
                    break;
            }
        }
        public void RespondToHelpButtonDropdown(object sender, DropdownScene.DropdownResultEventArgs args)
        {
            switch (args.SelectedValue)
            {
                case "View Cheat Sheet":
                    args.MonoGameInstance.SceneIn(new CheatSheetScene(args.MonoGameInstance));
                    break;
            }
        }

        public void DoGenerateRandomProgramMenu(MonoGameInstance mgi)
        {
            Random rng = new Random();
            int seed = rng.Next();
            try
            {
                _programPath = null;
                _programDirectory = null;
                _programName = $"RNG{seed}";
                _emulator = new Emulator(new Random(seed));
                SyncEmulatorConfig();
                _loadedProgramType = LoadedProgramType.RandomlyGenerated;

                ResetEmulatorEnvironment();
            }
            catch (Exception e)
            {
                mgi.SceneIn(new PopupScene(mgi, PopupScene.Type.Error, e.Message));
            }
        }
        public void DoLoadMenu(MonoGameInstance mgi)
        {
            FileExplorerScene fileExplorerScene;
            try
            {
                fileExplorerScene = new FileExplorerScene(mgi, FileExplorerScene.Mode.Open, _programDirectory);
            }
            catch
            {
                fileExplorerScene = new FileExplorerScene(mgi, FileExplorerScene.Mode.Open, "Resources\\Programs");
            }
            fileExplorerScene.FileSelected += RespondToFileExplorer;
            mgi.SceneIn(fileExplorerScene);
        }
        public void DoReloadMenu(MonoGameInstance mgi)
        {
            if (_loadedProgramType == LoadedProgramType.None) return;
            if (_loadedProgramType == LoadedProgramType.RandomlyGenerated)
            {
                DoGenerateRandomProgramMenu(mgi);
                return;
            }

            try
            {
                LoadProgramFromPath(_programPath);
            }
            catch (Exception e)
            {
                mgi.SceneIn(new PopupScene(mgi, PopupScene.Type.Error, e.Message));
            }
        }
        public void LoadProgramFromPath(string path)
        {
            _programPath = path;
            _programDirectory = _programPath.Substring(0, _programPath.Length - _programPath.Split('\\').Last().Length - 1);
            _programName = _programPath.Substring(_programDirectory.Length + 1);
            _emulator = new Emulator(_programPath);
            SyncEmulatorConfig();
            _loadedProgramType = LoadedProgramType.FromFile;

            ResetEmulatorEnvironment();
        }
        public void ResetEmulator()
        {
            _emulator?.Reset();
            _autoRun = false;
            _programOffset = _emulator == null ? 0 : _emulator.ProgramCounter;
            _instructionRuntimeDebt = 0;
        }
        public void ResetEmulatorEnvironment()
        {
            ResetEmulator();
            _breakpoints = new bool[128];
        }
        public void CycleDisplay()
        {
            if (_lowerLeftDisplay == LowerLeftDisplayType.Memory)
            {
                _lowerLeftDisplay = Enum.GetValues<LowerLeftDisplayType>().Last();
            }
            else
            {
                _lowerLeftDisplay--;
            }
        }

        public void SyncEmulatorConfig()
        {
            if (_emulator == null) return;
            _emulator.RandomiseStoredMemoryOnReset = _randomiseAllMemoryOnReset;
            _emulator.GenerateRandomProgramLinesWhenOutOfBounds = _generateRandomProgramLinesWhenOutOfBounds;
        }

        public void RespondToFileExplorer(object sender, FileExplorerScene.FileExplorerResultEventArgs args)
        {
            if (!args.Path.EndsWith(".txt"))
            {
                args.Reject("Expected a .txt file.");
                return;
            }
            try
            {
                LoadProgramFromPath(args.Path);
            }
            catch (Exception e)
            {
                args.Reject(e.Message);
                return;
            }
        }
        public void RespondToClockSpeedChangeMenu(object sender, ValueChangeScene.ValueChangeResultEventArgs args)
        {
            try
            {
                float value = float.Parse(args.Input);
                if (value > 1000)
                {
                    args.Reject("Inputted value must be less than 1000.");
                    return;
                }
                _instructionsPerSecond = value;
            }
            catch (FormatException)
            {
                args.Reject("Inputted value must be a valid float.");
                return;
            }
            catch (Exception e)
            {
                args.Reject(e.Message);
                return;
            }
        }

        public void Update(MonoGameInstance mgi)
        {
            _mgc.Update(mgi);
            Vec hoveredCell = _mgc.GetCursorHoveredCellPos(mgi);

            if (_infoboxStatus != InfoboxStatus.Persistant || _infoboxButton.IsLeftClicked)
            {
                _infoboxStatus = InfoboxStatus.Hidden;
                _infoboxReporter = null;
            }

            if (mgi.ControlWasJustPressed("escape"))
            {
                mgi.SceneOut();
                return;
            }

            if (mgi.ControlWasJustPressed("load program"))
            {
                DoLoadMenu(mgi);
                return;
            }

            if (_emulator != null)
            {
                if (mgi.ControlWasJustPressed("reload program"))
                {
                    DoReloadMenu(mgi);
                    return;
                }

                if (mgi.ControlWasJustPressed("restart program"))
                {
                    ResetEmulator();
                }

                if (mgi.ControlWasJustPressed("toggle machine code"))
                {
                    _showMachineCode = !_showMachineCode;
                }
                if (mgi.ControlWasJustPressed("cycle lower left display"))
                {
                    CycleDisplay();
                }
                if (mgi.ControlWasJustPressed("run/pause program") && !_emulator.IsHalted)
                {
                    _autoRun = !_autoRun;
                }

                if (_lowerLeftDisplay == LowerLeftDisplayType.Peripherals)
                {
                    _controllerRedRegion.Update(mgi, _mgc);
                    _controllerGreenRegion.Update(mgi, _mgc);
                    _controllerBlueRegion.Update(mgi, _mgc);
                    _controllerEnterButton.Update(mgi, _mgc);
                    _controllerShockRegion.Update(mgi, _mgc);
                    _controllerByteRegion.Update(mgi, _mgc);

                    if (_controllerRedRegion.IsLeftClicked)
                    {
                        _emulator.Controller.RedActive = true;
                    }
                    if (_controllerGreenRegion.IsLeftClicked)
                    {
                        _emulator.Controller.GreenActive = true;
                    }
                    if (_controllerBlueRegion.IsLeftClicked)
                    {
                        _emulator.Controller.BlueActive = true;
                    }
                    if (_controllerEnterButton.IsLeftClicked)
                    {
                        _emulator.Controller.EnterActive = true;
                    }

                    if (_controllerShockRegion.IsLeftClicked)
                    {
                        _emulator.Controller.ShockActive = !_emulator.Controller.ShockActive;
                    }

                    if (_controllerByteRegion.IsLeftClicked)
                    {
                        Vec relativePos = _controllerByteRegion.GetRelativePositionOfScreenPosition(hoveredCell);
                        if (relativePos.X % 2 == 0)
                        {
                            _emulator.Controller.ByteInput ^= (byte)(0b10000000 >> relativePos.X / 2);
                        }
                    }
                }
                else if (_lowerLeftDisplay == LowerLeftDisplayType.Memory)
                {
                    if (_infoboxStatus != InfoboxStatus.Persistant)
                    {
                        Vec relativePos = hoveredCell - (_canvasRegion.Position + new Vec(11, 25));
                        if (relativePos.IsInBounds(new Rectangle(0, 0, 48, 16)) && relativePos.X % 3 != 0)
                        {
                            _infoboxStatus = InfoboxStatus.Fleeting;
                            int hoveredIndex = 16 * relativePos.Y + relativePos.X / 3;
                            _infoboxReporter = new MemoryInfoboxReporter((byte)hoveredIndex);
                            if (mgi.CursorState.WasJustPressed(MouseButton.Right))
                            {
                                _infoboxStatus = InfoboxStatus.Persistant;
                            }
                        }
                    }
                }

                if (_autoRun)
                {
                    if (_emulator.IsHalted)
                    {
                        _autoRun = false;
                        _instructionRuntimeDebt = 0;
                    }
                    else
                    {
                        _instructionRuntimeDebt += _instructionsPerSecond * (float)mgi.FrameTime.TotalSeconds;

                        while (_instructionRuntimeDebt >= 1)
                        {
                            _emulator.Step();

                            _instructionRuntimeDebt -= 1;

                            if (_emulator.IsHalted || _breakpoints[_emulator.ProgramCounter])
                            {
                                _autoRun = false;
                                _instructionRuntimeDebt = 0;
                                break;
                            }
                        }
                    }

                    if (_emulator.ProgramCounter < _programOffset)
                    {
                        _programOffset = _emulator.ProgramCounter;
                    }
                    else if (_emulator.ProgramCounter >= _programOffset + _programRegion.Dimensions.Y)
                    {
                        _programOffset = _emulator.ProgramCounter - _programRegion.Dimensions.Y + 1;
                    }
                }
                else
                {
                    if (mgi.ControlWasJustPressed("progress program") || (mgi.ControlIsPressed("progress program fast")) && !_breakpoints[_emulator.ProgramCounter])
                    {
                        _emulator.Step();

                        if (_emulator.ProgramCounter < _programOffset)
                        {
                            _programOffset = _emulator.ProgramCounter;
                        }
                        else if (_emulator.ProgramCounter >= _programOffset + _programRegion.Dimensions.Y)
                        {
                            _programOffset = _emulator.ProgramCounter - _programRegion.Dimensions.Y + 1;
                        }
                    }
                    if (mgi.ControlWasJustPressed("regress program") || mgi.ControlIsPressed("regress program fast") && !_breakpoints[_emulator.ProgramCounter])
                    {
                        _emulator.Rewind();

                        if (_emulator.ProgramCounter < _programOffset)
                        {
                            _programOffset = _emulator.ProgramCounter;
                        }
                        else if (_emulator.ProgramCounter >= _programOffset + _programRegion.Dimensions.Y)
                        {
                            _programOffset = _emulator.ProgramCounter - _programRegion.Dimensions.Y + 1;
                        }
                    }
                }

                _hoveredLine = -1;
                _hoveringBreakpoint = false;
                if (_programRegion.IsHovered)
                {
                    int scrollThisTick = mgi.CursorState.GetCursorScrollThisTick();
                    if (scrollThisTick != 0 && _emulator.Program.Count > _programRegion.Dimensions.Y)
                    {
                        _programOffset -= Math.Sign(scrollThisTick);
                        if (_programOffset < 0)
                        {
                            _programOffset = 0;
                        }
                        else if (_programOffset > _emulator.Program.Count - _programRegion.Dimensions.Y)
                        {
                            _programOffset = _emulator.Program.Count - _programRegion.Dimensions.Y;
                        }
                    }
                    Vec relativeCursorPosition = _programRegion.GetRelativePositionOfScreenPosition(hoveredCell);
                    int hoveredIndex = relativeCursorPosition.Y + _programOffset;
                    if (hoveredIndex < _emulator.Program.Count && hoveredIndex >= 0)
                    {
                        _hoveredLine = hoveredIndex;
                        if (relativeCursorPosition.X == 0)
                        {
                            _hoveringBreakpoint = true;
                            if (mgi.CursorState.WasJustPressed(MouseButton.Left))
                            {
                                _breakpoints[_hoveredLine] = !_breakpoints[_hoveredLine];
                            }
                        }
                    }
                }
            }

            foreach (TaskbarButtonEntry entry in _topLevelTaskbarButtonEntries)
            {
                if (entry.IsPressed())
                {
                    entry.Activate(mgi, _mgc);
                    break;
                }
            }

            _infoboxButton.IsHidden = _infoboxStatus != InfoboxStatus.Persistant;
        }

        private void _drawOuterBorder()
        {
            _mgc.Fill(new Rectangle(1, 0, _mgc.Dimensions.X - 2, 1), Ch.Border.n0.e1.s0.w1, new Color(80, 80, 80));
            _mgc.SetCell(new Vec(0, 0), Ch.Border.n0.e1.s1.w0, new Color(80, 80, 80));
            _mgc.SetCell(new Vec(_mgc.Dimensions.X - 1, 0), Ch.Border.n0.e0.s1.w1, new Color(80, 80, 80));
            _mgc.Fill(new Rectangle(0, 1, _mgc.Dimensions.X, 1), Ch.Border.n0.e2.s0.w2, Color.DarkGray);
            _mgc.Fill(new Rectangle(1, 2, _mgc.Dimensions.X - 2, 1), Ch.Border.n0.e1.s0.w1, new Color(80, 80, 80));
            _mgc.SetCell(new Vec(0, 2), Ch.Border.n1.e1.s1.w0, new Color(80, 80, 80));
            _mgc.SetCell(new Vec(_mgc.Dimensions.X - 1, 2), Ch.Border.n1.e0.s1.w1, new Color(80, 80, 80));
            _mgc.Fill(new Rectangle(1, _mgc.Dimensions.Y - 1, _mgc.Dimensions.X - 2, 1), Ch.Border.n0.e1.s0.w1, new Color(80, 80, 80));
            _mgc.SetCell(new Vec(0, _mgc.Dimensions.Y - 1), Ch.Border.n1.e1.s0.w0, new Color(80, 80, 80));
            _mgc.SetCell(new Vec(_mgc.Dimensions.X - 1, _mgc.Dimensions.Y - 1), Ch.Border.n1.e0.s0.w1, new Color(80, 80, 80));
            _mgc.Fill(new Rectangle(0, 3, 1, _mgc.Dimensions.Y - 4), Ch.Border.n1.e0.s1.w0, new Color(80, 80, 80));
            _mgc.Fill(new Rectangle(_mgc.Dimensions.X - 1, 3, 1, _mgc.Dimensions.Y - 4), Ch.Border.n1.e0.s1.w0, new Color(80, 80, 80));
        }
        private void _drawEmulator(MonoGameInstance mgi)
        {
            int i;
            _mgc.Fill(new Rectangle(_canvasRegion.Position.X, _canvasRegion.Position.Y, _canvasRegion.Dimensions.X, 1), Ch.Border.n0.e2.s0.w2, new Color(120, 120, 120));
            _mgc.Fill(new Rectangle(_canvasRegion.Position.X, _canvasRegion.Position.Y + 1, _canvasRegion.Dimensions.X, 1), Ch.Border.n0.e1.s0.w1, new Color(80, 80, 80));
            _mgc.SetCell(_canvasRegion.Position + new Vec(-1, 1), Ch.Border.n1.e1.s1.w0, new Color(80, 80, 80));
            _mgc.SetCell(_canvasRegion.Position + new Vec(_canvasRegion.Dimensions.X, 1), Ch.Border.n1.e0.s1.w1, new Color(80, 80, 80));
            _mgc.SetCell(_canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION, 1), Ch.Border.n0.e1.s1.w1, new Color(80, 80, 80));
            _mgc.SetCell(_canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION, _canvasRegion.Dimensions.Y), Ch.Border.n1.e1.s0.w1, new Color(80, 80, 80));
            _mgc.Fill(new Rectangle(_canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION, 2), new Vec(1, _canvasRegion.Dimensions.Y - 2)), Ch.Border.n1.e0.s1.w0, new Color(80, 80, 80));
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(0, 2), $"Flags:");
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(1, 3), $"{Fm.Fg(Color.DarkGray)}C: {(_emulator.Flags[Emulator.Condition.C] ? $"{Fm.Fg(Color.Green)}True " : $"{Fm.Fg(Color.Red)}False")} {Fm.Fg(Color.DarkGray)}NC: {(_emulator.Flags[Emulator.Condition.NC] ? $"{Fm.Fg(Color.Green)}True " : $"{Fm.Fg(Color.Red)}False")}");
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(1, 4), $"{Fm.Fg(Color.DarkGray)}Z: {(_emulator.Flags[Emulator.Condition.Z] ? $"{Fm.Fg(Color.Green)}True " : $"{Fm.Fg(Color.Red)}False")} {Fm.Fg(Color.DarkGray)}NZ: {(_emulator.Flags[Emulator.Condition.NZ] ? $"{Fm.Fg(Color.Green)}True " : $"{Fm.Fg(Color.Red)}False")}");
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(23, 2), $"Program Counter: {Fm.Fg(Color.DarkGray)}{_emulator.ProgramCounter}");
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(23, 4), $"Instruction Counter: {Fm.Fg(Color.DarkGray)}{_emulator.InstructionCounter}");
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(23, 6), $"Call Stack:");
            i = 0;
            foreach (byte b in _emulator.CallStack)
            {
                _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(23, 7 + i), $" {Fm.Fg(Color.DarkGray)}0b{Convert.ToString(b, 2).PadLeft(8, '0')} ({b})");
                i++;
            }
            for (; i < 8; i++)
            {
                _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(23, 7 + i), $" {Fm.Fg(Color.DarkGray)}-");
            }
            if (_infoboxStatus != InfoboxStatus.Hidden)
            {
                _mgc.Box(mgi, new Rectangle(_canvasRegion.Position.X + 23, _canvasRegion.Position.Y + 16, 36, 8), _infoboxReporter.GetTitle(mgi, _emulator), _infoboxStatus == InfoboxStatus.Persistant ? Color.White : new Color(71, 71, 71));
                _mgc.WriteString(mgi, new Vec(_canvasRegion.Position.X + 24, _canvasRegion.Position.Y + 17), $"{Fm.Fg(_infoboxStatus == InfoboxStatus.Persistant ? Color.LightGray : new Color(110, 110, 110))}{_infoboxReporter.GetMessage(mgi, _emulator)}", new Vec(34, 6), MonoGameConsole.WrapType.WordWrap);
            }
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(0, 6), $"Registers:");
            for (i = 0; i < _emulator.Registers.Length; i++)
            {
                Vec drawPos = _canvasRegion.Position + new Vec(0, i + 7);
                _mgc.WriteString(mgi, drawPos, $"{Fm.Fg(Color.DarkGray)}{$"r{i}".PadLeft(4, ' ')}: 0b{Convert.ToString(_emulator.Registers[i], 2).PadLeft(8, '0')} ({_emulator.Registers[i]})");
            }
            switch (_lowerLeftDisplay)
            {
                case LowerLeftDisplayType.Memory:
                    {
                        _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(0, 16 + 8), $"Memory:");
                        for (i = 0; i < 16; i++)
                        {
                            Vec drawPos = _canvasRegion.Position + new Vec(0, 16 + 9 + i);
                            _mgc.WriteString(mgi, drawPos, $" {Fm.Fg(Color.DarkGray)}{$"{i * 16}".PadLeft(3, ' ')} - {$"{i * 16 + 15}".PadLeft(3, ' ')}:{Iterate.InBounds(i * 16, i * 16 + 16).Aggregate(string.Empty, (s, v) =>
                            {
                                string addition = _emulator.Memory[v].ToString("X").PadLeft(2, '0');
                                addition = $"{(addition == "00" ? Fm.Fg(Color.DarkGray) : Fm.Fg(Color.White))}{(Emulator.INPUT_ONLY_MEMORY[v] ? Fm.Bg(new Color(0, 100, 0)) : Emulator.OUTPUT_ONLY_MEMORY[v] ? Fm.Bg(new Color(100, 0, 0)) : Fm.Bg(Color.Black))}{addition}{Fm.Bg(Color.Black)}";
                                return $"{s} {addition}";
                            })}");
                        }
                    }
                    break;
                case LowerLeftDisplayType.Peripherals:
                    {
                        _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(0, 16 + 8), $"Peripherals:");
                        _mgc.Box(new Rectangle(_canvasRegion.Position.X, _canvasRegion.Position.Y + 16 + 9, 26, 16), new Color(67, 67, 67), null, true);
                        int x = 0;
                        foreach (byte b in _emulator.Screen.Bytes)
                        {
                            int y = 0;
                            for (byte a = 0b10000000; a != 0 ; a = (byte)(a >> 1))
                            {
                                Color color;
                                if ((a & b) != 0)
                                {
                                    color = new Color(196, 174, 144);
                                }
                                else
                                {
                                    color = new Color(107, 62, 33);
                                }
                                _mgc.Fill(new Rectangle(_canvasRegion.Position.X + 3 * x + 1, _canvasRegion.Position.Y + 16 + 8 + y++ * 2 + 1, 3, 2), Ch.BlockFull, color);
                            }
                            x++;
                        }

                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(4, 6), new Vec(17, 7)), Ch.BlockFull, new Color(51, 51, 51));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(5, 5), new Vec(3, 1)), Ch.BlockFull, new Color(51, 51, 51));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(8, 12), new Vec(9, 1)), Ch.Space, Color.White);
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(4, 14), new Vec(2, 1)), Ch.BlockFull, new Color(51, 51, 51));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(19, 14), new Vec(2, 1)), Ch.BlockFull, new Color(51, 51, 51));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(17, 5), new Vec(3, 1)), Ch.BlockFull, new Color(51, 51, 51));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(3, 8), new Vec(4, 6)), Ch.BlockFull, new Color(51, 51, 51));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(18, 8), new Vec(4, 6)), Ch.BlockFull, new Color(51, 51, 51));

                        if (_emulator.Controller.ShockActive)
                        {
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 4), Ch.BlockFull, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 5), _mgc.GetRandomGlyph(), new Color(0, 255, 255));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 3), Ch.Border.n1.e0.s1.w0, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 2), Ch.Border.n0.e0.s1.w1, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(11, 2), Ch.Border.n0.e1.s0.w1, new Color(204, 204, 204));
                        }
                        else
                        {
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 5), Ch.BlockFull, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 4), Ch.Border.n1.e0.s1.w0, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(12, 3), Ch.Border.n0.e0.s1.w1, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(11, 3), Ch.Border.n1.e1.s0.w0, new Color(204, 204, 204));
                            _mgc.SetCell(_controllerScreenOrigin + new Vec(11, 2), Ch.Border.n0.e0.s1.w1, new Color(204, 204, 204));
                        }

                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(1, 3), new Vec(5, 1)), Ch.Border.n0.e1.s0.w1, new Color(204, 204, 204));
                        _mgc.SetCell(_controllerScreenOrigin + new Vec(6, 2), Ch.Border.n0.e1.s1.w0, new Color(204, 204, 204));
                        _mgc.SetCell(_controllerScreenOrigin + new Vec(6, 3), Ch.Border.n1.e0.s0.w1, new Color(204, 204, 204));
                        _mgc.Fill(new Rectangle(_controllerScreenOrigin + new Vec(7, 2), new Vec(4, 1)), Ch.Border.n0.e1.s0.w1, new Color(204, 204, 204));
                        _mgc.SetCell(_controllerScreenOrigin + new Vec(0, 3), Ch.BlockFull, new Color(204, 204, 204));

                        _mgc.PrintString(_controllerByteRegion.Position, $"{Convert.ToString(_emulator.Controller.ByteInput, 2).PadLeft(8, '0').Aggregate(string.Empty, (i, x) => $"{i} {x}").Substring(1)}", Color.White, new Color(34, 34, 34));

                        _controllerEnterButton.Draw(mgi, _mgc);
                        if (_emulator.Controller.EnterActive)
                        {
                            _mgc.PrintString(_controllerEnterButton.Position, $"[  {Fm.GetCurrentCharacterInAnimatedCell(mgi, Fm.AnimatedCell.CycleClockwise)}  ]");
                        }

                        if (!_emulator.Controller.RedActive)
                        {
                            _mgc.Box(_controllerRedRegion.Area, Color.Black, new Color(127, 0, 0));
                        }
                        else
                        {
                            _mgc.Fill(_controllerRedRegion.Area, Ch.BlockFull, new Color(255, 0, 0));
                        }

                        if (!_emulator.Controller.GreenActive)
                        {
                            _mgc.Box(_controllerGreenRegion.Area, Color.Black, new Color(0, 64, 0));
                        }
                        else
                        {
                            _mgc.Fill(_controllerGreenRegion.Area, Ch.BlockFull, new Color(0, 128, 0));
                        }

                        if (!_emulator.Controller.BlueActive)
                        {
                            _mgc.Box(_controllerBlueRegion.Area, Color.Black, new Color(35, 65, 127));
                        }
                        else
                        {
                            _mgc.Fill(_controllerBlueRegion.Area, Ch.BlockFull, new Color(71, 129, 255));
                        }
                    }
                    break;
            }
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION + 1, 2), $"Loaded: {Fm.Fg(Color.DarkGray)}{_programName}", _programRegion.Dimensions.X, MonoGameConsole.WrapType.Cut);
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION + 1, 4), $"Program: {(_autoRun ? $"{Fm.Fg(Color.Yellow)}RUNNING at {_instructionsPerSecond}Hz {Fm.Fg(new Color(80, 80, 80))}({(_instructionRuntimeDebt*100):0.00}%)" : (_emulator.IsHalted ? $"{Fm.Fg(Color.Green)}Halted" : string.Empty))}");
            for (i = _programOffset; i < _emulator.Program.Count && i + PROGRAM_DRAW_VERTICAL_OFFSET - _programOffset < _canvasRegion.Dimensions.Y; i++)
            {
                Color backgroundColorForLine;

                if (_emulator.ProgramCounter == i)
                {
                    if (_emulator.IsHalted)
                    {
                        backgroundColorForLine = new Color(0, 41, 0);
                    }
                    else if (_breakpoints[i])
                    {
                        backgroundColorForLine = new Color(62, 0, 0);
                    }
                    else
                    {
                        backgroundColorForLine = new Color(41, 41, 0);
                    }
                }
                else if (_hoveredLine == i)
                {
                    backgroundColorForLine = new Color(38, 38, 38);
                }
                else
                {
                    backgroundColorForLine = Color.Black;
                }

                _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(EMULATOR_VISUAL_SPLIT_LOCATION + 1, i + PROGRAM_DRAW_VERTICAL_OFFSET - _programOffset), $"{(_hoveringBreakpoint && _hoveredLine == i ? $"{(_breakpoints[i] ? $"{Fm.Fg(Color.Red)}{Ch.AsChar(Ch.Circle)}" : $"{Fm.Fg(Color.DarkRed)}{Ch.AsChar(Ch.Circle)}")}" : $"{(_breakpoints[i] ? $"{Fm.Fg(Color.Red)}{Ch.AsChar(Ch.Circle)}" : $" ")}")}{Fm.Fg(new Color(60, 60, 60))}{$"{i}".PadLeft(3, ' ')} {Fm.Col(Color.White, backgroundColorForLine)}{(_showMachineCode ? _emulator.Program[i].MachineCode : _emulator.Program[i].ToFormattedString(mgi))}{new string(' ', _programRegion.Dimensions.X)}", _programRegion.Dimensions.X - 4, MonoGameConsole.WrapType.Cut);
            }
        }
        public void Draw(MonoGameInstance mgi)
        {
            _drawOuterBorder();

            if (_emulator != null)
            {
                _drawEmulator(mgi);
            }

            _mgc.Draw(mgi);
        }
    }
}
