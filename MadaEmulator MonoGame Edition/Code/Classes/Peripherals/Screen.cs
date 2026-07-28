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

        public bool ScreenBlocked { get { return _screenBlocked; } }

        // Fields
        private byte[] _bytes;
        private bool _screenBlocked;

        // Constructors
        public Screen(Emulator emulator)
        {
            _bytes = new byte[8];
            _screenBlocked = false;

            InputToEmulator(emulator);
            OutputFromEmulator(emulator);
        }
        public Screen(Screen toCopy)
        {
            _bytes = new byte[8];
            Array.Copy(toCopy._bytes, _bytes, 8);

            _screenBlocked = toCopy._screenBlocked;
        }

        // Methods
        public void InputToEmulator(Emulator emulator)
        {

        }
        public void OutputFromEmulator(Emulator emulator)
        {
            _screenBlocked = (emulator.Memory[Emulator.MEMORY_OUT_FLAGS] & 0b00000100) != 0;

            if (_screenBlocked)
            {
                int i = 0;
                foreach (byte b in Emulator.MEMORY_OUT_SCREEN)
                {
                    _bytes[i++] = 0;
                }
            }
            else
            {
                int i = 0;
                foreach (byte b in Emulator.MEMORY_OUT_SCREEN)
                {
                    _bytes[i++] = emulator.Memory[b];
                }
            }
        }
        public IPeripheral Clone()
        {
            return new Screen(this);
        }
    }
}
