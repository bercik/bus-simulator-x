using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Testy_mapy
{
    static class InputLogic
    {
        public static ToggleButton pauseButton = new ToggleButton(Keys.P);

        public static ToggleButton debugButton = new ToggleButton(Keys.F1, true);

        public static NormalButton brakeButton = new NormalButton(Keys.Down);
        public static NormalButton accelerateButton = new NormalButton(Keys.Up);
        public static NormalButton rightTurnButton = new NormalButton(Keys.Right);
        public static NormalButton leftTurnButton = new NormalButton(Keys.Left);
        public static OneTimePressButton doorsButton = new OneTimePressButton(Keys.D);
        public static OneTimePressButton lightsButton = new OneTimePressButton(Keys.L);
        public static OneTimePressButton gearUpButton = new OneTimePressButton(Keys.A);
        public static OneTimePressButton gearDownButton = new OneTimePressButton(Keys.Z);

        public static void Update(KeyboardState keybState, MouseState mouseState)
        {
            // Update pause button.
            pauseButton.Update(keybState);

            // Update debug button.
            debugButton.Update(keybState);
            
            // Update bus related buttons.
            brakeButton.Update(keybState);
            accelerateButton.Update(keybState);
            rightTurnButton.Update(keybState);
            leftTurnButton.Update(keybState);
            doorsButton.Update(keybState);
            lightsButton.Update(keybState);
            gearUpButton.Update(keybState);
            gearDownButton.Update(keybState);            

            // Update mouse.
            Mouse.Update(mouseState);

            // Obsługa skali mapy.
            if (keybState.IsKeyDown(Keys.PageUp))
                Helper.SetScale(Helper.GetScale() + 0.01f);

            if (keybState.IsKeyDown(Keys.PageDown))
                Helper.SetScale(Helper.GetScale() - 0.01f);

            if (keybState.IsKeyDown(Keys.Delete))
                Helper.SetScale(1.0f);            
        }

        /// <summary>
        /// Detect the mouse clicks and other mouse parameters.
        /// </summary>
        public static class Mouse
        {
            public static bool leftButtonClicked = false;  // This indicates if the LMB has been clicked.
            public static bool rightButtonClicked = false; // This indicates if the RMB has been clicked.
            public static Vector2 position = Vector2.Zero; // Current mouse position.
            public static int wheelValue;  // Current value of the mouse wheel.
            public static int wheelChange; // Mouse wheel value delta since the last update.

            private static bool prevLeft = false;
            private static bool prevRight = false;

            private static int lastWheelValue;

            public static void Update(MouseState mouseState)
            {
                // POSITION.
                position.X = mouseState.X;
                position.Y = mouseState.Y;

                // SCROLL.
                lastWheelValue = wheelValue;
                wheelValue = mouseState.ScrollWheelValue;
                wheelChange = wheelValue - lastWheelValue;

                // LMB.
                if (mouseState.LeftButton == ButtonState.Pressed && !prevLeft)
                {
                    leftButtonClicked = true;
                    prevLeft = true;
                }
                else
                {
                    leftButtonClicked = false;
                }

                if (mouseState.LeftButton == ButtonState.Released)
                    prevLeft = false;

                // RMB.
                if (mouseState.RightButton == ButtonState.Pressed && !prevRight)
                {
                    rightButtonClicked = true;
                    prevRight = true;
                }
                else
                {
                    rightButtonClicked = false;
                }

                if (mouseState.RightButton == ButtonState.Released)
                    prevRight = false;
            }
        }

        /// <summary>
        /// This class creates the button which TURNs THE STATE TO TRUE MULTIPLE TIMES.
        /// </summary>
        public class NormalButton
        {
            public bool state { private set; get; } // Current state.

            private Microsoft.Xna.Framework.Input.Keys button; // Monitored button.

            /// <summary>
            /// Constructor.
            /// </summary>
            public NormalButton(Microsoft.Xna.Framework.Input.Keys button)
            {
                this.button = button;
                state = false;
            }

            /// <summary>
            /// Turn the state to true.
            /// </summary>
            public void Enable()
            {
                state = true;
            }

            /// <summary>
            /// Turn the state to false.
            /// </summary>
            public void Disable()
            {
                state = false;
            }

            public void Update(KeyboardState keybState)
            {
                if (keybState.IsKeyDown(button)) // If the button is pressed turn the state to true.
                {
                    Enable();
                }
                else
                {
                    Disable();
                }
            }
        }

        /// <summary>
        /// This class is used to create button which CHANGES THE STATE ONLY ONCE every time it is pressed. eg. pause button.
        /// </summary>
        public class ToggleButton
        {
            public bool state { private set; get; } // Current state.

            private bool prev = false; // Previous state.            
            private Microsoft.Xna.Framework.Input.Keys button; // Monitored button.

            /// <summary>
            /// Constructor.
            /// </summary>
            public ToggleButton(Microsoft.Xna.Framework.Input.Keys button)
            {
                this.button = button;
                state = false;
            }

            /// <summary>
            /// Constructor with a starting state.
            /// </summary>
            public ToggleButton(Microsoft.Xna.Framework.Input.Keys button, bool state)
            {
                this.button = button;
                this.state = state;
            }

            /// <summary>
            /// Force the state change.
            /// </summary>
            public void Toggle()
            {
                state = !state;
            }

            /// <summary>
            /// Turn the state to true.
            /// </summary>
            public void Enable()
            {
                state = true;
            }

            /// <summary>
            /// Turn the state to false.
            /// </summary>
            public void Disable()
            {
                state = false;
            }

            public void Update(KeyboardState keybState)
            {
                if (keybState.IsKeyDown(button) && !prev) // If the button is pressed and was not pressed before change state and note that it has been pressed.
                {
                    Toggle();
                    prev = true;
                }

                if (keybState.IsKeyUp(button)) // Reset previous state.
                    prev = false;
            }
        }

        /// <summary>
        /// This class is used to create button which TURNS THE STATE TO TRUE ONLY ONCE each time it is pressed. eg. button increasing a value.
        /// </summary>
        public class OneTimePressButton
        {
            public bool state = false; // Current state.

            private bool prev = false; // Previous state.            
            private Microsoft.Xna.Framework.Input.Keys button; // Monitored button.

            /// <summary>
            /// Constructor.
            /// </summary>
            public OneTimePressButton(Microsoft.Xna.Framework.Input.Keys button)
            {
                this.button = button;
            }

            public void Update(KeyboardState keybState)
            {
                if (keybState.IsKeyDown(button) && !prev) // If the button is pressed and was not pressed before turn on thestate and note that it has been pressed.
                {
                    state = true;
                    prev = true;
                }
                else
                {
                    state = false;
                }

                if (keybState.IsKeyUp(button)) // Reset previous state.
                    prev = false;
            }
        }
    }
}
