//
// Copyright (c) .NET Foundation and Contributors
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

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly int _deviceId;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly Spi​Connection​Settings _connectionSettings;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private readonly SpiController _spiController;

        // this is used as the lock object 
        // a lock is required because multiple threads can access the device (Dispose)
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object _syncLock;

        internal SpiDevice(string spiBus, Spi​Connection​Settings settings)
        {
            // spiBus is an ASCII string with the bus name in format 'SPIn'
            // need to grab 'n' from the string and convert that to the integer value from the ASCII code (do this by subtracting 48 from the char value)
            var controllerId = spiBus[3] - '0';

            // Save reference to SpiController for _syncLock on controller
            // Each controller needs to restrict access from multiple threads
            // this will also work for same device from multiple threads
            _spiController = SpiController.FindController(controllerId);
            if (_spiController == null)
            {
                // this controller doesn't exist yet, create it...
                _spiController = new SpiController(spiBus);
            }

            try
            {
                _connectionSettings = new SpiConnectionSettings(settings);

                _deviceId = NativeOpenDevice();
            }
            catch(NotSupportedException )
            {
                // NotSupportedException 
                //   Device(chip select) already in use
                throw new SpiDeviceAlreadyInUseException();
            }
            catch(Exception ex)
            {
                // ArgumentException
                //   Invalid port or unable to init bus
                // IndexOutOfRangeException
                //   Too many devices open or spi already in use
                throw ex;
            }

            // device doesn't exist, create it...
            _connectionSettings = new SpiConnectionSettings(settings);

            _syncLock = new object();
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
                lock (_spiController._syncLock)
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
                lock (_spiController._syncLock)
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
		/// <exception cref="System.NotSupportedException">
		/// Thrown if the chip select pin is already in use</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// Thrown if the maximum number of devices on SPI bus is reached</exception>
		/// <exception cref="System.ArgumentException">
		/// Thrown if invalid SPI bus</exception>
		/// <exception cref="System.SystemException">
		/// Thrown if GPIO pin already in use.</exception>
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
            lock (_spiController._syncLock)
            {
                NativeTransfer(null, buffer, false);
            }
        }

        /// <summary>
        /// Reads from the connected device.
        /// </summary>
        /// <param name="buffer">Array containing data read from the device.</param>
        public void Read(ushort[] buffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(null, buffer, false);
            }
        }

        /// <summary>
        /// Transfer data using a full duplex communication system. Full duplex allows both the master and the slave to communicate simultaneously.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferFullDuplex(byte[] writeBuffer, byte[] readBuffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(writeBuffer, readBuffer, true);
            }
        }

        /// <summary>
        /// Transfer data using a full duplex communication system. Full duplex allows both the master and the slave to communicate simultaneously.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferFullDuplex(ushort[] writeBuffer, ushort[] readBuffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(writeBuffer, readBuffer, true);
            }
        }

        /// <summary>
        /// Transfer data sequentially to the device.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferSequential(byte[] writeBuffer, byte[] readBuffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(writeBuffer, readBuffer, false);
            }
        }

        /// <summary>
        /// Transfer data sequentially to the device.
        /// </summary>
        /// <param name="writeBuffer">Array containing data to write to the device.</param>
        /// <param name="readBuffer">Array containing data read from the device.</param>
        public void TransferSequential(ushort[] writeBuffer, ushort[] readBuffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(writeBuffer, readBuffer, false);
            }
        }

        /// <summary>
        /// Writes to the connected device.
        /// </summary>
        /// <param name="buffer">Array containing the data to write to the device.</param>
        public void Write(byte[] buffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(buffer, null, false);
            }
        }

        /// <summary>
        /// Writes to the connected device.
        /// </summary>
        /// <param name="buffer">Array containing the data to write to the device.</param>
        public void Write(ushort[] buffer)
        {
            lock (_spiController._syncLock)
            {
                NativeTransfer(buffer, null, false);
            }
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

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern Int32 NativeOpenDevice();

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
