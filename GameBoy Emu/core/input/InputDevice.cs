﻿using GameBoy_Emu.core.utils;
using SDL2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoy_Emu.core.input
{
    public abstract class InputDevice
    {
        private byte _directions = 0xf;
        private byte _buttons = 0xf;
        private static IntPtr _gamecontroller = IntPtr.Zero;

        public bool InterruptRequest { get; set; }

        protected abstract void ProcessInput(SDL.SDL_Event sdlEvent);

        internal static InputDevice GetInstance()
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK) < 0)
            {
                 Debug.WriteLine("Failed to Init SDL");
                SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);
                return new Keyboard();
            }

            if (SDL.SDL_NumJoysticks() < 1)
            {
                 Debug.WriteLine("Warning: No game controller connected!");
                SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);
                return new Keyboard();
            }
            
            _gamecontroller = SDL.SDL_JoystickOpen(0);
            if (_gamecontroller != null) return new Joypad();
                
             Debug.WriteLine("Warning: Unable to open game controller! Sdl Error: " + SDL.SDL_GetError());
            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);
            return new Keyboard();
        }
        protected void PressDirection(int bit)
        {
            _directions = BitUtils.ClearBit(_directions, bit);
        }

        protected void PressButton(int bit)
        {
            _buttons = BitUtils.ClearBit(_buttons, bit);
        }

        protected void ReleaseDirection(int bit)
        {
            _directions = BitUtils.SetBit(_directions, bit);
        }

        protected void ReleaseButton(int bit)
        {
            _buttons = BitUtils.SetBit(_buttons, bit);
        }

        public byte Process(byte b)
        {

            if ((b & 0x30) == 0x10)
            {
                b |= _buttons;
                InterruptRequest = true;
            }
            else if ((b & 0x30) == 0x20)
            {
                b |= _directions;
                InterruptRequest = true;
            }
            else
            {
                b |= 0xf;
            }

            return b;
        }

        public bool HandleInput()
        {
            while (SDL.SDL_PollEvent(out var sdlEvent) != 0)
            {
                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    return false;
                }

                ProcessInput(sdlEvent);
            }

            return true;
        }

        public void Dispose()
        {
            if (_gamecontroller == IntPtr.Zero) return;
            
            SDL.SDL_JoystickClose(_gamecontroller);
            _gamecontroller = IntPtr.Zero;
        }
    }
}
