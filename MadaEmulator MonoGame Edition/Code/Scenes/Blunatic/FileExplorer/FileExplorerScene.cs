using Blunatic.Core;
using Blunatic.Mgc;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Blunatic.Scenes
{
    public class FileExplorerScene : IScene
    {
        // Constants
        private const int DOUBLE_CLICK_TICKS = 20;

        // Enums
        public enum Mode
        {
            Open,
            Save,
        }
        private enum EntryType
        {
            Directory,
            File,
        }

        // Classes
        private class Entry
        {
            public EntryType Type;
            public string Path;

            public Entry(EntryType type, string path) {
                Type = type;
                Path = path;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is Entry objEntry)
                {
                    return objEntry.Type == Type && objEntry.Path == Path;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Type, Path);
            }
        }
        public class FileExplorerResultEventArgs : EventArgs
        {
            public MonoGameInstance MonoGameInstance { get; init; }
            public Mode Mode { get; init; }
            public string Path { get; init; }

            private RejectionStatus _rejectionStatusObject;

            public FileExplorerResultEventArgs(MonoGameInstance mgi, RejectionStatus rejectionStatusObject)
            {
                MonoGameInstance = mgi;
                _rejectionStatusObject = rejectionStatusObject;
            }

            public void Reject(string message) 
            {
                _rejectionStatusObject.HasBeenRejected = true;
                _rejectionStatusObject.RejectionMessage = message;
            }
        }

        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => false;

        // Events
        public event EventHandler<FileExplorerResultEventArgs> FileSelected;

        // Fields
        private MonoGameConsole _mgc;
        private string _currentDirectory;
        private Mode _mode;

        private List<Entry> _entries;

        private int _scroll;

        private MonoGameConsoleTextInput _textInput;

        private Entry _selectedEntry = null;
        private int _selectedFileTick = -1000;

        private bool _inputJustChanged = false;

        // Constructors
        public FileExplorerScene(MonoGameInstance mgi, Mode mode, string directory, string initialInput = null)
        {
            _mode = mode;
            _mgc = new MonoGameConsole(mgi, new Vec(96, 36));
            _currentDirectory = directory;

            if (mode == Mode.Save)
            {
                _textInput = new MonoGameConsoleTextInput(new Vec(1, _mgc.Dimensions.Y - 2), _mgc.Dimensions.X - 2, initialInput == null ? string.Empty : initialInput, Input.KeyState.FILENAME);
                _textInput.TextUpdated += _getInput;
                _mgc.AddElement(_textInput);
            }

            _entries = new List<Entry>();

            _selectedEntry = null;

            _reloadDirectory();
        }

        // Methods
        private void _reloadDirectory()
        {
            _entries.Clear();

            _scroll = 0;

            foreach (string directory in Directory.GetDirectories(_currentDirectory))
            {
                _entries.Add(new Entry(EntryType.Directory, directory));
            }
            foreach (string file in Directory.GetFiles(_currentDirectory))
            {
                _entries.Add(new Entry(EntryType.File, file));
            }
        }
        private void _getInput(object sender, MonoGameConsoleTextInput.TextUpdatedEventArgs e)
        {
            _inputJustChanged = true;
        }
        private bool _attemptReturn(MonoGameInstance mgi, string path)
        {
            mgi.SceneOut();

            RejectionStatus rejectionStatusObject = new RejectionStatus();
            FileSelected?.Invoke(this, new FileExplorerResultEventArgs(mgi, rejectionStatusObject) { Mode = _mode, Path = path });

            if (!rejectionStatusObject.HasBeenRejected)
            {
                return true;
            }

            mgi.SceneIn(this);
            mgi.SceneIn(new PopupScene(mgi, PopupScene.Type.Error, rejectionStatusObject.RejectionMessage));

            return false;
        }
        private void _selectEntry(MonoGameInstance mgi, Entry entry)
        {
            _selectedFileTick = mgi.Ticks;
            _selectedEntry = entry;
            if (_mode == Mode.Save && entry.Type == EntryType.File)
            {
                _textInput.Text = _selectedEntry.Path.Split('\\').Last();
            }
        }
        private void _enterDirectory(MonoGameInstance mgi, Entry entry)
        {
            _currentDirectory = entry.Path;
            _selectedFileTick = -1 - DOUBLE_CLICK_TICKS;
            _selectedEntry = null;
            _reloadDirectory();
        }

        // Scene Methods
        public void Update(MonoGameInstance mgi)
        {
            Vec hoveredCell = _mgc.GetCursorHoveredCellPos(mgi);

            bool controlsBeingCaptured = false;
            if (_textInput != null) controlsBeingCaptured = _textInput.CapturingControls;

            if (!controlsBeingCaptured && mgi.ControlWasJustPressed("escape"))
            {
                mgi.SceneOut();
                return;
            }

            _mgc.Update(mgi);

            if (_inputJustChanged)
            {
                _inputJustChanged = false;
            }
            else if (!controlsBeingCaptured && mgi.ControlWasJustPressed("navigate forwards"))
            {
                if (_selectedEntry != null && _selectedEntry.Type == EntryType.Directory)
                {
                    _enterDirectory(mgi, _selectedEntry);
                }
                else if (_mode == Mode.Save)
                {
                    _attemptReturn(mgi, $"{_currentDirectory}\\{_textInput.Text}");
                    return;
                }
                else if (_mode == Mode.Open)
                {
                    if (_selectedEntry != null && _selectedEntry.Type == EntryType.Directory)
                    {
                        _attemptReturn(mgi, _selectedEntry.Path);
                        return;
                    }
                }
            }

            _scroll -= Math.Sign(mgi.CursorState.GetCursorScrollThisTick());
            if (_scroll < 0) _scroll = 0;
            if (_scroll > _entries.Count) _scroll = _entries.Count;
            if (_entries.Count != 0 && _scroll == _entries.Count) _scroll--;

            if (hoveredCell.Y - 2 >= 0 && hoveredCell.Y - 2 <= _mgc.Dimensions.Y - 7 && hoveredCell.Y - 2 + _scroll < _entries.Count && hoveredCell.X >= 2 && hoveredCell.X <= _mgc.Dimensions.X - 3)
            {
                Entry hoveredEntry = _entries[hoveredCell.Y - 2 + _scroll];

                if (mgi.CursorState.WasJustPressed(MouseButton.Left))
                {
                    if (hoveredEntry.Type == EntryType.Directory)
                    {
                        if (hoveredEntry.Equals(_selectedEntry))
                        {
                            if (mgi.Ticks - _selectedFileTick < DOUBLE_CLICK_TICKS)
                            {
                                _enterDirectory(mgi, _selectedEntry);
                            }
                            else
                            {
                                _selectedFileTick = mgi.Ticks;
                                return;
                            }
                        }
                        else
                        {
                            _selectEntry(mgi, hoveredEntry);
                        }
                    }
                    else if (hoveredEntry.Type == EntryType.File)
                    {
                        if (hoveredEntry.Equals(_selectedEntry))
                        {
                            if (mgi.Ticks - _selectedFileTick < DOUBLE_CLICK_TICKS)
                            {
                                _attemptReturn(mgi, _selectedEntry.Path);
                                return;
                            }
                            else
                            {
                                _selectEntry(mgi, hoveredEntry);
                            }
                        }
                        else
                        {
                            _selectEntry(mgi, hoveredEntry);
                        }
                    }
                }
            }

            if ((mgi.CursorState.WasJustPressed(MouseButton.Right) || !controlsBeingCaptured && mgi.ControlWasJustPressed("navigate back")) && _currentDirectory.Split('\\').Length > 1)
            {
                string oldDirectory = _currentDirectory;
                _currentDirectory = _currentDirectory.Substring(0, _currentDirectory.Length - _currentDirectory.Split('\\').Last().Length).TrimEnd('\\');
                _reloadDirectory();
                for (int i = 0; i < _entries.Count; i++)
                {
                    if (_entries[i].Type == EntryType.Directory && _entries[i].Path == oldDirectory)
                    {
                        _selectEntry(mgi, _entries[i]);
                        break;
                    }
                }
                return;
            }

            if (!controlsBeingCaptured && mgi.ControlWasJustPressed("navigate down") && _entries.Count != 0)
            {
                int index = _entries.IndexOf(_selectedEntry);
                if (index == -1)
                {
                    index = 0;
                }
                else if (index == _entries.Count - 1)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }
                _selectEntry(mgi, _entries[index]);
            }
            if (!controlsBeingCaptured && mgi.ControlWasJustPressed("navigate up") && _entries.Count != 0)
            {
                int index = _entries.IndexOf(_selectedEntry);
                if (index == -1)
                {
                    index = 0;
                }
                else if (index == 0)
                {
                    index = _entries.Count - 1;
                }
                else
                {
                    index--;
                }
                _selectEntry(mgi, _entries[index]);
            }
        }

        public void Draw(MonoGameInstance mgi)
        {
            _mgc.Box(mgi, new Rectangle(new Vec(0,0), _mgc.Dimensions), $"{Fm.Fg(Color.White)}{_currentDirectory}", Color.DarkGray, Color.Black, true);
            _mgc.Box(Vec.GetRectangle(new Vec(0, _mgc.Dimensions.Y - 3), new Vec(_mgc.Dimensions.X - 1, _mgc.Dimensions.Y - 1)), Color.DarkGray, Color.Black, true);
            _mgc.SetCell(new Vec(0, _mgc.Dimensions.Y - 3), Ch.Border.n2.e2.s2.w0);
            _mgc.SetCell(new Vec(_mgc.Dimensions.X - 1, _mgc.Dimensions.Y - 3), Ch.Border.n2.e0.s2.w2);
            _mgc.WriteString(mgi, new Vec(_mgc.Dimensions.X - 2 - _mode.ToString().Length, 0), $"{Fm.Fg(Color.Yellow)}{_mode.ToString()}");

            Vec hoveredCell = _mgc.GetCursorHoveredCellPos(mgi);

            for (int i = 0; i <= _mgc.Dimensions.Y - 7 && i + _scroll < _entries.Count; i++)
            {
                Entry entry = _entries[i + _scroll];

                if (entry.Type == EntryType.Directory)
                {
                    _mgc.WriteString(mgi, new Vec(2, i + 2), $"{Fm.Fg(Color.Cyan)}○ {entry.Path.Split('\\').Last()}");
                }
                else if (entry.Type == EntryType.File)
                {
                    _mgc.WriteString(mgi, new Vec(2, i + 2), $"{Fm.Fg(Color.Firebrick)}∙ {entry.Path.Split('\\').Last()}");
                }

                if (i + 2 == hoveredCell.Y && hoveredCell.X >= 2 && hoveredCell.X <= _mgc.Dimensions.X - 3)
                {
                    _mgc.Fill(Vec.GetRectangle(new Vec(2, i + 2), new Vec(_mgc.Dimensions.X - 3, i + 2)), null, null, new Color(20, 20, 20));
                }
                if (entry.Equals(_selectedEntry))
                {
                    _mgc.Fill(Vec.GetRectangle(new Vec(2, i + 2), new Vec(_mgc.Dimensions.X - 3, i + 2)), null, null, new Color(40, 40, 40));
                }
            }

            if (_mode == Mode.Open && _selectedEntry != null)
            {
                _mgc.WriteString(mgi, new Vec(1, _mgc.Dimensions.Y - 2), _selectedEntry.Path.Split('\\').Last());
            }

            _mgc.Draw(mgi);
        }

    }
}
