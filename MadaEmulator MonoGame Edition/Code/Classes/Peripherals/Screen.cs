using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public class Screen : IPeripheral
    {
        // Properties
        public ReadOnlyCollection<byte> Bytes
        {
            get { return Array.AsReadOnly(_bytes); }
        }

        // Fields
        private byte[] _bytes;

        // Constructors
        public Screen(Emulator emulator)
        {
            _bytes = new byte[8];

            InputToEmulator(emulator);
            OutputFromEmulator(emulator);
        }
        public Screen(Screen toCopy)
        {
            _bytes = new byte[8];
            Array.Copy(toCopy._bytes, _bytes, 8);
        }

        // Methods
        public void InputToEmulator(Emulator emulator)
        {

        }
        public void OutputFromEmulator(Emulator emulator)
        {
            int i = 0;
            foreach (byte b in Emulator.MEMORY_OUT_SCREEN)
            {
                _bytes[i++] = emulator.Memory[b];
            }
        }
        public IPeripheral Clone()
        {
            return new Screen(this);
        }
    }
}
