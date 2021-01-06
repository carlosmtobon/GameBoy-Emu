using GameBoy_Emu.core.utils;
using SDL2;
using System;

namespace GameBoy_Emu.core.input
{
    public class Joypad : InputDevice
    {
        private const int DEAD_ZONE = 6000;
        public override void ProcessInput(SDL.SDL_Event sdlEvent)
        {
            if (sdlEvent.type == SDL.SDL_EventType.SDL_JOYAXISMOTION)
            {
                switch (sdlEvent.jaxis.axis)
                {
                    case 0:
                        if (sdlEvent.jaxis.axisValue < -DEAD_ZONE)
                            PressDirection(1);
                        else if (sdlEvent.jaxis.axisValue > DEAD_ZONE)
                            PressDirection(0);
                        else
                        {
                            ReleaseDirection(0);
                            ReleaseDirection(1);
                        }

                        break;
                    case 1:
                        if (sdlEvent.jaxis.axisValue < -DEAD_ZONE)
                            PressDirection(2);
                        else if (sdlEvent.jaxis.axisValue > DEAD_ZONE)
                            PressDirection(3);
                        else
                        {
                            ReleaseDirection(2);
                            ReleaseDirection(3);
                        }
                        break;
                }

            }

                if (sdlEvent.type == SDL.SDL_EventType.SDL_JOYBUTTONDOWN)
            {
                switch (sdlEvent.jbutton.button)
                {
                    case 1:
                        PressButton(0);
                        break;
                    case 2:
                        PressButton(1);
                        break;
                    case 8:
                        PressButton(2);
                        break;
                    case 9:
                        PressButton(3);
                        break;

                }
            }
            if (sdlEvent.type == SDL.SDL_EventType.SDL_JOYBUTTONUP)
            {
                switch (sdlEvent.jbutton.button)
                {
                    case 1:
                        ReleaseButton(0);
                        break;
                    case 2:
                        ReleaseButton(1);
                        break;
                    case 8:
                        ReleaseButton(2);
                        break;
                    case 9:
                        ReleaseButton(3);
                        break;

                }
            }
        }
    }
}