using System;
//using System.Text;
//using System.Collections.Generic;
using System.IO;
using G711Audio;
using Microsoft.DirectX.DirectSound;
using System.Threading;


       class DirectSoundHelper
        {

            protected static CaptureBufferDescription _capBufDescr;
            protected static AutoResetEvent _eventToReset = null;
            protected static Notify _not = null;
            protected static WaveFormat _waveForm = new WaveFormat();
            protected static Capture _capture = null;
            protected static Device _device = null;
            protected static CaptureBuffer _captureBuffer;
            protected static SecondaryBuffer _playbackBuffer;
            protected static BufferDescription _playbackBufferDescription = null;
            protected static int _bufferSize = 0;
            protected static byte[] _byteData = new byte[1024];
            public bool _StopLoop = true;

            public event EventHandler OnBufferFulfill;


            public DirectSoundHelper()
            {
                SetVoiceDevices();
            }

            public void SetVoiceDevices()
            {

                SetVoiceDevices(
                0, // Номер устройства (Первое устройство) 
                1, // каналы      (2 если стерео) 
                16, // Битов на образец
                22050); // Образцов на секунду   

            }

            public void SetVoiceDevices(int deviceID, short channels, short bitsPerSample, int samplesPerSecond)
            {
                // Установка голосовых устройств
                _device = new Device(); // Звуковое устройство ввода
                _device.SetCooperativeLevel(new System.Windows.Forms.Control(), CooperativeLevel.Normal); // Установить форму приложения и приоритет
                CaptureDevicesCollection captureDeviceCollection = new CaptureDevicesCollection(); // Получить доступ к устройству (звуковая карта) 
                DeviceInformation deviceInfo = captureDeviceCollection[deviceID]; // установить номер устройства
                _capture = new Capture(deviceInfo.DriverGuid); //Получить информация о драйвере выбранного устройства

                //Настроить wave формат для захвата.
                _waveForm = new WaveFormat(); // Объявление формата 
                _waveForm.Channels = channels; // Каналы
                _waveForm.FormatTag = WaveFormatTag.Pcm; // PCM - Pulse Code Modulation
                _waveForm.SamplesPerSecond = samplesPerSecond; // установить число образцов в секунду
                _waveForm.BitsPerSample = bitsPerSample; // Установить колличество бит на образец
                _waveForm.BlockAlign = (short)(channels * (bitsPerSample / (short)8)); // данных в одном байте,  1 * (16/8) = 2 bits 
                _waveForm.AverageBytesPerSecond = _waveForm.BlockAlign * samplesPerSecond; // байт в секунду  22050*2= 44100
                _capBufDescr = new CaptureBufferDescription();
                _capBufDescr.BufferBytes = _waveForm.AverageBytesPerSecond / 5; // 200 милисекунд для записи = 8820 бит
                _capBufDescr.Format = _waveForm; // Using Wave Format

                // воспроизведение
                _playbackBufferDescription = new BufferDescription();
                _playbackBufferDescription.BufferBytes = _waveForm.AverageBytesPerSecond / 5;  // воспроизведение - 200 милисекунд = 8820 Bит
                _playbackBufferDescription.Format = _waveForm;
                _playbackBuffer = new SecondaryBuffer(_playbackBufferDescription, _device);
                _bufferSize = _capBufDescr.BufferBytes;
            }

            protected void SetBufferEvents()
            {
                // Отправлять данный во время записи
                // установить размер буфера 200 милисекунд и разделить его на два, 
                // при заполнение первой части её мож использовать
                // вторая половина будет заполняться данными

                try
                {
                    _eventToReset = new AutoResetEvent(false); // Ожидание уведомлений
                    _not = new Notify(_captureBuffer); // Количество байтов, которые могут инициировать событие уведомления

                    // первая часть
                    BufferPositionNotify bufferPositionNotify1 = new BufferPositionNotify(); // Для описания позиции уведомления
                    bufferPositionNotify1.Offset = _bufferSize / 2 - 1; //  половине буфера, чтобы узнать, где будет происходить событие уведомления
                    bufferPositionNotify1.EventNotifyHandle = _eventToReset.SafeWaitHandle.DangerousGetHandle();

                    // вторая часть
                    BufferPositionNotify bufferPositionNotify2 = new BufferPositionNotify();
                    bufferPositionNotify2.Offset = _bufferSize - 1; 
                    bufferPositionNotify2.EventNotifyHandle = _eventToReset.SafeWaitHandle.DangerousGetHandle();

                    _not.SetNotificationPositions(new BufferPositionNotify[] { bufferPositionNotify1, bufferPositionNotify2 }); 
                }
                catch { }
            }

           public void StartCapturing()
            {
                try
                {
                    _captureBuffer = new CaptureBuffer(_capBufDescr, _capture); 
                    SetBufferEvents();  
                    int halfBuffer = _bufferSize / 2;
                    _captureBuffer.Start(true);
                    bool readFirstBufferPart = true;   
                    int offset = 0;
                    MemoryStream memStream = new MemoryStream(halfBuffer); 

                    while (true) 
                    {
                        
                        _eventToReset.WaitOne();

                        memStream.Seek(0, SeekOrigin.Begin);
                        _captureBuffer.Read(offset, memStream, halfBuffer, LockFlag.None);
                        readFirstBufferPart = !readFirstBufferPart; 
                        offset = readFirstBufferPart ? 0 : halfBuffer; 
                        
                        byte[] dataToWrite = ALawEncoder.ALawEncode(memStream.GetBuffer());
                        
                        if (!_StopLoop)
                        OnBufferFulfill(dataToWrite, null);
                    }
                }
                catch {}
                
            }
           public void PlayReceivedVoice(byte[] byteData)
            {
                try
                {
                        byte[] byteDecodedData = new byte[byteData.Length * 2];

                        ALawDecoder.ALawDecode(byteData, out byteDecodedData); 
                        _playbackBuffer = new SecondaryBuffer(_playbackBufferDescription, _device);
                        _playbackBuffer.Write(0, byteDecodedData, LockFlag.None);
                        _playbackBuffer.Play(0, BufferPlayFlags.Default); 
                }
                catch  { }
            }
        }


        #region G.711 Encoder Classes

