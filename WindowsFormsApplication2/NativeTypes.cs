using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using System.IO.MemoryMappedFiles;
using System.Diagnostics;

namespace DisplayForwarder
{

    // Memfile "MAHMSharedMemory"
    struct MAHM_SHARED_MEMORY_HEADER
    {
        public UInt32 dwSignature;
        //signature allows applications to verify status of shared memory

        //The signature can be set to:
        //'MAHM'	- hardware monitoring memory is initialized and contains 
        //			valid data 
        //0xDEAD	- hardware monitoring memory is marked for deallocation and
        //			no longer contain valid data
        //otherwise the memory is not initialized
        public UInt32 dwVersion;
        //header version ((major<<16) + minor)
        //must be set to 0x00020000 for v2.0
        public UInt32 dwHeaderSize;
        //size of header
        public UInt32 dwNumEntries;
        //number of subsequent MAHM_SHARED_MEMORY_ENTRY entries
        public UInt32 dwEntrySize;
        //size of entries in subsequent MAHM_SHARED_MEMORY_ENTRY entries array
        public UInt32 time;
        //last polling time 

        //WARNING! Force 32-bit time_t usage with #define _USE_32BIT_TIME_T 
        //to provide compatibility with VC8.0 and newer compiler versions

        //WARNING! The following fields are valid for v2.0 and newer shared memory layouts only

        public UInt32 dwNumGpuEntries;
        //number of subsequent MAHM_SHARED_MEMORY_GPU_ENTRY entries
        public UInt32 dwGpuEntrySize;
        //size of entries in subsequent MAHM_SHARED_MEMORY_GPU_ENTRY entries array

    }

    struct MAHM_SHARED_MEMORY_ENTRY
    {
        //char szSrcName[MAX_PATH];
        char[] szSrcName;
        //data source name (e.g. "Core clock")
        //char szSrcUnits[MAX_PATH];
        char[] szSrcUnits;
        //data source units (e.g. "MHz")

        //char szLocalizedSrcName[MAX_PATH];
        char[] szLocalizedSrcName;
        //localized data source name (e.g. "×àñòîòà ÿäðà" for Russian GUI)
        //char szLocalizedSrcUnits[MAX_PATH];
        char[] szLocalizedSrcUnits;
        //localized data source units (e.g. "ÌÃö" for Russian GUI)

        //char szRecommendedFormat[MAX_PATH];
        char[] szRecommendedFormat;
        //recommended output format (e.g. "%.3f" for "Core voltage" data source) 

        float data;
        //last polled data (e.g. 500MHz)
        //(this field can be set to FLT_MAX if data is not available at
        //the moment)
        float minLimit;
        //minimum limit for graphs (e.g. 0MHz)
        float maxLimit;
        //maximum limit for graphs (e.g. 2000MHz)

        UInt32 dwFlags;
        //bitmask containing combination of MAHM_SHARED_MEMORY_ENTRY_FLAG_...

        //WARNING! The following fields are valid for v2.0 and newer shared memory layouts only

        UInt32 dwGpu;
        //data source GPU index (zero based) or 0xFFFFFFFF for global data sources (e.g. Framerate)
        UInt32 dwSrcId;
        //data source ID

    }

    // Memfile "RTSSSharedMemoryV2"
    [StructLayout(LayoutKind.Sequential)]
    public struct RTSS_SHARED_MEMORY
    {
        public UInt32 dwSignature;
        //signature allows applications to verify status of shared memory

        //The signature can be set to:
        //'RTSS'	- statistics server's memory is initialized and contains 
        //			valid data 
        //0xDEAD	- statistics server's memory is marked for deallocation and
        //			no longer contain valid data
        //otherwise	the memory is not initialized
        public UInt32 dwVersion;
        //structure version ((major<<16) + minor)
        //must be set to 0x0002xxxx for v2.x structure 

        public UInt32 dwAppEntrySize;
        //size of RTSS_SHARED_MEMORY_OSD_ENTRY for compatibility with future versions
        public UInt32 dwAppArrOffset;
        //offset of arrOSD array for compatibility with future versions
        public UInt32 dwAppArrSize;
        //size of arrOSD array for compatibility with future versions

        public UInt32 dwOSDEntrySize;
        //size of RTSS_SHARED_MEMORY_APP_ENTRY for compatibility with future versions
        public UInt32 dwOSDArrOffset;
        //offset of arrApp array for compatibility with future versions
        public UInt32 dwOSDArrSize;
        //size of arrOSD array for compatibility with future versions

        public UInt32 dwOSDFrame;
        //Global OSD frame ID. Increment it to force the server to update OSD for all currently active 3D
        //applications.
    }

    //application descriptor structure
    [StructLayout(LayoutKind.Sequential)]
    public struct RTSS_SHARED_MEMORY_APP_ENTRY
    {
        //application identification related fields
        //0x000
        public UInt32 dwProcessID;
        //process ID

