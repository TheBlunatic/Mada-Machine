using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Blunatic.Core;

namespace Blunatic.Mgc
{
    public class MonoGameConsoleText : IMonoGameConsoleElement
    {
        // Properties
        public Vec Position => _position;
        public Vec Dimensions => _dimensions;
        public bool CapturingControls => false;

        // Fields
        private Vec _position;
        private string _text;
        private Vec _dimensions;

        // Constructors
        public MonoGameConsoleText(MonoGameInstance mgi, Vec pos, string text, Color? foregroundColor = null, Color? backgroundColor = null)
        {
            Color _foregroundColor = foregroundColor == null ? Color.White : foregroundColor.Value;
            Color _backgroundColor = backgroundColor == null ? Color.Black : backgroundColor.Value;
            _position = pos;
            _text = text;
            _text = $"{Fm.Fg(_foregroundColor)}{Fm.Bg(_backgroundColor)}{_text}";
            _dimensions = MonoGameConsole.GetWriteStringDimensions(mgi, _text);
        }

        // Methods
        public void Centre()
        {
            _position.X -= _dimensions.X / 2;
        }

        public void Update(MonoGameInstance mgi, MonoGameConsole mgc)
        {

        }

        public void Draw(MonoGameInstance mgi, MonoGameConsole mgc)
        {
            mgc.WriteString(mgi, _position, _text);
        }
    }
}