namespace G711Audio
{
    /// <summary>
    /// Turns 16-bit linear PCM values into 8-bit A-law bytes.
    /// </summary>
    public class ALawEncoder
    {
        public const int MAX = 0x7fff; //maximum that can be held in 15 bits

        /// <summary>
        /// An array where the index is the 16-bit PCM input, and the value is
        /// the a-law result.
        /// </summary>
        private static byte[] pcmToALawMap;

        static ALawEncoder()
        {
            pcmToALawMap = new byte[65536];
            for (int i = short.MinValue; i <= short.MaxValue; i++)
                pcmToALawMap[(i & 0xffff)] = encode(i);
        }

        /// <summary>
        /// Encode one a-law byte from a 16-bit signed integer. protected use only.
        /// </summary>
        /// <param name="pcm">A 16-bit signed pcm value</param>
        /// <returns>A a-law encoded byte</returns>
        private static byte encode(int pcm)
        {
            //Get the sign bit.  Shift it for later use without further modification
            int sign = (pcm & 0x8000) >> 8;
            //If the number is negative, make it positive (now it's a magnitude)
            if (sign != 0)
                pcm = -pcm;
            //The magnitude must fit in 15 bits to avoid overflow
            if (pcm > MAX) pcm = MAX;

            /* Finding the "exponent"
             * Bits:
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * S 7 6 5 4 3 2 1 0 0 0 0 0 0 0 0
             * We want to find where the first 1 after the sign bit is.
             * We take the corresponding value from the second row as the exponent value.
             * (i.e. if first 1 at position 7 -> exponent = 2)
             * The exponent is 0 if the 1 is not found in bits 2 through 8.
             * This means the exponent is 0 even if the "first 1" doesn't exist.
             */
            int exponent = 7;
            //Move to the right and decrement exponent until we hit the 1 or the exponent hits 0
            for (int expMask = 0x4000; (pcm & expMask) == 0 && exponent > 0; exponent--, expMask >>= 1) { }

            /* The last part - the "mantissa"
             * We need to take the four bits after the 1 we just found.
             * To get it, we shift 0x0f :
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * S 0 0 0 0 0 1 . . . . . . . . . (say that exponent is 2)
             * . . . . . . . . . . . . 1 1 1 1
             * We shift it 5 times for an exponent of two, meaning
             * we will shift our four bits (exponent + 3) bits.
             * For convenience, we will actually just shift the number, then AND with 0x0f. 
             * 
             * NOTE: If the exponent is 0:
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * S 0 0 0 0 0 0 0 Z Y X W V U T S (we know nothing about bit 9)
             * . . . . . . . . . . . . 1 1 1 1
             * We want to get ZYXW, which means a shift of 4 instead of 3
             */
            int mantissa = (pcm >> ((exponent == 0) ? 4 : (exponent + 3))) & 0x0f;

            //The a-law byte bit arrangement is SEEEMMMM (Sign, Exponent, and Mantissa.)
            byte alaw = (byte)(sign | exponent << 4 | mantissa);

            //Last is to flip every other bit, and the sign bit (0xD5 = 1101 0101)
            return (byte)(alaw ^ 0xD5);
        }

