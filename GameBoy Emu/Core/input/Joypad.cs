using GameBoy_Emu.core.utils;
using SDL2;
using System;

namespace GameBoy_Emu.core.input
{
    public class Joypad : InputDevice
    {
        public override void ProcessInput(SDL.SDL_Event sdlEvent)
        {
            //if (sdlEvent.type == SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN)
            //{
            //    switch (sdlEvent.cbutton.which)
            //    {
            //        case (int)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A:
            //            PressButton(0);
            //            break;
            //        case (int)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START:
            //            PressButton(3);
            //            break;

            //    }
            //}
        }
    }
}