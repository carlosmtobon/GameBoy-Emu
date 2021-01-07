using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.input
{
    class Keyboard : InputDevice
    {
        protected override void ProcessInput(SDL.SDL_Event sdlEvent)
        {
            ProcessKeyDown(sdlEvent);

            ProcessKeyUp(sdlEvent);
        }

        private void ProcessKeyUp(SDL.SDL_Event sdlEvent)
        {
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

        private void ProcessKeyDown(SDL.SDL_Event sdlEvent)
        {
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
        }
    }
}
