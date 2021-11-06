using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TsudaKageyu;

using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DisplayForwarder
{
    

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        public List<string> GetProcLoad()
        {
            List<string> ret = new List<string>();

            var pc = new PerformanceCounter("Processor Information", "% Processor Time");
            var cat = new PerformanceCounterCategory("Processor Information");
            var instances = cat.GetInstanceNames();
            var cs = new Dictionary<string, CounterSample>();

            foreach (var s in instances)
            {
                pc.InstanceName = s;
                cs.Add(s, pc.NextSample());
            }

            Array.Sort(instances);

            foreach (var s in instances)
            {
                pc.InstanceName = s;
                ret.Add(string.Format("{0} - {1:f}", s, Auxiliary.Calculate(cs[s], pc.NextSample())));
                cs[s] = pc.NextSample();
            }

            return ret;
        }

        public void CpuCountersCreate(out List<PerformanceCounter> pcs)
        {
            List<string> ret = new List<string>();

            var pc = new PerformanceCounter("Processor Information", "% Processor Time");
            var cat = new PerformanceCounterCategory("Processor Information");
            var names = cat.GetInstanceNames().ToList();

            names.Sort();

            pcs = new List<PerformanceCounter>();
            foreach (var s in names)
            {
                var npc = new PerformanceCounter("Processor Information", "% Processor Time", s);
                pcs.Add(npc);
            }

            return;
        }

        public void CpuCountersCreate(out PerformanceCounter pc)
        {
            pc = new PerformanceCounter("Processor Information", "% Processor Time");

            return;
        }

        public void CpuCountersCreate(out PerformanceCounter pc, out PerformanceCounterCategory cat)
        {
            pc = new PerformanceCounter("Processor Information", "% Processor Time");
            cat = new PerformanceCounterCategory("Processor Information");

            return;
        }

        public List<string> CpuCountersRead(PerformanceCounter pc, PerformanceCounterCategory cat)
        {
            var instances = cat.GetInstanceNames();
            var cs = new Dictionary<string, CounterSample>();

            List<string> ret = new List<string>();

            foreach (var s in instances)
            {
                pc.InstanceName = s;
                cs.Add(s, pc.NextSample());
            }

            foreach (var s in instances)
            {
                pc.InstanceName = s;
                ret.Add(string.Format("{0:0.00}", Calculate(cs[s], pc.NextSample())));
                cs[s] = pc.NextSample();
            }


            return ret;
        }


        public static Double Calculate(CounterSample oldSample, CounterSample newSample)
        {
            double difference = newSample.RawValue - oldSample.RawValue;
            double timeInterval = newSample.TimeStamp100nSec - oldSample.TimeStamp100nSec;
            if (timeInterval != 0) return 100 * (1 - (difference / timeInterval));
            return 0;
        }


        public List<string> CpuCountersRead(PerformanceCounter pc)
        {
            List<string> ret = new List<string>();
            var cat = new PerformanceCounterCategory("Processor Information");

            foreach (var inst in cat.GetInstanceNames())
            {
                pc.InstanceName = inst;
                float cnt = (float)(pc.NextValue());
                if (cnt < 0.0f) cnt = 0.0f;
                else if (cnt >= 100.0000f) cnt = 99.9f;
                ret.Add(string.Format("{0:0.00}", cnt));
            }

            return ret;
        }



        public List<float> CpuCountersRead(List<PerformanceCounter> pcs)
        {
            List<float> ret = new List<float>();

            foreach (var pc in pcs)
            {
                float cnt = (float)(pc.NextValue());
                if (cnt < 0.0f) cnt = 0.0f;
                else if (cnt >= 100.0000f) cnt = 99.9f;
                ret.Add(cnt);
            }

            return ret;
        }

        public void DrawTitle(string title)
        {
            if (_port == null) return;

            _port.Write("C");
            /*for (int i = 0; i < test.Length; i++)
            {
                port.Write(test[i].ToString());
            }*/
            _port.Write(title);
            _port.Write("\r");
        }

        public void DrawString(string cmd, string text)
        {
            if (_port == null) return;

            _port.Write(cmd);
            /*for (int i = 0; i < test.Length; i++)
            {
                port.Write(test[i].ToString());
            }*/
            _port.Write(text);
            _port.Write("\r");
        }

        public void SetImagePos(UInt16 x, UInt16 y, int width, int height)
        {
            if (_protVersion == 2) SetImagePos2(x, y, (UInt16)width, (UInt16)height);
            else if (_protVersion == 1) SetImagePos1((UInt32)x, (UInt32)y);
        }

        public void SetImagePos1(UInt32 x, UInt32 y)
        {
            if (_port == null) return;

            var ax = BitConverter.GetBytes(x);
            var ay = BitConverter.GetBytes(y);
            Byte[] bt = new byte[ax.Length + ay.Length];
            Array.Copy(ax, 0, bt, 0, ax.Length);
            Array.Copy(ay, 0, bt, ax.Length, ay.Length);
            var base64 = Convert.ToBase64String(bt);

            _port.Write("j");
            _port.Write(base64);
            _port.Write("\r");
        }


        public void SetImagePos2(UInt16 x, UInt16 y, UInt16 width, UInt16 height)
        {
            if (_port == null) return;

            var ax = BitConverter.GetBytes(x);
            var ay = BitConverter.GetBytes(y);
            var aw = BitConverter.GetBytes(width);
            var ah = BitConverter.GetBytes(height);
            Byte[] bt = new byte[ax.Length + ay.Length + aw.Length + ah.Length];
            Array.Copy(ax, 0, bt, 0, ax.Length);
            Array.Copy(ay, 0, bt, ax.Length, ay.Length);
            Array.Copy(aw, 0, bt, ax.Length + ay.Length, aw.Length);
            Array.Copy(ah, 0, bt, ax.Length + ay.Length + aw.Length, ah.Length);
            var base64 = Convert.ToBase64String(bt);

            _port.Write("J");
            _port.Write(base64);
            _port.Write("\r");

            System.Threading.Thread.Sleep(10);
        }

        public Bitmap GetBitmapFromExe(string pathToExe)
        {
            // Construct an IconExtractor object with a file.
            //IconExtractor ie = new IconExtractor(@"C:\Program Files (x86)\Steam\SteamApps\common\Planetary Annihilation Titans\PA.exe");
            TsudaKageyu.IconExtractor ie = new TsudaKageyu.IconExtractor(pathToExe);
            // Extract all the icons in one go.
            Icon[] allIcons = ie.GetAllIcons();

            //Icon[] splitIcons = IconUtil.Split(ie.GetIcon(0));

            if(allIcons==null || allIcons.Length==0)
            {
                return null;
            }

            var LargeIcon = new Icon(ie.GetIcon(0), 128, 128);
            if(LargeIcon.Width < 128)
            {
                // if the icon is too small, center it on a black background
                var bmp = new Bitmap(128, 128);
                var graph = Graphics.FromImage(bmp);
                var brush = new SolidBrush(Color.Black);

                graph.FillRectangle(brush, new RectangleF(0, 0, 127, 127));
                graph.DrawIcon(LargeIcon, new Rectangle((128 - LargeIcon.Width) / 2, (128 - LargeIcon.Height) / 2, LargeIcon.Width, LargeIcon.Height));
                //return new Bitmap(128, 128, graph);
                return bmp;
            }

            return LargeIcon.ToBitmap();
        }

        public Bitmap GetBitmapFromExeScaled(string pathToExe)
        {
            // Construct an IconExtractor object with a file.
            //IconExtractor ie = new IconExtractor(@"C:\Program Files (x86)\Steam\SteamApps\common\Planetary Annihilation Titans\PA.exe");
            TsudaKageyu.IconExtractor ie = new TsudaKageyu.IconExtractor(pathToExe);
            // Extract all the icons in one go.
            Icon[] allIcons = ie.GetAllIcons();

            //Icon[] splitIcons = IconUtil.Split(ie.GetIcon(0));

            if (allIcons == null || allIcons.Length == 0)
            {
                return null;
            }

            var LargeIcon = new Icon(ie.GetIcon(0), 128, 128);
            if (LargeIcon.Width < 128)
            {
                return ResizeImage(LargeIcon.ToBitmap(), new Size(128, 128));
            }

            return LargeIcon.ToBitmap();
        }


        public void DrawImage(Bitmap bmp)
        {
            if (_protVersion == 2) DrawImage2(bmp);
            else if (_protVersion == 1) DrawImage(bmp);
        }

        public void DrawImage1(Bitmap bmp)
        {
            if (_port == null || !_port.IsOpen) return;

            // convert bitmap to byte array
            var bmpArray = ImageToByte2(bmp);

            //File.WriteAllBytes(@"C:\temp\test.bmp", bmpArray);

            // Convert byte-array from binary to base64
            var base64 = Convert.ToBase64String(bmpArray);

            int len = base64.Length;

            _port.Write("i");
            //for (int i = 0; i < base64.Length; i++)
            //{
            //    port.Write(base64[i].ToString());
            //}
            _port.Write(base64.ToString());
            _port.Write("\r");
        }


        public void DrawImage2(Bitmap bmp)
        {
            if (_port == null || !_port.IsOpen) return;

            // convert bitmap to byte array with 16bpp color
            byte[] bmpArray = null;
            List<byte> lst = new List<byte>();

            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var px = bmp.GetPixel(x, y);
                    byte r = px.R;
                    byte g = px.G;
                    byte b = px.B;

                    uint pixel = ((uint)b >> 3);
                    pixel = pixel | (((uint)g << 3) & 0x07E0);
                    pixel = pixel | (((uint)r << 8) & 0xF800);

                    lst.Add((byte)(pixel >> 8));
                    lst.Add((byte)pixel);
                }
            }
            bmpArray = lst.ToArray();

            // Convert byte-array from binary to base64
            var base64 = Convert.ToBase64String(bmpArray);

            int len = base64.Length;

            for (int i = 0; i < (len / 16); i++)
            {
                _port.Write("I");
                _port.Write(base64.Substring(i * 16, 16));
                _port.Write("\r");
            }

            if(len % 16 > 0)
            {
                _port.Write("I");
                _port.Write(base64.Substring(base64.Length - (len%16), (len % 16)));
                _port.Write("\r");
            }
            
        }



        public string _portName = "COM4";
        public SerialPort _port = null;
        public System.Windows.Forms.Timer _timerRefresh = null;
        int _timerRefreshInterval = 1000;
        public uint _protVersion = 1;

        MemoryMappedFile _mfRiva = null;
        MemoryMappedViewAccessor _acRiva = null;
        RTSS_SHARED_MEMORY _headerRiva;

        MemoryMappedFile _mfBurner = null;
        MemoryMappedViewAccessor _acBurner = null;
        MAHM_SHARED_MEMORY_HEADER _headerBurner;

        string _currentProcName = string.Empty;
        Bitmap _bitmapBlack = null;
        List<PerformanceCounter> _pcNames = null;
        PerformanceCounter _pc = null;
        PerformanceCounterCategory _cat = null;

        Temperature _cpuTemp = null;

        bool bFirstStart = true;


        public bool InitMFileRiva()
        {
            if (_mfRiva != null) return false;

            // Riva Statistics Server init
            try
            {
                _mfRiva = MemoryMappedFile.OpenExisting("RTSSSharedMemoryV2");
            }
            catch (FileNotFoundException xcp)
            {
                _mfRiva = null;
                _acRiva = null;
                return false;
            }
            
            _acRiva = _mfRiva.CreateViewAccessor();
            _acRiva.Read<RTSS_SHARED_MEMORY>(0, out _headerRiva);

            return CheckMFileRiva();
        }

        public bool CheckMFileRiva()
        {
            return _mfRiva != null && _headerRiva.dwSignature == 0x52545353;
        }

        public bool CheckMFileAfterburner()
        {
            return _mfBurner!=null && _headerBurner.dwSignature == 0x4D41484D;
        }

        public bool InitMFileAfterburner()
        {
            if (_mfBurner != null) return false;

            // Afterburner init
            try
            {
                _mfBurner = MemoryMappedFile.OpenExisting("MAHMSharedMemory");
            }
            catch (FileNotFoundException xcp)
            {
                _mfBurner = null;
                _acBurner = null;
                return false;
            }
            
            _acBurner = _mfBurner.CreateViewAccessor();
            _acBurner.Read<MAHM_SHARED_MEMORY_HEADER>(0, out _headerBurner);

            return CheckMFileAfterburner();
        }


        private void button1_Click(object sender, EventArgs e)
        {
        }


        public AppStructAccessor GetForegroundApp()
        {
            List<AppStructAccessor> imagePaths = new List<AppStructAccessor>();
            for (int i = 0; i < _headerRiva.dwAppArrSize; i++)
            {
                AppStructAccessor ax = new AppStructAccessor(_acRiva, _headerRiva.dwAppArrOffset + _headerRiva.dwAppEntrySize * (uint)i);
                string name = ax.szName;
                if (name == string.Empty) break;

                imagePaths.Add(ax);
                //var proc = Process.GetProcessById((int)(ax.dwProcessID));
                //string title = proc.MainWindowTitle;
            }

            var mainApp = imagePaths.Find(x => x.dwFrameTime > 0 && x.dwProcessID != 0);
            return mainApp;
        }


        public List<MonStructAccessor> GetMonitors()
        {
            List<MonStructAccessor> entries = new List<MonStructAccessor>();
            for (int i = 0; i < _headerBurner.dwNumEntries; i++)
            {
                MonStructAccessor ax = new MonStructAccessor(_acBurner, _headerBurner.dwHeaderSize + _headerBurner.dwEntrySize * (uint)i);
                entries.Add(ax);
            }
            return entries;
        }



        private void _timerRefresh_Tick(object sender, EventArgs e)
        {
            //_timerRefresh.Enabled = false;

            try
            {
                if(_port==null)
                {
                    Init();
                    if (_port != null)
                    {
                        this.notifyIconMain.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info; //Shows the info icon so the user doesn't thing there is an error.
                        this.notifyIconMain.BalloonTipText = "Display connected";
                        this.notifyIconMain.BalloonTipTitle = "Display forwarder";
                        notifyIconMain.ShowBalloonTip(500);

                        InitializeDisplay();
                    }
                }

                // Send Default Image
                if (bFirstStart)
                {
                    bFirstStart = false;
                    SetImagePos(0, 0, _bitmapBlack.Width, _bitmapBlack.Height);
                    DrawImage(_bitmapBlack);
                }

                // Update Application
                if (!CheckMFileRiva())
                {
                    _mfRiva = null;
                    if (InitMFileRiva())
                    {
                        //_timerRefresh.Enabled = true;
                        RunRivaDependentSub();
                    }
                }
                else
                {
                    RunRivaDependentSub();
                }

                // Show GPU-related performance counters
                if (!CheckMFileAfterburner())
                {
                    _mfBurner = null;
                    if (InitMFileAfterburner())
                    {
                        //_timerRefresh.Enabled = true;
                        RunAfterburnerDependentSub();
                    }
                }
                else
                {
                    RunAfterburnerDependentSub();
                }

                // Show CPU load
                var vals = CpuCountersRead(_pcNames);
                float sum = 0.0f;
                for (int i = 2; i < _pcNames.Count; i++) sum += vals[i];
                sum /= (float)(_pcNames.Count - 2);
                //var vals = CpuCountersRead(_pc);
                //var vals = CpuCountersRead(_pc, _cat);
                if(_pcNames.Count > 2) DrawString("a", string.Format("{0:0.0}", vals[2]));
                if(_pcNames.Count > 3) DrawString("b", string.Format("{0:0.0}", vals[3]));
                if(_pcNames.Count > 4) DrawString("c", string.Format("{0:0.0}", vals[4]));
                if(_pcNames.Count > 5) DrawString("d", string.Format("{0:0.0}", vals[5]));
                if(_pcNames.Count > 6) DrawString("e", string.Format("{0:0.0}", vals[6]));
                if(_pcNames.Count > 7) DrawString("f", string.Format("{0:0.0}", vals[7]));
                if (_pcNames.Count > 8) DrawString("u", string.Format("{0:0.0}", vals[8]));
                if(_pcNames.Count > 9) DrawString("v", string.Format("{0:0.0}", vals[9]));
                if(_pcNames.Count > 10) DrawString("w", string.Format("{0:0.0}", vals[10]));
                if(_pcNames.Count > 11) DrawString("x", string.Format("{0:0.0}", vals[11]));
                if(_pcNames.Count > 12) DrawString("y", string.Format("{0:0.0}", vals[12]));
                if(_pcNames.Count > 13) DrawString("z", string.Format("{0:0.0}", vals[13]));
                DrawString("h", string.Format("{0:0.0}", sum));
                //DrawString("a", "99.9");
                //DrawString("b", "99.9");
                //DrawString("c", "99.9");
                //DrawString("d", "99.9");
                //DrawString("e", "99.9");
                //DrawString("f", "99.9");
                //DrawString("h", "99.9");


                //var cpuTemps = Temperature.Temperatures;
                //double dCpuTemp = _cpuTemp.CurrentValue;

                //double dCpuTemp = 0.0;

                double dCpuTemp = Temperature.Tctl;
                double dSysTemp = Temperature.Tsys;

                DrawString("A", string.Format("{0:0.0} °C", dCpuTemp));
                DrawString("B", string.Format("{0:0.0} °C", dSysTemp));
                if (_bmpTemperatureCpu == _bmpTemperatureCold && dCpuTemp >= 52)
                {
                    _bmpTemperatureCpu = _bmpTemperatureHot;
                    SetImagePos(0, 160, _bmpTemperatureCpu.Width, _bmpTemperatureCpu.Height);
                    DrawImage(_bmpTemperatureCpu);
                    //hasTPicChanged = true;
                }
                else if (_bmpTemperatureCpu == _bmpTemperatureHot && dCpuTemp <= 48)
                {
                    _bmpTemperatureCpu = _bmpTemperatureCold;
                    SetImagePos(0, 160, _bmpTemperatureCpu.Width, _bmpTemperatureCpu.Height);
                    DrawImage(_bmpTemperatureCpu);
                    //hasTPicChanged = true;
                }
                else if (_bmpTemperatureCpu == _bmpTemperatureHot && dCpuTemp >= 70)
                {
                    _bmpTemperatureCpu = _bmpTemperatureBlazing;
                    SetImagePos(0, 160, _bmpTemperatureCpu.Width, _bmpTemperatureCpu.Height);
                    DrawImage(_bmpTemperatureCpu);
                    //hasTPicChanged = true;
                }
                else if (_bmpTemperatureCpu == _bmpTemperatureBlazing && dCpuTemp <= 64)
                {
                    _bmpTemperatureCpu = _bmpTemperatureHot;
                    SetImagePos(0, 160, _bmpTemperatureCpu.Width, _bmpTemperatureCpu.Height);
                    DrawImage(_bmpTemperatureCpu);
                    //hasTPicChanged = true;
                }
                

            }
            catch (InvalidOperationException xcp)
            {
                if (!(xcp is InvalidOperationException) && !(xcp is IOException)) throw;

                if (_port != null)
                {
                    this.notifyIconMain.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Warning; //Shows the info icon so the user doesn't thing there is an error.
                    this.notifyIconMain.BalloonTipText = "Connection to Display lost";
                    this.notifyIconMain.BalloonTipTitle = "Display forwarder";
                    notifyIconMain.ShowBalloonTip(500);
                }

                if (_port != null && _port.IsOpen) _port.Close();
                _port = null;
            }

            //_timerRefresh.Interval = _timerRefreshInterval;
            //_timerRefresh.Enabled = true;
        }


        void RunAfterburnerDependentSub()
        {
            if (!CheckMFileAfterburner()) return;

            var mons = GetMonitors();
            foreach (var mon in mons)
            {
                //bool hasTPicChanged = false;
                switch (mon.szSrc)
                {
                    case "Power":
                        DrawString("P", mon.data + " " + mon.szUnits + "  ");
                        break;
                    case "GPU temperature":
                        DrawString("T", mon.data + " " + mon.szUnits + "  ");
                        if (_bmpTemperatureGpu == _bmpTemperatureCold && mon.data >= 70)
                        {
                            _bmpTemperatureGpu = _bmpTemperatureHot;
                            SetImagePos(150, 160, _bmpTemperatureGpu.Width, _bmpTemperatureGpu.Height);
                            DrawImage(_bmpTemperatureGpu);
                            //hasTPicChanged = true;
                        }
                        else if (_bmpTemperatureGpu == _bmpTemperatureHot && mon.data <= 60)
                        {
                            _bmpTemperatureGpu = _bmpTemperatureCold;
                            SetImagePos(150, 160, _bmpTemperatureGpu.Width, _bmpTemperatureGpu.Height);
                            DrawImage(_bmpTemperatureGpu);
                            //hasTPicChanged = true;
                        }
                        else if (_bmpTemperatureGpu == _bmpTemperatureHot && mon.data >= 85)
                        {
                            _bmpTemperatureGpu = _bmpTemperatureBlazing;
                            SetImagePos(150, 160, _bmpTemperatureGpu.Width, _bmpTemperatureGpu.Height);
                            DrawImage(_bmpTemperatureGpu);
                            //hasTPicChanged = true;
                        }
                        else if (_bmpTemperatureGpu == _bmpTemperatureBlazing && mon.data <= 80)
                        {
                            _bmpTemperatureGpu = _bmpTemperatureHot;
                            SetImagePos(150, 160, _bmpTemperatureGpu.Width, _bmpTemperatureGpu.Height);
                            DrawImage(_bmpTemperatureGpu);
                            //hasTPicChanged = true;
                        }
                        break;
                    case "Core clock":
                        DrawString("g", mon.data.ToString("0.0") + " " + mon.szUnits + "  ");
                        break;
                    case "Memory clock":
                        DrawString("m", mon.data + " " + mon.szUnits);
                        break;
                    case "Framerate":
                        //float fps = (mon.data < 0.0f || mon.data==float.MaxValue ? 0.0f : (mon.data > 999.0f ? 999.0f : mon.data));
                        var ft = mons.Find(x => x.szSrc == "Frametime");
                        if (ft != null)
                        {
                            DrawString("X", (int)(mon.data < 0.0f || mon.data == float.MaxValue ? 0.0f : (mon.data > 999.0f ? 999.0f : mon.data)) + " fps, " + (int)(ft.data < 0.0f || ft.data == float.MaxValue ? 0.0f : (ft.data > 999.0f ? 999.0f : ft.data)) + " ms");
                        }
                        else
                        {
                            DrawString("X", (int)(mon.data < 0.0f || mon.data == float.MaxValue ? 0.0f : (mon.data > 999.0f ? 999.0f : mon.data)) + " fps");
                        }
                        break;
                    default:
                        break;
                }
                /*if (hasTPicChanged)
                {
                    _timerRefresh.Interval = _timerRefreshInterval;
                    _timerRefresh.Enabled = true;
                    return;
                }*/
            }
        }

        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                Bitmap b = new Bitmap(size.Width, size.Height);
                using (Graphics g = Graphics.FromImage((Image)b))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch
            {
                Console.WriteLine("Bitmap could not be resized");
                return imgToResize;
            }
        }

        void RunRivaDependentSub()
        {
            if (!CheckMFileRiva()) return;
            
            var ac = GetForegroundApp();
            if (ac != null)
            {
                if (ac.szName != _currentProcName)
                {
                    _currentProcName = ac.szName;

                    var proc = Process.GetProcessById((int)(ac.dwProcessID));

                    string title = proc.MainWindowTitle.Trim();
                    DrawTitle(title.Substring(0, title.Length > 50 ? 50 : title.Length));

                    //var bitmap = GetBitmapFromExe(_currentProcName);
                    var bitmap = GetBitmapFromExeScaled(_currentProcName);
                    if (bitmap == null)
                    {
                        SetImagePos(0, 0, _bitmapBlack.Width, _bitmapBlack.Height);
                        DrawImage(_bitmapBlack);
                    }
                    else
                    {
                        SetImagePos(0, 0, bitmap.Width, bitmap.Height);
                        DrawImage(bitmap);
                    }
                }
            }
            else
            {
                if (bFirstStart)
                {
                    bFirstStart = false;
                    SetImagePos(0, 0, _bitmapBlack.Width, _bitmapBlack.Height);
                    DrawImage(_bitmapBlack);
                }
            }

            if (ac == null && _currentProcName != string.Empty)
            {
                _currentProcName = string.Empty;
                DrawTitle(" ");
                SetImagePos(0, 0, _bitmapBlack.Width, _bitmapBlack.Height);
                DrawImage(_bitmapBlack);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            bool portFound = false;

            var args = Environment.GetCommandLineArgs();
            if(args != null && args.Length > 1 )
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if(args[i].ToUpper().StartsWith("-COM"))
                    {
                        _portName = args[i].ToUpper().Substring(1);
                        portFound = true;
                    }
                }
            }
            if(!portFound)
            {
                var list = SerialPort.GetPortNames();
                foreach (var item in list)
                {
                    if ( MessageBox.Show("Use Port '" + item + "' ?", "Choose a port", MessageBoxButtons.YesNo, MessageBoxIcon.Question)==DialogResult.Yes )
                    {
                        _portName = item;
                        portFound = true;
                        break;
                    }
                }
            }

            MessageBox.Show("Using Port '" + _portName + "'", "Chosen port", MessageBoxButtons.OK, MessageBoxIcon.Information);

            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.
            Opacity = 0;

            // Create Notify Icon
            notifyIconMain.Visible = true;
            this.notifyIconMain.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info; //Shows the info icon so the user doesn't thing there is an error.
            this.notifyIconMain.BalloonTipText = "I am now here!";
            this.notifyIconMain.BalloonTipTitle = "Display forwarder";
            IntPtr Hicon = new Bitmap(DisplayForwarder.Properties.Resources.default_icon_small).GetHicon();
            this.notifyIconMain.Icon = Icon.FromHandle(Hicon); //The tray icon to use
            this.notifyIconMain.Text = "Display forwarder";
            //notifyIconMain.ShowBalloonTip(500);

            Init();
        }

        Bitmap _bmpPower = null;
        Bitmap _bmpRocket = null;
        Bitmap _bmpTemperatureCold = null;
        Bitmap _bmpTemperatureHot = null;
        Bitmap _bmpTemperatureBlazing = null;
        Bitmap _bmpTemperatureGpu = null;
        Bitmap _bmpTemperatureCpu = null;
        Bitmap _bmpMonitor = null;

        private void InitPort()
        {
            try
            {
                _port = new SerialPort(_portName, 115200);
                _port.Handshake = Handshake.None;
                _port.NewLine = "\r";
                _port.Open();
                _port.NewLine = "\r";
                _port.Handshake = Handshake.None;
            }
            catch (Exception xcp)
            {
                _port = null;
            }

            // Get Protocol Version
            _port.Write("?\r");
            string version = _port.ReadLine();

            version = version.Substring(version.LastIndexOf("V") + 1);
            uint.TryParse(version, out _protVersion);

            DateTime now = DateTime.Now;
            DrawString("t", string.Format("{0:D2}:{1:D2}:{2:D2}", now.Hour, now.Minute, now.Second));
        }

        private void Init()
        {
            _currentProcName = string.Empty;
            _mfBurner = null;
            _acBurner = null;
            _mfRiva = null;
            _acRiva = null;
            bFirstStart = true;


            //// Create a black bitmap
            //_bitmapBlack = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            //using (Graphics gfx = Graphics.FromImage(_bitmapBlack))
            //using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 0, 0)))
            //{
            //    gfx.FillRectangle(brush, 0, 0, _bitmapBlack.Width, _bitmapBlack.Height);
            //}

           

            // Create performance counters
            if(_pcNames==null) CpuCountersCreate(out _pcNames);

            //CpuCountersCreate(out _pc);
            //CpuCountersCreate(out _pc, out _cat);

            // Create CPU temperature object
            _cpuTemp = new Temperature();

            // Load resources
            _bmpPower = new Bitmap(DisplayForwarder.Properties.Resources.power4, 32, 32);
            _bmpRocket = new Bitmap(DisplayForwarder.Properties.Resources.rocket, 32, 32);
            _bmpTemperatureCold = new Bitmap(DisplayForwarder.Properties.Resources.temperature_cold2_small);
            _bmpTemperatureHot = new Bitmap(DisplayForwarder.Properties.Resources.temperature_hot2_small);
            _bmpTemperatureBlazing = new Bitmap(DisplayForwarder.Properties.Resources.temperature_blazing_small);
            _bmpTemperatureGpu = _bmpTemperatureCold;
            _bmpTemperatureCpu = _bmpTemperatureCold;
            _bmpMonitor = new Bitmap(DisplayForwarder.Properties.Resources.monitor_small);
            _bitmapBlack = new Bitmap(DisplayForwarder.Properties.Resources.default_icon_small);


            // open com port
            InitPort();
            if (_port != null)
            {
                SetImagePos(150, 60, _bmpPower.Width, _bmpPower.Height);
                DrawImage(_bmpPower);

                SetImagePos(150, 100, _bmpRocket.Width, _bmpRocket.Height);
                DrawImage(_bmpRocket);

                SetImagePos(150, 160, _bmpTemperatureCold.Width, _bmpTemperatureCold.Height);
                DrawImage(_bmpTemperatureCold);

                SetImagePos(0, 160, _bmpTemperatureCold.Width, _bmpTemperatureCold.Height);
                DrawImage(_bmpTemperatureCold);

                SetImagePos(150, 200, (ushort)(_bmpMonitor.Width), (ushort)(_bmpMonitor.Height));
                DrawImage(_bmpMonitor);
            }

            // Create timer
            if (_timerRefresh == null)
            {
                _timerRefresh = new System.Windows.Forms.Timer();
                _timerRefresh.Tick += _timerRefresh_Tick;
                _timerRefresh.Interval = _timerRefreshInterval;
                _timerRefresh.Enabled = true;
            }
        }

        private void InitializeDisplay()
        {
            if (_port != null)
            {
                SetImagePos(150, 60, _bmpPower.Width, _bmpPower.Height);
                DrawImage(_bmpPower);

                SetImagePos(150, 100, _bmpRocket.Width, _bmpRocket.Height);
                DrawImage(_bmpRocket);

                SetImagePos(150, 160, _bmpTemperatureCold.Width, _bmpTemperatureCold.Height);
                DrawImage(_bmpTemperatureCold);

                SetImagePos(150, 200, (ushort)(_bmpMonitor.Width), (ushort)(_bmpMonitor.Height));
                DrawImage(_bmpMonitor);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIconMain.Visible = true;
                notifyIconMain.ShowBalloonTip(500);
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                notifyIconMain.Visible = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.BringToFront();
            this.Focus();
        }

        private void menuItemShow_Click(object sender, EventArgs e)
        {
            this.Show();
            this.BringToFront();
            this.Focus();
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIconMain = null;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
