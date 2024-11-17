using J3DModelViewer.Properties;
using J3DModelViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using WiiExplorer;
using System.Windows.Forms;
using System.Windows.Controls;
using Hack.io.Util;
using System.Drawing;
using System.Windows.Media;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics;

namespace J3DModelViewer.View
{
    /// <summary>
    /// Interaction logic for InfoTab.xaml
    /// </summary>
    public partial class ExportOptions : System.Windows.Controls.UserControl
    {

        public static ExportOptions INSTANCE;

        public ExportOptions()
        {
            InitializeComponent();

            INSTANCE = this;

            blenderPath.Text = Settings.Default.BlenderPath;
            scriptPath.Text = Settings.Default.ScriptPath;
            upscalingPath.Text = Settings.Default.UpscalingPath;
            upscaleImages.IsChecked = Settings.Default.UpscaleTextures;
            exportAnimations.IsChecked = Settings.Default.ExportAnimations;
            exportTextures.IsChecked = Settings.Default.ExportTextures;
        }

        public static void SaveSettings()
        {
            Settings.Default.BlenderPath = INSTANCE.blenderPath.Text;
            Settings.Default.ScriptPath = INSTANCE.scriptPath.Text;
            Settings.Default.UpscalingPath = INSTANCE.upscalingPath.Text;
            Settings.Default.UpscaleTextures = (bool)INSTANCE.upscaleImages.IsChecked;
            Settings.Default.ExportAnimations = (bool)INSTANCE.exportAnimations.IsChecked;
            Settings.Default.ExportTextures = (bool)INSTANCE.exportTextures.IsChecked;

            Settings.Default.Save();
        }

        public static string GetBlenderPath()
        {
            return INSTANCE.blenderPath.Text;
        }

        public static string GetScriptPath()
        {
            return INSTANCE.scriptPath.Text;
        }

        public static string GetUpscalingPath()
        {
            return INSTANCE.upscalingPath.Text;
        }

        public static bool GetUpscaleImages()
        {
            return (bool)INSTANCE.upscaleImages.IsChecked;
        }

        public static bool GetExportAnimations()
        {
            return (bool)INSTANCE.exportAnimations.IsChecked;
        }

        public static bool GetExportTextures()
        {
            return (bool)INSTANCE.exportTextures.IsChecked;
        }
    }
}