        //0x004
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 512)]
        public byte[] szName;
        //process executable name

        //0x108
        public UInt32 dwFlags;
        //application specific flags

        //instantaneous framerate related fields

        //0x10C
        public UInt32 dwTime0;
        //start time of framerate measurement period (in milliseconds)

        //Take a note that this field must contain non-zero value to calculate 
        //framerate properly!
        //0x110
        public UInt32 dwTime1;
        //end time of framerate measurement period (in milliseconds)

        //0x114
        public UInt32 dwFrames;
        //amount of frames rendered during (dwTime1 - dwTime0) period

        //0x118
        public UInt32 dwFrameTime;
        //frame time (in microseconds)


        //to calculate framerate use the following formulas:

        //1000.0f * dwFrames / (dwTime1 - dwTime0) for framerate calculated once per second
        //or
        //1000000.0f / dwFrameTime for framerate calculated once per frame 

        //framerate statistics related fields

        //0x218
        public UInt32 dwStatFlags;
        //bitmask containing combination of STATFLAG_... flags

        //0x21C
        public UInt32 dwStatTime0;
        //statistics record period start time

        //0x220
        public UInt32 dwStatTime1;
        //statistics record period end time

        //0x224
        public UInt32 dwStatFrames;
        //total amount of frames rendered during statistics record period

        //0x228
        public UInt32 dwStatCount;
        //amount of min/avg/max measurements during statistics record period 

        //0x22C
        public UInt32 dwStatFramerateMin;
        //minimum instantaneous framerate measured during statistics record period 

        //0x230
        public UInt32 dwStatFramerateAvg;
        //average instantaneous framerate measured during statistics record period 

        //0x234
        public UInt32 dwStatFramerateMax;
        //maximum instantaneous framerate measured during statistics record period 

        //OSD related fields

        //0x238
        public UInt32 dwOSDX;
        //OSD X-coordinate (coordinate wrapping is allowed, i.e. -5 defines 5
        //pixel offset from the right side of the screen)

        //0x23C
        public UInt32 dwOSDY;
        //OSD Y-coordinate (coordinate wrapping is allowed, i.e. -5 defines 5
        //pixel offset from the bottom side of the screen)

        //0x240
        public UInt32 dwOSDPixel;
        //OSD pixel zooming ratio

        //0x244
        public UInt32 dwOSDColor;
        //OSD color in RGB format

        //0x248
        public UInt32 dwOSDFrame;
        //application specific OSD frame ID. Don't change it directly!

        //0x24C
        public UInt32 dwScreenCaptureFlags;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 512)]
        public byte[] szScreenCapturePath;//[MAX_PATH];

        //next fields are valid for v2.1 and newer shared memory format only

        //0x44C
        public UInt32 dwOSDBgndColor;
        //OSD background color in RGB format

        //next fields are valid for v2.2 and newer shared memory format only

        //0x450
        public UInt32 dwVideoCaptureFlags;
        //0x454
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 512)]
        public byte[] szVideoCapturePath;//[MAX_PATH];
        //0x654
        public UInt32 dwVideoFramerate;
        //0x658
        public UInt32 dwVideoFramesize;
        //0x65C
        public UInt32 dwVideoFormat;
        //0x660
        public UInt32 dwVideoQuality;
        //0x664
        public UInt32 dwVideoCaptureThreads;
        //0x668
        public UInt32 dwScreenCaptureQuality;
        //0x66C
        public UInt32 dwScreenCaptureThreads;

        //next fields are valid for v2.3 and newer shared memory format only

        //0x670
        public UInt32 dwAudioCaptureFlags;

        //next fields are valid for v2.4 and newer shared memory format only

        //0x674
        public UInt32 dwVideoCaptureFlagsEx;

        //next fields are valid for v2.5 and newer shared memory format only

        //0x678
        public UInt32 dwAudioCaptureFlags2;

        //0x67C
        public UInt32 dwStatFrameTimeMin;
        //0x680
        public UInt32 dwStatFrameTimeAvg;
        //0x684
        public UInt32 dwStatFrameTimeMax;
        //0x688
        public UInt32 dwStatFrameTimeCount;
        //0x69C
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4, SizeConst = 1024)]
        public UInt32[] dwStatFrameTimeBuf;
        //0x169C
        public UInt32 dwStatFrameTimeBufPos;
        //0x1670
        public UInt32 dwStatFrameTimeBufFramerate;

        //next fields are valid for v2.6 and newer shared memory format only

        //0x1674
        public UInt64 qwAudioCapturePTTEventPush;
        //0x167C
        public UInt64 qwAudioCapturePTTEventRelease;

        //0x1684
        public UInt64 qwAudioCapturePTTEventPush2;
        //0x168C
        public UInt64 qwAudioCapturePTTEventRelease2;

        //next fields are valid for v2.8 and newer shared memory format only

        //0x1694
        public UInt32 dwPrerecordSizeLimit;
        //0x1698
        public UInt32 dwPrerecordTimeLimit;
    }

    public class AppStructAccessor
    {
        MemoryMappedViewAccessor _mac;
        public UInt32 uOffset;

        public AppStructAccessor(MemoryMappedViewAccessor mac)
        {
            this._mac = mac;
            this._mac.Read<UInt32>(0xC, out uOffset);
        }

        public AppStructAccessor(MemoryMappedViewAccessor mac, uint manualOffset)
        {
            this._mac = mac;
            this.uOffset = manualOffset;
        }

        //0x208
        public UInt32 dwTime0
        {
            get
            {
                UInt32 u;
                _mac.Read<UInt32>(uOffset + 0x10C, out u);
                return u;
            }
            set
            {
                _mac.Write<UInt32>(uOffset + 0x10C, ref value);
            }
        }
        //start time of framerate measurement period (in milliseconds)

        //Take a note that this field must contain non-zero value to calculate 
        //framerate properly!
        //0x20C
        public UInt32 dwTime1
        {
            get
            {
                UInt32 u;
                _mac.Read<UInt32>(uOffset + 0x110, out u);
                return u;
            }
            set
            {
                _mac.Write<UInt32>(uOffset + 0x110, ref value);
            }
        }
        //end time of framerate measurement period (in milliseconds)

        //0x210
        public UInt32 dwFrames
        {
            get
            {
                UInt32 u;
                _mac.Read<UInt32>(uOffset + 0x114, out u);
                return u;
            }
            set
            {
                _mac.Write<UInt32>(uOffset + 0x114, ref value);
            }
        }
        //amount of frames rendered during (dwTime1 - dwTime0) period

        //0x214
        public UInt32 dwFrameTime
        {
            get
            {
                UInt32 u;
                _mac.Read<UInt32>(uOffset + 0x118, out u);
                return u;
            }
            set
            {
                _mac.Write<UInt32>(uOffset + 0x118, ref value);
            }
        }
        //frame time (in microseconds)


        //to calculate framerate use the following formulas:

        //1000.0f * dwFrames / (dwTime1 - dwTime0) for framerate calculated once per second
        //or
        //1000000.0f / dwFrameTime for framerate calculated once per frame 
        public float FpsByFrameTime
        {
            get { return 1000000.0f / (float)dwFrameTime; }
        }

        public UInt32 dwProcessID
        {
            get
            {
                UInt32 u;
                _mac.Read<UInt32>(uOffset + 0x000, out u);
                return u;
            }
        }

        //0x004
        public string szName
        {
            get
            {
                byte[] arr = new byte[260];
                _mac.ReadArray<byte>(uOffset + 0x004, arr, 0, 260);
                int idx0 = 0;
                for (int i = 0; i < 260; i++) if (arr[i] == '\0') { idx0 = i; break; }
                byte[] bt = new byte[idx0];
                Array.Copy(arr, 0, bt, 0, idx0);

                return System.Text.Encoding.Default.GetString(bt);
            }
        }


    }



    public class MonStructAccessor
    {
        private MemoryMappedViewAccessor _mac;
        private UInt32 uOffset;

        public MonStructAccessor(MemoryMappedViewAccessor mac)
        {
            this._mac = mac;
            this._mac.Read<UInt32>(0xC, out uOffset);
        }

        public MonStructAccessor(MemoryMappedViewAccessor mac, uint manualOffset)
        {
            this._mac = mac;
            this.uOffset = manualOffset;
        }

        //0x000
        public string szSrc
        {
            get
            {
                byte[] arr = new byte[260];
                _mac.ReadArray<byte>(uOffset + 0x000, arr, 0, 260);
                int idx0 = 0;
                for (int i = 0; i < 260; i++) if (arr[i] == '\0') { idx0 = i; break; }
                byte[] bt = new byte[idx0];
                Array.Copy(arr, 0, bt, 0, idx0);

                return System.Text.Encoding.Default.GetString(bt);
            }
        }

        //0x104
        public string szUnits
        {
            get
            {
                byte[] arr = new byte[260];
                _mac.ReadArray<byte>(uOffset + 0x104, arr, 0, 260);
                int idx0 = 0;
                for (int i = 0; i < 260; i++) if (arr[i] == '\0') { idx0 = i; break; }
                byte[] bt = new byte[idx0];
                Array.Copy(arr, 0, bt, 0, idx0);

                return System.Text.Encoding.Default.GetString(bt);
            }
        }

        //0x514
        public float data
        {
            get
            {
                float f;
                _mac.Read<float>(uOffset + 0x514, out f);
                return f;
            }
        }

        //0x518
        public float minLimit
        {
            get
            {
                float f;
                _mac.Read<float>(uOffset + 0x518, out f);
                return f;
            }
        }

        //0x51C
        public float maxLimit
        {
            get
            {
                float f;
                _mac.Read<float>(uOffset + 0x51C, out f);
                return f;
            }
        }

    }


    public class Auxiliary
    {
        public static Double Calculate(CounterSample oldSample, CounterSample newSample)
        {
            double difference = newSample.RawValue - oldSample.RawValue;
            double timeInterval = newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec;
            if (timeInterval != 0) return 100 * (1 - (difference / timeInterval));
            return 0;
        }
    }

}
