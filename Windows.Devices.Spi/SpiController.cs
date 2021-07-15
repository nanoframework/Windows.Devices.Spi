//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Spi
{
    /// <summary>
    /// Represents the SPI controller on the system.
    /// </summary>
    public sealed class SpiController
    {
        // this is used as the lock object 
        // a lock is required because multiple threads can access the SpiController/SpiDevice at same time
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        internal object _syncLock;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly int _controllerId;

        internal SpiController(string controller)
        {
            // the SPI id is an ASCII string with the format 'SPIn'
            // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
            _controllerId = controller[3] - '0';

            // check if this controller is already opened
            var myController = FindController(_controllerId);
            if (myController == null)
            {
                _syncLock = new object();

                // add controller to collection 
                SpiControllerManager.ControllersCollection.Add(this);
            }
            else
            {
                // this controller already exists: throw an exception
                throw new ArgumentException();
            }
        }

        /// <summary>
        /// Gets the default SPI controller on the system.
        /// </summary>
        /// <returns>The default SPI controller on the system, or null if the system has no SPI controller.</returns>
        public static SpiController GetDefault()
        {
            string controllersAqs = GetDeviceSelector();
            string[] controllers = controllersAqs.Split(',');

            if (controllers.Length > 0)
            {
                // the SPI id is an ASCII string with the format 'SPIn'
                // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
                var controllerId = controllers[0][3] - '0';

                var myController = FindController(controllerId);
                if (myController != null)
                {
                    // controller is already open
                    return myController;
                }
                else
                {
                    // this controller is not in the collection, create it
                    return new SpiController(controllers[0]);
                }
            }

            // the system has no SPI controller 
            return null;
        }

        /// <summary>
        /// Gets the SPI device with the specified settings.
        /// </summary>
        /// <param name="settings">The desired connection settings.</param>
        /// <returns>The SPI device.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the chip select pin is already in use</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if the maximum number of devices on SPI bus is reached</exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if invalid SPI bus</exception>
        /// <exception cref="System.SystemException">
        /// Thrown if GPIO pin already in use.</exception>
        public SpiDevice GetDevice(SpiConnectionSettings settings)
        {
            //TODO: fix return value. Should return an existing device (if any), not really documented what it does
            // Although examples seen to just open device against current controller
            return new SpiDevice($"SPI{_controllerId}", settings);
        }
        internal static SpiController FindController(int index)
        {
            for (int i = 0; i < SpiControllerManager.ControllersCollection.Count; i++)
            {
                if (((SpiController)SpiControllerManager.ControllersCollection[i])._controllerId == index)
                {
                    return (SpiController)SpiControllerManager.ControllersCollection[i];
                }
            }

            return null;
        }

        #region Native Calls

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string GetDeviceSelector();

        #endregion
    }
}
