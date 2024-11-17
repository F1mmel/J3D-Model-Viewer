using J3DModelViewer.View;
using J3DModelViewer.ViewModel;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindEditor;
using System.Windows.Interop;

namespace J3DModelViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel m_viewModel;

        public MainWindow()
        {
            //FileAssociationHelper.AssociateFileExtension(".arc", "ArcViewer", "View arc files", System.Windows.Forms.Application.ExecutablePath);



            InitializeComponent();

            Process instance = CheckInstancesFromRunningProcesses();
            if (instance != null)
            {
                string[] args = Environment.GetCommandLineArgs();
                string correctArg = "";

                if (args.Length >= 1)
                {
                    foreach (string a in args)
                    {
                        if (a.EndsWith(".arc"))
                        {
                            correctArg = a;
                            break;
                        }
                    }

                    SendDataMessage(instance, correctArg);
                    //System.Windows.Forms.MessageBox.Show("Sendd: " + correctArg + " to " + instance.ProcessName);

                }


                Environment.Exit(1);
            }

            Closing += OnWindowClosing;

            this.tabArcViewer.Focus();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            foreach(string folder in ArcImporter.folders)
            {
                if(Directory.Exists(folder))
                {
                    Console.WriteLine("CleanUp: " + folder);
                    Directory.Delete(folder, true);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ColorFormat cF = new ColorFormat(8, 8, 8, 8);
            GraphicsMode gm = new GraphicsMode(cF, 24, 0, 0);
            var glControl = new GLControl(gm);
            glControlHost.Child = glControl;
            glControlHost.SizeChanged += GlControlHost_SizeChanged;
            glControlHost.PreviewKeyDown += GlControlHost_PreviewKeyDown;
            glControlHost.PreviewKeyUp += GlControlHost_PreviewKeyUp;
            glControlHost.Child.MouseDown += GlControlHost_MouseDown;
            glControlHost.Child.MouseUp += GlControlHost_MouseUp;
            glControlHost.Child.MouseWheel += GlControlHost_MouseWheel;

            m_viewModel = (MainWindowViewModel)DataContext;
            m_viewModel.OnMainEditorWindowLoaded((GLControl)glControlHost.Child);
        }

        private void GlControlHost_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            WInput.SetMouseState(WinFormToWPFMouseButton(e), false);
        }

        private void GlControlHost_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            WInput.SetMouseState(WinFormToWPFMouseButton(e), true);
        }

        private void GlControlHost_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            WInput.SetMouseScrollDelta(e.Delta);
        }

        private void GlControlHost_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            WInput.SetKeyboardState(e.Key, false);
        }

        private void GlControlHost_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            WInput.SetKeyboardState(e.Key, true);
        }

        private void GlControlHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            m_viewModel.OnViewportResized((int)e.NewSize.Width, (int)e.NewSize.Height);
        }

        private static System.Windows.Input.MouseButton WinFormToWPFMouseButton(System.Windows.Forms.MouseEventArgs e)
        {
            System.Windows.Input.MouseButton btn = System.Windows.Input.MouseButton.Left;
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Left:
                    btn = System.Windows.Input.MouseButton.Left;
                    break;
                case System.Windows.Forms.MouseButtons.Middle:
                    btn = System.Windows.Input.MouseButton.Middle;
                    break;
                case System.Windows.Forms.MouseButtons.Right:
                    btn = System.Windows.Input.MouseButton.Right;
                    break;
                case System.Windows.Forms.MouseButtons.XButton1:
                    btn = System.Windows.Input.MouseButton.XButton1;
                    break;
                case System.Windows.Forms.MouseButtons.XButton2:
                    btn = System.Windows.Input.MouseButton.XButton2;
                    break;
            }

            return btn;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] droppedFilePaths = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                if(droppedFilePaths.Length > 0)
                {
                    m_viewModel.OnFilesDropped(droppedFilePaths);
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            string correctArg = "";

            if (args.Length >= 1)
            {
                foreach (string a in args)
                {
                    if (a.EndsWith(".arc"))
                    {
                        correctArg = a;
                        break;
                    }
                }

                Console.WriteLine("Load arc: " + correctArg);
                ArcImporter.INSTANCE.LoadArcFile(correctArg);
            }
        }

        private Process CheckInstancesFromRunningProcesses()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] allProcesses = Process.GetProcessesByName(currentProcess.ProcessName);

            if (allProcesses.Length > 1)
            {
                return allProcesses[0];
            }

            return null;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_COPYDATA)
            {
                //Reconstruct copy data structure
                COPYDATASTRUCT _dataStruct = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);

                //Get the messag (file name we sent from the other instance)
                string _strMsg = Marshal.PtrToStringUni(_dataStruct.lpData, _dataStruct.cbData / 2);

                ArcImporter.INSTANCE.LoadArcFile(_strMsg);
            }


            return IntPtr.Zero;
        }

        const int WM_COPYDATA = 0x004A;

        [DllImport("user32", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr Hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        //.....

        [StructLayout(LayoutKind.Sequential)]
        struct COPYDATASTRUCT
        {
            public IntPtr dwData;    // Any value the sender chooses. Perhaps its main window handle?
            public int cbData;       // The count of bytes in the message.
            public IntPtr lpData;    // The address of the message.
        }

        public static void SendDataMessage(Process targetProcess, string msg)
        {
            //Copy the string message to a global memory area in unicode format
            IntPtr _stringMessageBuffer = Marshal.StringToHGlobalUni(msg);

            //Prepare copy data structure
            COPYDATASTRUCT _copyData = new COPYDATASTRUCT();
            _copyData.dwData = IntPtr.Zero;
            _copyData.lpData = _stringMessageBuffer;
            _copyData.cbData = msg.Length * 2;   //Number of bytes required for marshalling 
                                                 //this string as a series of unicode characters
            IntPtr _copyDataBuff = IntPtrAlloc(_copyData);

            //Send message to the other process
            SendMessage(targetProcess.MainWindowHandle, WM_COPYDATA, IntPtr.Zero, _copyDataBuff);

            Marshal.FreeHGlobal(_copyDataBuff);
            Marshal.FreeHGlobal(_stringMessageBuffer);
        }

        public static IntPtr IntPtrAlloc<T>(T param)
        {
            IntPtr retval = Marshal.AllocHGlobal(Marshal.SizeOf(param));
            Marshal.StructureToPtr(param, retval, false);
            return retval;
        }
    }
}
