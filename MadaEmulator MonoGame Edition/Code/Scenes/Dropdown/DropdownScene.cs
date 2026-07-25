using Blunatic.Core;
using Blunatic.Mgc;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public class DropdownScene : IScene
    {
        // Classes
        public class DropdownResultEventArgs : EventArgs
        {
            public MonoGameInstance MonoGameInstance;
            public int SelectedIndex { get; init; }

            public DropdownResultEventArgs(MonoGameInstance mgi)
            {
                MonoGameInstance = mgi;
            }
        }

        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => true;

        // Fields
        private MonoGameConsole _mgc;
        private MonoGameConsoleButton[] _buttons;
        private MonoGameConsoleRegion _screenRegion;
        private Rectangle _rectangle;

        // Events
        public event EventHandler<DropdownResultEventArgs> ResultSelected;

        // Constructors
        public DropdownScene(MonoGameInstance mgi, Vec monoGameConsoleDimensions, Vec origin, params string[] buttons)
        {
            if (buttons.Length <= 0)
            {
                throw new ArgumentException("Dropdown menu must have at least one button.");
            }

            _mgc = new MonoGameConsole(mgi, monoGameConsoleDimensions);
            _mgc.Transparency = MonoGameConsole.TransparencyType.NotWrittenTo;
            _buttons = new MonoGameConsoleButton[buttons.Length];

            _screenRegion = new MonoGameConsoleRegion(Vec.Zero, _mgc.Dimensions);
            _mgc.AddElement(_screenRegion);

            int longestButton = buttons.Aggregate(0, (i, x) => Math.Max(i, x.Length));

            for (int i = 0; i < buttons.Length; i++)
            {
                _buttons[i] = new MonoGameConsoleButton(mgi, origin + new Vec(3, i + 2), buttons[i].PadRight(longestButton, ' '), false);
                _buttons[i].InactiveBackgroundColor = new Color(60, 60, 60);
                _buttons[i].ActiveBackgroundColor = new Color(90, 90, 90);
                _mgc.AddElement(_buttons[i]);
            }

            _rectangle = new Rectangle(origin.X, origin.Y + 1, longestButton + 4, buttons.Length + 2);
        }

        // Methods
        public void Update(MonoGameInstance mgi)
        {
            _mgc.Update(mgi);

            if (mgi.ControlWasJustPressed("escape"))
            {
                mgi.SceneOut();
                return;
            }

            if (_screenRegion.IsLeftClicked || _screenRegion.IsRightClicked)
            {
                mgi.SceneOut();
                for (int i = 0; i < _buttons.Length; i++)
                {
                    if (_buttons[i].IsLeftClicked)
                    {
                        ResultSelected?.Invoke(this, new DropdownResultEventArgs(mgi) { SelectedIndex = i });
                        return;
                    }
                }
                return;
            }
        }
        public void Draw(MonoGameInstance mgi)
        {
            _mgc.Box(_rectangle, Color.White, _buttons[0].InactiveBackgroundColor, false);
            _mgc.Fill(new Rectangle(_rectangle.X, _rectangle.Y, 1, _buttons.Length + 1), Ch.Border.n1.e1.s1.w0);
            _mgc.Fill(new Rectangle(_rectangle.X + 1, _rectangle.Y + 1, 1, _buttons.Length), Ch.Border.n0.e1.s0.w1);
            _mgc.Fill(new Rectangle(_rectangle.X + 1, _rectangle.Y + 1, 1, _buttons.Length), Ch.ArrowRight);
            _mgc.Draw(mgi);
        }
    }
}
