//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System.Collections;

namespace Windows.Devices.Spi
{
    /// <summary>
    /// Represents the SPI controller on the system.
    /// </summary>
    public sealed class SpiController
    {
        // this is used as the lock object 
        // a lock is required because multiple threads can access the I2C controller
        readonly static object _syncLock = new object();

        // we can have only one instance of the SpiController
        // need to do a lazy initialization of this field to make sure it exists when called elsewhere.
        private static SpiController s_instance;

        // backing field for DeviceCollection
        private static Hashtable s_deviceCollection;

        /// <summary>
        /// Device collection associated with this <see cref="SpiController"/>.
        /// </summary>
        /// <remarks>
        /// This collection is for internal use only.
        /// </remarks>
        internal static Hashtable DeviceCollection
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

        /// <summary>
        /// Gets the default SPI controller on the system.
        /// </summary>
        /// <returns>The default SPI controller on the system, or null if the system has no SPI controller.</returns>
        public static SpiController GetDefault()
        {
            if (s_instance == null)
            {
                lock (_syncLock)
                {
                    if (s_instance == null)
                    {
                        s_instance = new SpiController();
                    }
                }
            }

            return s_instance;
        }

        /// <summary>
        /// Gets the SPI device with the specified settings.
        /// </summary>
        /// <param name="settings">The desired connection settings.</param>
        /// <returns>The SPI device.</returns>
        public SpiDevice GetDevice(Spi​Connection​Settings settings)
        {
            //TODO: fix return value. Should return an existing device (if any)
            return new SpiDevice(string.Empty, settings);
        }
    }
}