        /// <summary>
        /// Encode a pcm value into a a-law byte
        /// </summary>
        /// <param name="pcm">A 16-bit pcm value</param>
        /// <returns>A a-law encoded byte</returns>
        public static byte ALawEncode(int pcm)
        {
            return pcmToALawMap[pcm & 0xffff];
        }

        /// <summary>
        /// Encode a pcm value into a a-law byte
        /// </summary>
        /// <param name="pcm">A 16-bit pcm value</param>
        /// <returns>A a-law encoded byte</returns>
        public static byte ALawEncode(short pcm)
        {
            return pcmToALawMap[pcm & 0xffff];
        }

        /// <summary>
        /// Encode an array of pcm values
        /// </summary>
        /// <param name="data">An array of 16-bit pcm values</param>
        /// <returns>An array of a-law bytes containing the results</returns>
        public static byte[] ALawEncode(int[] data)
        {
            int size = data.Length;
            byte[] encoded = new byte[size];
            for (int i = 0; i < size; i++)
                encoded[i] = ALawEncode(data[i]);
            return encoded;
        }

        /// <summary>
        /// Encode an array of pcm values
        /// </summary>
        /// <param name="data">An array of 16-bit pcm values</param>
        /// <returns>An array of a-law bytes containing the results</returns>
        public static byte[] ALawEncode(short[] data)
        {
            int size = data.Length;
            byte[] encoded = new byte[size];
            for (int i = 0; i < size; i++)
                encoded[i] = ALawEncode(data[i]);
            return encoded;
        }

        /// <summary>
        /// Encode an array of pcm values
        /// </summary>
        /// <param name="data">An array of bytes in Little-Endian format</param>
        /// <returns>An array of a-law bytes containing the results</returns>
        public static byte[] ALawEncode(byte[] data)
        {
            int size = data.Length / 2;
            byte[] encoded = new byte[size];
            for (int i = 0; i < size; i++)
                encoded[i] = ALawEncode((data[2 * i + 1] << 8) | data[2 * i]);
            return encoded;
        }

        /// <summary>
        /// Encode an array of pcm values into a pre-allocated target array
        /// </summary>
        /// <param name="data">An array of bytes in Little-Endian format</param>
        /// <param name="target">A pre-allocated array to receive the A-law bytes.  This array must be at least half the size of the source.</param>
        public static void ALawEncode(byte[] data, byte[] target)
        {
            int size = data.Length / 2;
            for (int i = 0; i < size; i++)
                target[i] = ALawEncode((data[2 * i + 1] << 8) | data[2 * i]);
        }
    }


