//
// Copyright (c) 2017 The nanoFramework project contributors
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
        // a lock is required because multiple threads can access the SpiController
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly object _syncLock = new object();

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly int _controllerId;

        // backing field for DeviceCollection
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private Hashtable s_deviceCollection;

        /// <summary>
        /// Device collection associated with this <see cref="SpiController"/>.
        /// </summary>
        /// <remarks>
        /// This collection is for internal use only.
        /// </remarks>
        internal Hashtable DeviceCollection
        {
            get
            {
                if (s_deviceCollection == null)
                {
                    lock (_syncLock)
                    {
                        if (s_deviceCollection == null)
                        {
                            s_deviceCollection = new Hashtable();
                        }
                    }
                }

                return s_deviceCollection;
            }

            set
            {
                s_deviceCollection = value;
            }
        }

        internal SpiController(string controller)
        {
            // the SPI id is an ASCII string with the format 'SPIn'
            // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
            _controllerId = controller[3] - '0';

            // check if this controller is already opened
            if (!SpiControllerManager.ControllersCollection.Contains(_controllerId))
            {

                // add controller to collection, with the ID as key 
                // *** just the index number ***
                SpiControllerManager.ControllersCollection.Add(_controllerId, this);
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

                if (SpiControllerManager.ControllersCollection.Contains(controllerId))
                {
                    // controller is already open
                    return (SpiController)SpiControllerManager.ControllersCollection[controllerId];
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
        public SpiDevice GetDevice(Spi​Connection​Settings settings)
        {
            //TODO: fix return value. Should return an existing device (if any)
            return new SpiDevice(String.Empty, settings);
        }

        #region Native Calls

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern string GetDeviceSelector();

        #endregion
    }
}
