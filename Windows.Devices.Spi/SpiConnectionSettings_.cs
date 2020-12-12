//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

namespace Windows.Devices.Spi
{
    /// <summary>
    /// Represents the settings for the connection with a device.
    /// </summary>
    public sealed class Spi​Connection​Settings
    {
        private int _csLine;
        /// <summary>
        /// Default clock frequency of 1mhz if not specified
        /// </summary>
        private int _clockFrequency = 1 * 1000 * 1000;
        /// <summary>
        /// Default data bit length should be 8. Typically it would be either 8 or 16
        /// </summary>
        private int _databitLength = 8; 
        /// <summary>
        /// Default SPI mode is Mode0 which is supported for the majority of devices
        /// </summary>
        private SpiMode _spiMode = SpiMode.Mode0;
        private SpiSharingMode _spiSharingMode;
        /// <summary>
        /// Default bit order as MSB, which is typical for most devices
        /// </summary>
        private DataBitOrder _bitOrder = DataBitOrder.MSB;

        /// <summary>
        /// Initializes new instance of SpiConnectionSettings.
        /// </summary>
        /// <param name="chipSelectLine">The chip select line on which the connection will be made.</param>
        public Spi​Connection​Settings(int chipSelectLine)
        {
            _csLine = chipSelectLine;
        }

        /// <summary>
        /// Initializes a copy of a <see cref="SpiConnectionSettings"/> object.
        /// </summary>
        /// <param name="value">Object to copy from.</param>
        internal Spi​Connection​Settings(Spi​Connection​Settings value)
        {
            _csLine = value._csLine;
            _clockFrequency = value._clockFrequency;
            _databitLength = value._databitLength;
            _spiMode = value._spiMode;
            _spiSharingMode = value._spiSharingMode;
            _bitOrder = value._bitOrder;
        }

        /// <summary>
        /// Gets the chip select line for the connection to the SPI device.
        /// </summary>
        /// <value>
        /// The chip select line.
        /// </value>
        public int ChipSelectLine
        {
            get { return _csLine; }
        }

        /// <summary>
        /// Gets or sets the clock frequency for the connection.
        /// </summary>
        /// <value>
        /// Value of the clock frequency in Hz.
        /// </value>
        public int ClockFrequency
        {
            get { return _clockFrequency; }
            set { _clockFrequency = value; }
        }

        /// <summary>
        /// Gets or sets the bit length for data on this connection.
        /// </summary>
        /// <value>
        /// The data bit length.
        /// </value>
        public int DataBitLength
        {
            get { return _databitLength; }
            set { _databitLength = value; }
        }

        /// <summary>
        /// Gets or sets the SpiMode for this connection.
        /// </summary>
        /// <value>
        /// The communication mode.
        /// </value>
        public SpiMode Mode
        {
            get { return _spiMode; }
            set { _spiMode = value; }
        }

        /// <summary>
        /// Gets or sets the sharing mode for the SPI connection.
        /// </summary>
        /// <value>
        /// The sharing mode.
        /// </value>
        public SpiSharingMode SharingMode
        {
            get { return _spiSharingMode; }
            set { _spiSharingMode = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataBitOrder"/> for the data in the buffers.
        /// This setting is only used when the <see cref="DataBitLength"/> is set to 16.
        /// </summary>
        /// <value>
        /// The bit order mode.
        /// </value>
        /// <remarks>This field is specific to nanoFramework. Doesn't have correspondence in the UWP API.</remarks>
        public DataBitOrder BitOrder
        {
            get { return _bitOrder; }
            set { _bitOrder = value; }
        }
    }
}
