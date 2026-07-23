using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Blunatic.Parsing;

namespace Blunatic.Core
{
    public class Controls
    {
        // Enums
        [Flags]
        public enum Modifier : byte
        {
            None =     0b00000000,
            Ctrl =     0b00000011,
            Alt =      0b00001100,
            Shift =    0b00110000,
            Any =      0b11111111,
        }

        // Structs
        public struct ControlAssignment
        {
            public Modifier Modifier;
            public Keys Key;

            public ControlAssignment(Modifier modifier, Keys key)
            {
                Modifier = modifier;
                Key = key;
            }

            public readonly bool CheckModifier(Modifier modifier)
            {
                if (Modifier == Modifier.Any) return true;
                return modifier == Modifier;
            }
            public readonly bool Check(Modifier modifier, Keys key)
            {
                return key == Key && CheckModifier(modifier);
            }

            public readonly override string ToString()
            {
                return $"[{Modifier}+{Key}]";
            }
        }

        // Fields
        private Dictionary<string, ControlAssignment[]> _controlAssignments;

        // Constructors
        public Controls(string path)
        {
            _controlAssignments = new Dictionary<string, ControlAssignment[]>();

            if (!HTML.Parse(path).TryGetElementWithKeyword("controls", out HTML.Element controlsElement)) throw new BlunaticException($"'{path}' is an invalid controls file.");

            foreach (HTML.Element controlElement in controlsElement.Elements)
            {
                if (controlElement.Keyword != "control") continue;
                if (!controlElement.TryGetParameter("Name", out string name)) throw new BlunaticException($"Control entry missing Name.");

                List<ControlAssignment> controlAssignments = new List<ControlAssignment>();

                foreach (HTML.Element keybindElement in controlElement.Elements)
                {
                    if (keybindElement.Keyword != "keybind") continue;
                    if (!keybindElement.TryGetParameter("Key", out string key)) throw new BlunaticException($"Keybind entry missing Key.");
                    if (!keybindElement.TryGetParameter("Modifier", out string modifier)) modifier = Modifier.Any.ToString();
                    controlAssignments.Add(new ControlAssignment(Enum.Parse<Modifier>(modifier), Enum.Parse<Keys>(key)));
                }

                _controlAssignments.Add(name, controlAssignments.ToArray());
            }
        }

        // Methods
        public ControlAssignment[] GetKeys(string control)
        {
            if (_controlAssignments.TryGetValue(control, out ControlAssignment[] value))
            {
                return value;
            }
            else
            {
                return new ControlAssignment[0];
            }
        }
    }
}
