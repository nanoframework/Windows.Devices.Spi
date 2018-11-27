//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Runtime.CompilerServices;

namespace Windows.Devices.Spi
{
    /// <summary>
    /// Represents a device connected through the SPI bus.
    /// </summary>
    public sealed class SpiDevice : IDisposable
    {
        // generate a unique ID for the device by joining the SPI bus ID and the chip select line, should be pretty unique
        // the encoding is (SPI bus number x 1000 + chip select line number)
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private const int deviceUniqueIdMultiplier = 1000;

        // this is used as the lock object 
        // a lock is required because multiple threads can access the device
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly object _syncLock = new object();

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly string _spiBus;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly int _deviceId;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly Spi​Connection​Settings _connectionSettings;

        internal SpiDevice(string spiBus, Spi​Connection​Settings settings)
        {
            // generate a unique ID for the device by joining the SPI bus ID and the chip select line, should be pretty unique
            // the encoding is (SPI bus number x 1000 + chip select line number)
            // spiBus is an ASCII string with the bus name in format 'SPIn'
            // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
            var controllerId = spiBus[3] - '0';
            var deviceId = (controllerId * deviceUniqueIdMultiplier) + settings.ChipSelectLine;

            SpiController controller;

            if (!SpiControllerManager.ControllersCollection.Contains(controllerId))
            {
                // this controller doesn't exist yet, create it...
                controller = new SpiController(spiBus);
            }
            else
            {
                // get the controller from the collection...
                controller = (SpiController)SpiControllerManager.ControllersCollection[controllerId];
            }

            // check if this device ID already exists
            if (!controller.DeviceCollection.Contains(deviceId))
            {
                // device doesn't exist, create it...
                _connectionSettings = new SpiConnectionSettings(settings);

                // save device ID
                _deviceId = deviceId;

                // call native init to allow HAL/PAL inits related with Spi hardware
                NativeInit();

                // ... and add this device
                controller.DeviceCollection.Add(deviceId, this);
            }
            else
            {
                // this device already exists, throw an exception
                throw new SpiDeviceAlreadyInUseException();
            }
        }

        /// <summary>
        /// Gets the connection settings for the device.
        /// </summary>
        /// <value>
        /// The connection settings.
        /// </value>
        public Spi​Connection​Settings ConnectionSettings
        {
            get
            {
                lock (_syncLock)
                {
                    // check if device has been disposed
                    if (!_disposedValue)
                    {
                        // need to return a copy so that the caller doesn't change the settings
                        return new Spi​Connection​Settings(_connectionSettings);
                    }

                    throw new ObjectDisposedException();
                }
            }
        }

        /// <summary>
        /// Gets the unique ID associated with the device.
        /// </summary>
        /// <value>
        /// The ID.
        /// </value>
        public string DeviceId
        {
            get
            {
                lock (_syncLock)
                {
                    // check if device has been disposed
                    if (!_disposedValue) { return _deviceId.ToString(); }

                    throw new ObjectDisposedException();
                }
            }
        }

        /// <summary>
        /// Opens a device with the connection settings provided.
        /// </summary>
        /// <param name="busId">The id of the bus.</param>
        /// <param name="settings">The connection settings.</param>
        /// <returns>The SPI device requested.</returns>
        /// <remarks>This method is specific to nanoFramework. The equivalent method in the UWP API is: FromIdAsync.</remarks>
        public static SpiDevice FromId(string busId, Spi​Connection​Settings settings)
        {
            //TODO: some sanity checks on busId
            return new SpiDevice(busId, settings);
        }

        /// <summary>
        /// Retrieves the info about a certain bus.
        /// </summary>
        /// <param name="busId">The id of the bus.</param>
        /// <returns>The bus info requested.</returns>
        public static SpiBusInfo GetBusInfo(string busId)
        {
            return new SpiBusInfo(busId);
        }

