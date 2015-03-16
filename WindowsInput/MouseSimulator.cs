using System;
using System.Text;
using System.Drawing;
using WindowsInput.Native;

namespace WindowsInput
{
    /// <summary>
    /// Implements the <see cref="IMouseSimulator"/> interface by calling the an <see cref="IInputMessageDispatcher"/> to simulate Mouse gestures.
    /// </summary>
    public class MouseSimulator : IMouseSimulator
    {
        private const int MouseWheelClickSize = 120;

        /// <summary>
        /// The instance of the <see cref="IInputMessageDispatcher"/> to use for dispatching <see cref="INPUT"/> messages.
        /// </summary>
        private readonly IInputMessageDispatcher _messageDispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSimulator"/> class using the specified <see cref="IInputMessageDispatcher"/> for dispatching <see cref="INPUT"/> messages.
        /// </summary>
        /// <param name="messageDispatcher">The <see cref="IInputMessageDispatcher"/> to use for dispatching <see cref="INPUT"/> messages.</param>
        /// <exception cref="InvalidOperationException">If null is passed as the <paramref name="messageDispatcher"/>.</exception>
        public MouseSimulator(IInputMessageDispatcher messageDispatcher)
        {
            if (messageDispatcher == null)
                throw new InvalidOperationException(
                    string.Format("The {0} cannot operate with a null {1}. Please provide a valid {1} instance to use for dispatching {2} messages.",
                    typeof(MouseSimulator).Name, typeof(IInputMessageDispatcher).Name, typeof(INPUT).Name));
            
            _messageDispatcher = messageDispatcher;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseSimulator"/> class using an instance of a <see cref="WindowsInputMessageDispatcher"/> for dispatching <see cref="INPUT"/> messages.
        /// </summary>
        public MouseSimulator()
        {
            _messageDispatcher = new WindowsInputMessageDispatcher();
        }

        /// <summary>
        /// Sends the list of <see cref="INPUT"/> messages using the <see cref="IInputMessageDispatcher"/> instance.
        /// </summary>
        /// <param name="inputList">The <see cref="System.Array"/> of <see cref="INPUT"/> messages to send.</param>
        /// <returns>The number of successful messages that were sent.</returns>
        private int SendSimulatedInput(INPUT[] inputList)
        {
            if (inputList == null || inputList.Length == 0) return -1;
            return (int)_messageDispatcher.DispatchInput(inputList);
        }

        /// <summary>
        /// Sends the supplied commands as scripted mouse input.
        /// </summary>
        /// <param name="input">The input to send.</param>
        public void SendMouse(string input)
        {
            input = input.ToUpper();
            StringBuilder location = new StringBuilder();

            string currentState = "MOUSE_UP";
            char currentButton = ' ';
            bool clickAfter = false;
            bool dragging = false;
            bool haveMoved = true;

            foreach (char element in input.ToCharArray())
            {
                switch (currentState)
                {
                    case "PARSING_LOCATION":
                        {
                            if (element == '}')
                            {
                                PointConverter pointConverter = new PointConverter();

                                if (String.IsNullOrEmpty(location.ToString()))
                                {
                                    throw new InvalidOperationException("Please specify a point.");
                                }

                                Point newLocation = (Point)pointConverter.ConvertFromString(location.ToString());

                                if (newLocation == null)
                                {
                                    throw new InvalidOperationException("Invalid syntax for a point.");
                                }

                                this.MoveMouseTo(newLocation.X, newLocation.Y);
                                location = new StringBuilder();

                                if (dragging)
                                {
                                    if (!haveMoved)
                                    {
                                        switch (currentButton)
                                        {
                                            case 'L': this.LeftButtonDown(); break;
                                            case 'R': this.RightButtonDown(); break;
                                        }
                                        haveMoved = true;
                                    }

                                    currentState = "MOUSE_DRAGGING";
                                    dragging = false;
                                }
                                else
                                {
                                    currentState = "MOUSE_UP";
                                }

                                if (clickAfter)
                                {
                                    switch (currentButton)
                                    {
                                        case 'L': this.LeftButtonClick(); break;
                                        case 'R': this.RightButtonClick(); break;
                                    }

                                    clickAfter = false;
                                }
                            }
                            else
                            {
                                location.Append(element);
                            }
                        }; break;
                    case "MOUSE_UP":
                        {
                            // See if it's a button
                            if ((element == 'L') || (element == 'R'))
                            {
                                currentState = "MOUSE_DOWN";
                                currentButton = element;
                            }
                            // See if it's a location
                            else if (element == '{')
                            {
                                currentState = "PARSING_LOCATION";
                            }
                            // See if it's a drag start
                            else if (element == '(')
                            {
                                throw new InvalidOperationException("A location list must be preceded by a mouse button");
                            }
                            // See if it's a drag end
                            else if (element == ')')
                            {
                                throw new InvalidOperationException("Error attempting to close a location list not yet opened.");
                            }
                        }; break;
                    case "MOUSE_DOWN":
                        {
                            // See if it's a button
                            if ((element == 'L') || (element == 'R'))
                            {
                                switch (currentButton)
                                {
                                    case 'L': this.LeftButtonClick(); break;
                                    case 'R': this.RightButtonClick(); break;
                                }
                                currentButton = element;
                            }
                            // See if it's a location
                            else if (element == '{')
                            {
                                clickAfter = true;
                                currentState = "PARSING_LOCATION";
                            }
                            // See if it's a drag start
                            else if (element == '(')
                            {
                                currentState = "MOUSE_DRAGGING";
                            }
                            // See if it's a drag end
                            else if (element == ')')
                            {
                                throw new InvalidOperationException("Error attempting to close a location list not yet opened.");
                            }
                        }; break;
                    case "MOUSE_DRAGGING":
                        {
                            // See if it's a button
                            if ((element == 'L') || (element == 'R'))
                            {
                                throw new InvalidOperationException("Cannot click a new button while dragging.");
                            }
                            // See if it's a location
                            else if (element == '{')
                            {
                                dragging = true;
                                currentState = "PARSING_LOCATION";
                                haveMoved = false;
                            }
                            // See if it's a drag start
                            else if (element == '(')
                            {
                                throw new InvalidOperationException("A mouse drag list is already started.");
                            }
                            // See if it's a drag end
                            else if (element == ')')
                            {
                                switch (currentButton)
                                {
                                    case 'L': this.LeftButtonUp(); break;
                                    case 'R': this.RightButtonUp(); break;
                                }
                                currentState = "MOUSE_UP";
                            }
                        }; break;
                }
            }

            if (currentState == "PARSING_LOCATION")
            {
                throw new InvalidOperationException("Location did not contain a closing curly brace.");
            }
            else if (currentState == "MOUSE_DOWN")
            {
                switch (currentButton)
                {
                    case 'L': this.LeftButtonClick(); break;
                    case 'R': this.RightButtonClick(); break;
                }
            }
            else if (currentState == "MOUSE_DRAGGING")
            {
                throw new InvalidOperationException("Mouse drag location list was not closed.");
            }
        }

        /// <summary>
        /// Simulates mouse movement by the specified distance measured as a delta from the current mouse location in pixels.
        /// </summary>
        /// <param name="pixelDeltaX">The distance in pixels to move the mouse horizontally.</param>
        /// <param name="pixelDeltaY">The distance in pixels to move the mouse vertically.</param>
        public void MoveMouseBy(int pixelDeltaX, int pixelDeltaY)
        {
            var inputList = new InputBuilder().AddRelativeMouseMovement(pixelDeltaX, pixelDeltaY).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates mouse movement to the specified location on the primary display device.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the primary display device where 0 is the extreme left hand side of the display device.</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the primary display device where 0 is the top of the display device.</param>
        public void MoveMouseTo(double absoluteX, double absoluteY)
        {
            System.Drawing.Size primaryMonitorSize = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            double newX = Math.Ceiling(absoluteX * 65536 / primaryMonitorSize.Width);
            double newY = Math.Ceiling(absoluteY * 65536 / primaryMonitorSize.Height);

            var inputList = new InputBuilder().AddAbsoluteMouseMovement((int)Math.Truncate(newX), (int)Math.Truncate(newY)).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates mouse movement to the specified location on the Virtual Desktop which includes all active displays.
        /// </summary>
        /// <param name="absoluteX">The destination's absolute X-coordinate on the virtual desktop where 0 is the left hand side of the virtual desktop and 65535 is the extreme right hand side of the virtual desktop.</param>
        /// <param name="absoluteY">The destination's absolute Y-coordinate on the virtual desktop where 0 is the top of the virtual desktop and 65535 is the bottom of the virtual desktop.</param>
        public void MoveMouseToPositionOnVirtualDesktop(double absoluteX, double absoluteY)
        {
            var inputList = new InputBuilder().AddAbsoluteMouseMovementOnVirtualDesktop((int)Math.Truncate(absoluteX), (int)Math.Truncate(absoluteY)).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse left button down gesture.
        /// </summary>
        public void LeftButtonDown()
        {
            var inputList = new InputBuilder().AddMouseButtonDown(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse left button up gesture.
        /// </summary>
        public void LeftButtonUp()
        {
            var inputList = new InputBuilder().AddMouseButtonUp(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse left-click gesture.
        /// </summary>
        public void LeftButtonClick()
        {
            var inputList = new InputBuilder().AddMouseButtonClick(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse left button double-click gesture.
        /// </summary>
        public void LeftButtonDoubleClick()
        {
            var inputList = new InputBuilder().AddMouseButtonDoubleClick(MouseButton.LeftButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse right button down gesture.
        /// </summary>
        public void RightButtonDown()
        {
            var inputList = new InputBuilder().AddMouseButtonDown(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse right button up gesture.
        /// </summary>
        public void RightButtonUp()
        {
            var inputList = new InputBuilder().AddMouseButtonUp(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse right button click gesture.
        /// </summary>
        public void RightButtonClick()
        {
            var inputList = new InputBuilder().AddMouseButtonClick(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse right button double-click gesture.
        /// </summary>
        public void RightButtonDoubleClick()
        {
            var inputList = new InputBuilder().AddMouseButtonDoubleClick(MouseButton.RightButton).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse X button down gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public void XButtonDown(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonDown(buttonId).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse X button up gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public void XButtonUp(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonUp(buttonId).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse X button click gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public void XButtonClick(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonClick(buttonId).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse X button double-click gesture.
        /// </summary>
        /// <param name="buttonId">The button id.</param>
        public void XButtonDoubleClick(int buttonId)
        {
            var inputList = new InputBuilder().AddMouseXButtonDoubleClick(buttonId).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates mouse vertical wheel scroll gesture.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated forward, away from the user; a negative value indicates that the wheel was rotated backward, toward the user.</param>
        public void VerticalScroll(int scrollAmountInClicks)
        {
            var inputList = new InputBuilder().AddMouseVerticalWheelScroll(scrollAmountInClicks * MouseWheelClickSize).ToArray();
            SendSimulatedInput(inputList);
        }

        /// <summary>
        /// Simulates a mouse horizontal wheel scroll gesture. Supported by Windows Vista and later.
        /// </summary>
        /// <param name="scrollAmountInClicks">The amount to scroll in clicks. A positive value indicates that the wheel was rotated to the right; a negative value indicates that the wheel was rotated to the left.</param>
        public void HorizontalScroll(int scrollAmountInClicks)
        {
            var inputList = new InputBuilder().AddMouseHorizontalWheelScroll(scrollAmountInClicks * MouseWheelClickSize).ToArray();
            SendSimulatedInput(inputList);
        }
    }
}