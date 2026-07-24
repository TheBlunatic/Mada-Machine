using Blunatic.Core;
using Blunatic.Mgc;
using Blunatic.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Blunatic.Mathematics;

namespace MadaEmulator_MonoGame_Edition
{
    public class EmulatorScene : IScene
    {
        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => false;

        // Fields
        private MonoGameConsole _mgc;

        private MonoGameConsoleButton _fileButton;

        private MonoGameConsoleRegion _canvasRegion;

        private string _programPath;
        private string _programDirectory;

        private Emulator _emulator;

        // Constructors
        public EmulatorScene(MonoGameInstance mgi)
        {
            _mgc = new MonoGameConsole(mgi, new Vec(120, 45));

            _fileButton = new MonoGameConsoleButton(mgi, new Vec(1, 1), "File", true);
            _mgc.AddElement(_fileButton);

            _canvasRegion = new MonoGameConsoleRegion(new Rectangle(1, 3, _mgc.Dimensions.X - 2, _mgc.Dimensions.Y - 4));
            _mgc.AddElement(_canvasRegion);

            _programPath = null;
            _programDirectory = null;
            _emulator = null;
        }

        // Methods
        public void RespondToFileExplorer(object sender, FileExplorerScene.FileExplorerResultEventArgs args)
        {
            if (!args.Path.EndsWith(".txt"))
            {
                args.Reject("Expected a .txt file.");
                return;
            }
            _programPath = args.Path;
            _programDirectory = _programPath.Substring(0, _programPath.Length - _programPath.Split('\\').Last().Length - 1);
            _emulator = new Emulator(_programPath);
        }

        public void Update(MonoGameInstance mgi)
        {
            _mgc.Update(mgi);

            if (mgi.ControlWasJustPressed("escape"))
            {
                mgi.SceneOut();
                return;
            }
            if (mgi.ControlWasJustPressed("load program"))
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
                return;
            }
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
            _mgc.WriteString(mgi, _canvasRegion.Position, $"Registers:");
            for (int i = 0; i < _emulator.Registers.Length; i++)
            {
                Vec drawPos = _canvasRegion.Position + new Vec(0, i + 1);
                _mgc.WriteString(mgi, drawPos, $"{Fm.Fg(Color.DarkGray)}{$"r{i}".PadLeft(4, ' ')}: 0b{Convert.ToString(_emulator.Registers[i], 2).PadLeft(8, '0')} ({_emulator.Registers[i]})");
            }
            _mgc.WriteString(mgi, _canvasRegion.Position + new Vec(0, _emulator.Registers.Length + 2), $"Memory:");
            for (int i = 0; i < 16; i++)
            {
                Vec drawPos = _canvasRegion.Position + new Vec(0, _emulator.Registers.Length + 3 + i);
                _mgc.WriteString(mgi, drawPos, $" {Fm.Fg(Color.DarkGray)}{$"{i * 16}".PadLeft(3, ' ')} - {$"{i * 16 + 15}".PadLeft(3, ' ')}:{Iterate.InBounds(i * 16, i * 16 + 16).Aggregate(string.Empty, (s, v) => 
                {  
                    string addition = _emulator.Memory[v].ToString("x").PadLeft(2, '0');
                    switch (addition)
                    {
                        case "00":
                            addition = $"{Fm.Fg(Color.DarkGray)}{addition}";
                            break;
                        default:
                            addition = $"{Fm.Fg(Color.White)}{addition}";
                            break;
                    }
                    return $"{s} {addition}";
                })}");
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