        /// <summary>
        /// Gets all the SPI buses found on the system that match the input parameter.
        /// </summary>
        /// <param name="friendlyName">Input parameter specifying an identifying name for the desired bus. This usually corresponds to a name on the schematic.</param>
        /// <returns>String containing all the buses that have the input in the name.</returns>
        public static string GetDeviceSelector(string friendlyName)
        {
            // At the moment, ignore the friendly name.
            return GetDeviceSelector();
        }

        /// <summary>
        /// Reads from the connected device.
        /// </summary>
        /// <param name="buffer">Array containing data read from the device.</param>
        public void Read(byte[] buffer)
        {
            NativeTransfer(null, buffer, false);
        }

        /// <summary>
        /// Reads from the connected device.
        /// </summary>
        /// <param name="buffer">Array containing data read from the device.</param>
        public void Read(ushort[] buffer)
        {
            NativeTransfer(null, buffer, false);
        }

        /// <summary>
        /// Transfer data using a full duplex communication system. Full duplex allows both the master and the slave to communicate simultaneously.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer)
        {
            NativeTransfer(writeBuffer, readBuffer, true);
        }

        /// <summary>
        /// Transfer data using a full duplex communication system. Full duplex allows both the master and the slave to communicate simultaneously.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferFullDuplex(ushort[] writeBuffer, ushort[] readBuffer)
        {
            NativeTransfer(writeBuffer, readBuffer, true);
        }

        /// <summary>
        /// Transfer data sequentially to the device.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer)
        {
            NativeTransfer(writeBuffer, readBuffer, false);
        }

        /// <summary>
        /// Transfer data sequentially to the device.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferSequential(ushort[] writeBuffer, ushort[] readBuffer)
        {
            NativeTransfer(writeBuffer, readBuffer, false);
        }

        /// <summary>
        /// Writes to the connected device.
        /// </summary>
        /// <param name="buffer">Array containing the data to write to the device.</param>
        public void Write(byte[] buffer)
        {
            NativeTransfer(buffer, null, false);
        }

        /// <summary>
        /// Writes to the connected device.
        /// </summary>
        /// <param name="buffer">Array containing the data to write to the device.</param>
        public void Write(ushort[] buffer)
        {
            NativeTransfer(buffer, null, false);
        }

        /// <summary>
        /// Gets all the SPI buses found on the system.
        /// </summary>
        /// <returns>String containing all the buses found on the system.</returns>
        public static string GetDeviceSelector()
        {
            return SpiController.GetDeviceSelector();
        }

        #region IDisposable Support

        private bool _disposedValue;

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // get the controller Id
                    // it's enough to divide by the device unique id multiplier as we'll get the thousands digit, which is the controller ID
                    var controller = (SpiController)SpiControllerManager.ControllersCollection[_deviceId / deviceUniqueIdMultiplier];

                    // remove from device collection
                    controller.DeviceCollection.Remove(_deviceId);

                    // it's OK to also remove the controller, if there is no other device associated
                    if (controller.DeviceCollection.Count == 0)
                    {
                        SpiControllerManager.ControllersCollection.Remove(controller);

                        controller = null;
                    }
                }

                DisposeNative();

                _disposedValue = true;
            }
        }

        #pragma warning disable 1591
        ~SpiDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            lock (_syncLock)
            {
                if (!_disposedValue)
                {
                    Dispose(true);

                    GC.SuppressFinalize(this);
                }
            }
        }

        #pragma warning restore 1591

        #endregion

        #region Native Calls

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void DisposeNative();

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeTransfer(byte[] writeBuffer, byte[] readBuffer, bool fullDuplex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeTransfer(ushort[] writeBuffer, ushort[] readBuffer, bool fullDuplex);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern void NativeInit();

        #endregion
    }

    /// <summary>
    /// Exception thrown when a check in driver's constructor finds a device that already exists with the same settings (SPI bus AND chip select line)
    /// </summary>
    [Serializable]
    public class SpiDeviceAlreadyInUseException : Exception
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() { return base.Message; }
    }
}
