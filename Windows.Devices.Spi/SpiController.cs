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
        // a lock is required because multiple threads can access the SpiController
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object _syncLock;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly int _controllerId;

        // backing field for DeviceCollection
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private ArrayList s_deviceCollection;

        /// <summary>
        /// Device collection associated with this <see cref="SpiController"/>.
        /// </summary>
        /// <remarks>
        /// This collection is for internal use only.
        /// </remarks>
        internal ArrayList DeviceCollection
        {
            get
            {
                if (s_deviceCollection == null)
                {
                    if (_syncLock == null)
                    {
                        _syncLock = new object();
                    }

                    lock (_syncLock)
                    {
                        if (s_deviceCollection == null)
                        {
                            s_deviceCollection = new ArrayList();
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
            var myController = FindController(_controllerId);
            if (myController == null)
            {
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
        public SpiDevice GetDevice(Spi​Connection​Settings settings)
        {
            //TODO: fix return value. Should return an existing device (if any)
            return new SpiDevice(String.Empty, settings);
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
