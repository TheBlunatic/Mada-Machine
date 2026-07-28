using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadaEmulator_MonoGame_Edition
{
    public class Controller : IPeripheral
    {
        // Properties
        public bool ShockActive
        {
            get { return _shockActive; }
            set
            {
                if (_shockActive && !value)
                {
                    if (!_enterAcknowledge)
                    {
                        _enterActive = false;
                    }
                    if (!_colourAcknowledge)
                    {
                        _redActive = false;
                        _greenActive = false;
                        _blueActive = false;
                    }
                }
                else if (!_shockActive && value)
                {
                    _redActive = true;
                    _greenActive = true;
                    _blueActive = true;
                    _enterActive = true;
                }
                _shockActive = value;
            }
        }

        public bool RedActive
        {
            get { return _redActive; }
            set
            {
                if (value)
                {
                    _redActive = true;
                }
            }
        }
        public bool GreenActive
        {
            get { return _greenActive; }
            set
            {
                if (value)
                {
                    _greenActive = true;
                }
            }
        }
        public bool BlueActive
        {
            get { return _blueActive; }
            set
            {
                if (value)
                {
                    _blueActive = true;
                }
            }
        }
        public bool EnterActive
        {
            get { return _enterActive; }
            set
            {
                if (value)
                {
                    _enterActive = true;
                }
            }
        }
        public byte ByteInput
        {
            get { return _byteInput; }
            set
            {
                _byteInput = value;
            }
        }

        public bool ColourAcknowledge { get { return _colourAcknowledge; } }
        public bool EnterAcknowledge { get { return _enterAcknowledge; } }

        // Fields
        private bool _shockActive;

        private bool _redActive;
        private bool _greenActive;
        private bool _blueActive;
        private bool _enterActive;
        private byte _byteInput;

        private bool _colourAcknowledge;
        private bool _enterAcknowledge;

        // Constructors
        public Controller(Emulator emulator)
        {
            _shockActive = false;

            _redActive = false;
            _greenActive = false;
            _blueActive = false;
            _enterActive = false;
            _byteInput = 0;

            _enterAcknowledge = false;
            _colourAcknowledge = false;

            InputToEmulator(emulator);
            OutputFromEmulator(emulator); 
        }
        public Controller(Controller toCopy)
        {
            _shockActive = toCopy._shockActive;

            _redActive = toCopy._redActive;
            _greenActive = toCopy._greenActive;
            _blueActive = toCopy._blueActive;
            _enterActive = toCopy._enterActive;
            _byteInput = toCopy._byteInput;

            _colourAcknowledge = toCopy._colourAcknowledge;
            _enterAcknowledge = toCopy._enterAcknowledge;
        }

        // Methods
        public void InputToEmulator(Emulator emulator)
        {
            emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BYTE] = _byteInput;

            if (_redActive) emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] |= 0b10000000;
            else emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] &= 0b01111111;

            if (_greenActive) emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] |= 0b01000000;
            else emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] &= 0b10111111;

            if (_blueActive) emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] |= 0b00100000;
            else emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] &= 0b11011111;

            if (_enterActive) emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] |= 0b00010000;
            else emulator.Memory[Emulator.MEMORY_IN_CONTROLLER_BUTTONS] &= 0b11101111;
        }
        public void OutputFromEmulator(Emulator emulator)
        {
            bool newEnterAcknowledge = (emulator.Memory[Emulator.MEMORY_OUT_FLAGS] & 0b00000010) != 0;
            if (_enterAcknowledge && !newEnterAcknowledge && !_shockActive)
            {
                _enterActive = false;
            }
            else if (!_enterAcknowledge && newEnterAcknowledge)
            {
                _enterActive = true;
            }
            _enterAcknowledge = newEnterAcknowledge;

            bool newColourAcknowledge = (emulator.Memory[Emulator.MEMORY_OUT_FLAGS] & 0b00000001) != 0;
            if (_colourAcknowledge && !newColourAcknowledge && !_shockActive)
            {
                _redActive = false;
                _greenActive = false;
                _blueActive = false;
            }
            else if (!_colourAcknowledge && newColourAcknowledge)
            {
                _redActive = true;
                _greenActive = true;
                _blueActive = true;
            }
            _colourAcknowledge = newColourAcknowledge;
        }
        public IPeripheral Clone()
        {
            return new Controller(this);
        }
    }
}