    /// <summary>
    /// Turns 8-bit A-law bytes back into 16-bit PCM values.
    /// </summary>
    public static class ALawDecoder
    {
        /// <summary>
        /// An array where the index is the a-law input, and the value is
        /// the 16-bit PCM result.
        /// </summary>
        private static short[] aLawToPcmMap;

        static ALawDecoder()
        {
            aLawToPcmMap = new short[256];
            for (byte i = 0; i < byte.MaxValue; i++)
                aLawToPcmMap[i] = decode(i);
        }

        /// <summary>
        /// Decode one a-law byte. For protected use only.
        /// </summary>
        /// <param name="alaw">The encoded a-law byte</param>
        /// <returns>A short containing the 16-bit result</returns>
        private static short decode(byte alaw)
        {
            //Invert every other bit, and the sign bit (0xD5 = 1101 0101)
            alaw ^= 0xD5;

            //Pull out the value of the sign bit
            int sign = alaw & 0x80;
            //Pull out and shift over the value of the exponent
            int exponent = (alaw & 0x70) >> 4;
            //Pull out the four bits of data
            int data = alaw & 0x0f;

            //Shift the data four bits to the left
            data <<= 4;
            //Add 8 to put the result in the middle of the range (like adding a half)
            data += 8;

            //If the exponent is not 0, then we know the four bits followed a 1,
            //and can thus add this implicit 1 with 0x100.
            if (exponent != 0)
                data += 0x100;
            /* Shift the bits to where they need to be: left (exponent - 1) places
             * Why (exponent - 1) ?
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * . 7 6 5 4 3 2 1 . . . . . . . . <-- starting bit (based on exponent)
             * . . . . . . . Z x x x x 1 0 0 0 <-- our data (Z is 0 only when exponent is 0)
             * We need to move the one under the value of the exponent,
             * which means it must move (exponent - 1) times
             * It also means shifting is unnecessary if exponent is 0 or 1.
             */
            if (exponent > 1)
                data <<= (exponent - 1);

            return (short)(sign == 0 ? data : -data);
        }

        /// <summary>
        /// Decode one a-law byte
        /// </summary>
        /// <param name="alaw">The encoded a-law byte</param>
        /// <returns>A short containing the 16-bit result</returns>
        public static short ALawDecode(byte alaw)
        {
            return aLawToPcmMap[alaw];
        }

        /// <summary>
        /// Decode an array of a-law encoded bytes
        /// </summary>
        /// <param name="data">An array of a-law encoded bytes</param>
        /// <returns>An array of shorts containing the results</returns>
        public static short[] ALawDecode(byte[] data)
        {
            int size = data.Length;
            short[] decoded = new short[size];
            for (int i = 0; i < size; i++)
                decoded[i] = aLawToPcmMap[data[i]];
            return decoded;
        }

        /// <summary>
        /// Decode an array of a-law encoded bytes
        /// </summary>
        /// <param name="data">An array of a-law encoded bytes</param>
        /// <param name="decoded">An array of shorts containing the results</param>
        /// <remarks>Same as the other method that returns an array of shorts</remarks>
        public static void ALawDecode(byte[] data, out short[] decoded)
        {
            int size = data.Length;
            decoded = new short[size];
            for (int i = 0; i < size; i++)
                decoded[i] = aLawToPcmMap[data[i]];
        }

        /// <summary>
        /// Decode an array of a-law encoded bytes
        /// </summary>
        /// <param name="data">An array of a-law encoded bytes</param>
        /// <param name="decoded">An array of bytes in Little-Endian format containing the results</param>
        public static void ALawDecode(byte[] data, out byte[] decoded)
        {
            int size = data.Length;
            decoded = new byte[size * 2];
            for (int i = 0; i < size; i++)
            {
                //First byte is the less significant byte
                decoded[2 * i] = (byte)(aLawToPcmMap[data[i]] & 0xff);
                //Second byte is the more significant byte
                decoded[2 * i + 1] = (byte)(aLawToPcmMap[data[i]] >> 8);
            }
        }
    }
}
#endregion G.711 Encoding
