using GameBoy_Emu.core.utils;
using SDL2;

namespace GameBoy_Emu.core.input
{
    public class Joypad
    {
        private byte _directions = 0xf;
        private byte _buttons = 0xf;

        private void PressDirection(int bit)
        {
            _directions = BitUtils.ClearBit(_directions, bit);
        }

        private void PressButton(int bit)
        {
            _buttons = BitUtils.ClearBit(_buttons, bit);
        }

        private void ReleaseDirection(int bit)
        {
            _directions = BitUtils.SetBit(_directions, bit);
        }

        private void ReleaseButton(int bit)
        {
            _buttons = BitUtils.SetBit(_buttons, bit);
        }

        public byte Process(byte b)
        {
            bool interruptRequest = false;
            if ((b & 0x30) == 0x10)
            {
                b |= _buttons;
                interruptRequest = true;
            }
            else if ((b & 0x30) == 0x20)
            {
                b |= _directions;
                interruptRequest = true;
            }
            else
            {
                b |= 0xf;
            }

            if (interruptRequest)
            {
                // joyp irq
            }

            return b;
        }

        public bool HandleInput()
        {
            while (SDL.SDL_PollEvent(out var sdlEvent) != 0)
            {
                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                    return false;
                if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYDOWN)
                {
                    switch (sdlEvent.key.keysym.sym)
                    {
                        case SDL.SDL_Keycode.SDLK_RIGHT:
                            PressDirection(0);
                            break;
                        case SDL.SDL_Keycode.SDLK_LEFT:
                            PressDirection(1);
                            break;
                        case SDL.SDL_Keycode.SDLK_UP:
                            PressDirection(2);
                            break;
                        case SDL.SDL_Keycode.SDLK_DOWN:
                            PressDirection(3);
                            break;
                        case SDL.SDL_Keycode.SDLK_z:
                            PressButton(0);
                            break;
                        case SDL.SDL_Keycode.SDLK_x:
                            PressButton(1);
                            break;
                        case SDL.SDL_Keycode.SDLK_SPACE:
                            PressButton(2);
                            break;
                        case SDL.SDL_Keycode.SDLK_RETURN:
                            PressButton(3);
                            break;
                    }
                }

                if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYUP)
                {
                    switch (sdlEvent.key.keysym.sym)
                    {
                        case SDL.SDL_Keycode.SDLK_RIGHT:
                            ReleaseDirection(0);
                            break;
                        case SDL.SDL_Keycode.SDLK_LEFT:
                            ReleaseDirection(1);
                            break;
                        case SDL.SDL_Keycode.SDLK_UP:
                            ReleaseDirection(2);
                            break;
                        case SDL.SDL_Keycode.SDLK_DOWN:
                            ReleaseDirection(3);
                            break;
                        case SDL.SDL_Keycode.SDLK_z:
                            ReleaseButton(0);
                            break;
                        case SDL.SDL_Keycode.SDLK_x:
                            ReleaseButton(1);
                            break;
                        case SDL.SDL_Keycode.SDLK_SPACE:
                            ReleaseButton(2);
                            break;
                        case SDL.SDL_Keycode.SDLK_RETURN:
                            ReleaseButton(3);
                            break;
                    }
                }
            }

            return true;
        }
    }
}