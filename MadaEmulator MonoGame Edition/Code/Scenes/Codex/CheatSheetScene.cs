using Blunatic.Core;
using Blunatic.Mgc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Blunatic.Parsing;

namespace MadaEmulator_MonoGame_Edition
{
    public class CheatSheetScene : IScene
    {
        // Properties
        public bool UpdatePreviousScene => false;
        public bool DrawPreviousScene => false;

        // Fields
        private MonoGameConsole _mgc;

        private CheatSheetMasterTableElement _masterTableElement;

        bool _hasDrawn;

        // Constructors
        public CheatSheetScene(MonoGameInstance mgi)
        {
            _mgc = new MonoGameConsole(mgi, new Vec(103, 32));
            _mgc.ClearScreenAfterDraw = false;

            _masterTableElement = new CheatSheetMasterTableElement(new Vec(2, 2));

            _hasDrawn = false;
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
        }
        public void Draw(MonoGameInstance mgi)
        {
            if (!_hasDrawn)
            {
                _hasDrawn = true;
                _mgc.Box(new Rectangle(Vec.Zero, _mgc.Dimensions));
                _masterTableElement.Draw(mgi, _mgc);
            }
            _mgc.Draw(mgi);
        }
    }
}
