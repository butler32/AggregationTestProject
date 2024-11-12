namespace AggregationTestProject.Constants
{
    /// <summary>
    /// CoreScanner Opcodes
    /// Please refer Scanner SDK for Windows Developer Guide for more information on opcodes.   
    /// </summary>
    public enum ZebraOpcode
    {
        /// <summary>
        /// Gets the version of CoreScanner
        /// </summary>
        GetVersion = 1000,

        /// <summary>
        /// Register for API events
        /// </summary>
        RegisterForEvents = 1001,

        /// <summary>
        /// Unregister for API events
        /// </summary>
        UnregisterForEvents = 1002,

        /// <summary>
        /// Get Bluetooth scanner pairing bar code
        /// </summary>
        GetPairingBarcode = 1005,

        /// <summary>
        /// Claim a specific device
        /// </summary>
        ClaimDevice = 1500,

        /// <summary>
        /// Release a specific device
        /// </summary>
        ReleaseDevice = 1501,

        /// <summary>
        /// Abort MacroPDF of a specified scanner
        /// </summary>
        AbortMacroPdf = 2000,

        /// <summary>
        /// Abort firmware update process of a specified scanner, while in progress
        /// </summary>
        AbortUpdateFirmware = 2001,

        /// <summary>
        /// Turn Aim off
        /// </summary>
        DeviceAimOff = 2002,

        /// <summary>
        /// Turn Aim on
        /// </summary>
        DeviceAimOn = 2003,

        /// <summary>
        /// Flush MacroPDF of a specified scanner
        /// </summary>
        FlushMacroPdf = 2005,

        /// <summary>
        /// Pull the trigger of a specified scanner
        /// </summary>
        DevicePullTrigger = 2011,

        /// <summary>
        /// Release the trigger of a specified scanner
        /// </summary>
        DeviceReleaseTrigger = 2012,

        /// <summary>
        /// Disable scanning on a specified scanner
        /// </summary>
        DeviceScanDisable = 2013,

        /// <summary>
        /// Enable scanning on a specified scanner
        /// </summary>
        DeviceScanEnable = 2014,

        /// <summary>
        /// Set parameters to default values of a specified scanner
        /// </summary>
        SetParameterDefaults = 2015,

        /// <summary>
        /// Set parameters of a specified scanner
        /// </summary>
        DeviceSetParameters = 2016,

        /// <summary>
        /// Set and persist parameters of a specified scanner
        /// </summary>
        SetParameterPersistance = 2017,

        /// <summary>
        /// Reboot a specified scanner
        /// </summary>
        RebootScanner = 2019,

        /// <summary>
        /// Disconnect the specified Bluetooth scanner
        /// </summary>
        DisconnectBluetoothScanner = 2023,

        /// <summary>
        /// Change a specified scanner to snapshot mode 
        /// </summary>
        DeviceCaptureImage = 3000,

        /// <summary>
        /// Change a specified scanner to decode mode 
        /// </summary>
        DeviceCaptureBarcode = 3500,

        /// <summary>
        /// Change a specified scanner to video mode 
        /// </summary>
        DeviceCaptureVideo = 4000,

        /// <summary>
        /// Get all the attributes of a specified scanner
        /// </summary>
        RsmAttrGetAll = 5000,

        /// <summary>
        /// Get the attribute values(s) of specified scanner
        /// </summary>
        RsmAttrGet = 5001,

        /// <summary>
        /// Get the next attribute to a given attribute of specified scanner
        /// </summary>
        RsmAttrGetNext = 5002,

        /// <summary>
        /// Set the attribute values(s) of specified scanner
        /// </summary>
        RsmAttrSet = 5004,

        /// <summary>
        /// Store and persist the attribute values(s) of specified scanner
        /// </summary>
        RsmAttrStore = 5005,

        /// <summary>
        /// Get the topology of the connected devices
        /// </summary>
        GetDeviceTopology = 5006,

        /// <summary>
        /// Remove all Symbol device entries from registry
        /// </summary>
        UninstallSymbolDevices = 5010,

        /// <summary>
        /// Start (flashing) the updated firmware
        /// </summary>
        StartNewFirmware = 5014,

        /// <summary>
        /// Update the firmware to a specified scanner
        /// </summary>
        UpdateFirmware = 5016,

        /// <summary>
        /// Update the firmware to a specified scanner using a scanner plug-in
        /// </summary>
        UpdateFirmwareFromPlugin = 5017,

        /// <summary>
        /// Update good scan tone of the scanner with specified wav file
        /// </summary>
        UpdateDecodeTone = 5050,

        /// <summary>
        /// Erase good scan tone of the scanner
        /// </summary>
        EraseDecodeTone = 5051,

        /// <summary>
        /// Perform an action involving scanner beeper/LEDs
        /// </summary>
        SetAction = 6000,

        /// <summary>
        /// Set the serial port settings of a NIXDORF Mode-B scanner
        /// </summary>
        DeviceSetSerialPortSettings = 6101,

        /// <summary>
        /// Switch the USB host mode of a specified scanner
        /// </summary>
        DeviceSwitchHostMode = 6200,

        /// <summary>
        /// Switch CDC devices
        /// </summary>
        SwitchCdcDevices = 6201,

        /// <summary>
        /// Enable/Disable keyboard emulation mode
        /// </summary>
        KeyboardEmulatorEnable = 6300,

        /// <summary>
        /// Set the locale for keyboard emulation mode
        /// </summary>
        KeyboardEmulatorSetLocale = 6301,

        /// <summary>
        /// Get current configuration of the HID keyboard emulator
        /// </summary>
        KeyboardEmulatorGetConfig = 6302,

        /// <summary>
        ///  Configure Driver ADF
        /// </summary>
        ConfigureDADF = 6400,

        /// <summary>
        /// Reset Driver ADF
        /// </summary>
        ResetDADF = 6401,

        /// <summary>
        /// Measure the weight on the scanner's platter and get the value
        /// </summary>
        ScaleReadWeight = 7000,

        /// <summary>
        ///  Zero the scale
        /// </summary>
        ScaleZeroScale = 7002,

        /// <summary>
        /// Reset the scale
        /// </summary>
        ScaleSystemReset = 7015,
    }
}
