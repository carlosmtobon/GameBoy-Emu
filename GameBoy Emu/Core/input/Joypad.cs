using System.ComponentModel;
using System.Diagnostics;
using GameBoy_Emu.core.cpu;
using GameBoy_Emu.core.ram;
using GameBoy_Emu.core.utils;

namespace GameBoy_Emu.core.input
{
    public class Joypad
    {
        private Mmu _ram;

        public Joypad(Mmu ram)
        {
            _ram = ram;
        }

        public enum Input
        {
            DOWN,
            START,
            UP,
            SELECT,
            LEFT,
            B,
            RIGHT,
            A
        }

        public enum InputType
        {
            BUTTON,
            DIRECTION
        }

        public void SetKeyPressed(Input input)
        {
            byte joyp = _ram.LoadUnsigned8(Mmu.JOYP_REGISTER);
            InputType type = GetInputType(joyp);
            joyp >>= 4;
            joyp |= 0xf;
            if ((type == InputType.BUTTON && IsButton(input)) || (type == InputType.DIRECTION && IsDirection(input)))
                SetInput(input, joyp, true);
        }

        public void SetKeyReleased(Input input)
        {
            byte joyp = _ram.LoadUnsigned8(Mmu.JOYP_REGISTER);
            InputType type = GetInputType(joyp);
            joyp >>= 4;
            joyp |= 0xf;
            if ((type == InputType.BUTTON && IsButton(input)) || (type == InputType.DIRECTION && IsDirection(input)))
                SetInput(input, joyp, false);
        }

        private bool IsDirection(Input input)
        {
            return input == Input.UP || input == Input.DOWN || input == Input.LEFT || input == Input.RIGHT;
        }

        private bool IsButton(Input input)
        {
            return input == Input.A || input == Input.B || input == Input.SELECT || input == Input.START;
        }

        private static InputType GetInputType(byte joyp)
        {
            return BitUtils.isBitSet(joyp, 5) ? InputType.BUTTON : InputType.DIRECTION;
        }

        private void SetInput(Input input, byte joyp, bool pressed)
        {
            if (input == Input.LEFT || input == Input.B)
            {
                if (pressed)
                    joyp = BitUtils.ClearBit(joyp, 1);
                else
                    joyp = BitUtils.SetBit(joyp, 1);
            }
            else if (input == Input.RIGHT || input == Input.A)
            {
                if (pressed)
                    joyp = BitUtils.ClearBit(joyp, 0);
                else
                    joyp = BitUtils.SetBit(joyp, 0);
            }
            else if (input == Input.UP || input == Input.SELECT)
            {
                if (pressed)
                    joyp = BitUtils.ClearBit(joyp, 2);
                else
                    joyp = BitUtils.SetBit(joyp, 2);
            }
            else if (input == Input.DOWN || input == Input.START)
            {
                if (pressed)
                    joyp = BitUtils.ClearBit(joyp, 3);
                else
                    joyp = BitUtils.SetBit(joyp, 3);
            }

            byte interruptFlag = _ram.LoadUnsigned8(Mmu.IF_REGISTER);
            interruptFlag = BitUtils.SetBit(interruptFlag, 4);
            _ram.StoreUnsigned8(Mmu.IF_REGISTER, interruptFlag);
            _ram.StoreUnsigned8(Mmu.JOYP_REGISTER, joyp);
            Debug.WriteLine("Key pressed/released");
        }
    }
}