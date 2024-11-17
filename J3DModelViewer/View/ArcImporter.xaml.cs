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
using Microsoft.VisualBasic;
using JStudio;
using System.Text.RegularExpressions;
using GameFormatReader.Common;
using JStudio.J3D;
using WindEditor;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using Xceed.Wpf.Toolkit;
using Microsoft.Win32;
using J3DModelViewer.View;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties;
using System.Reflection;

namespace J3DModelViewer.View
{
    /// <summary>
    /// Interaction logic for InfoTab.xaml
    /// </summary>
    public partial class ArcImporter : System.Windows.Controls.UserControl
    {
        Archive Archive;

        public static List<string> folders = new List<string>();
        private static string currentFolder;
        private static string arcPath;
        private static string arcName;
        public static string currentBMD = "";
        private static bool noItemSelected = true;

        private static BackgroundWorker worker;

        public static ArcImporter INSTANCE;

        public ArcImporter()
        {
            InitializeComponent();

            INSTANCE = this;

            return;

            // DEMO
            
             foreach (string arcFile in Directory.GetFiles("C:\\Users\\finno\\Desktop\\Extracted\\res\\Object"))
            {
                //if (arcFile.StartsWith("E")) continue;

                Console.WriteLine("EXTRACING: " + arcFile);
                OpenArchive(arcFile);

                string targetDir = "C:\\Users\\finno\\Desktop\\Extracted\\res\\Object\\Gates";
                string search = "Ga";

                if (Directory.Exists(currentFolder + "/bmdr"))
                {
                    foreach (string file in Directory.GetFiles(currentFolder + "/bmdr"))
                    {
                        Console.WriteLine(file);
                        if (Path.GetFileNameWithoutExtension(file).Contains(search))
                        {
                            Console.WriteLine(file);
                            //Directory.CreateDirectory(targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")));
                            //File.Copy(file, targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")) + "/" + Path.GetFileName(file), true);
                            //File.Copy(file, targetDir + "/" + Path.GetFileName(file), true
                            //
                        }
                    }
                }

                if (Directory.Exists(currentFolder + "/bmdv"))
                {
                    foreach (string file in Directory.GetFiles(currentFolder + "/bmdv"))
                    {
                        Console.WriteLine(file);
                        if (Path.GetFileNameWithoutExtension(file).Contains(search))
                        {
                            Console.WriteLine(file);
                            //Directory.CreateDirectory(targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")));
                            //File.Copy(file, targetDir + "/" + Path.GetFileName(file), true);
                            //File.Copy(file, targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")) + "/" + Path.GetFileName(file), true);
                        }
                    }
                }

                if (Directory.Exists(currentFolder + "/bmwr"))
                {
                    foreach (string file in Directory.GetFiles(currentFolder + "/bmwr"))
                    {
                        Console.WriteLine(file);
                        if (Path.GetFileNameWithoutExtension(file).Contains(search))
                        {
                            Console.WriteLine(file);
                            //Directory.CreateDirectory(targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")));
                            //File.Copy(file, targetDir + "/" + Path.GetFileName(file), true);
                            //File.Copy(file, targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")) + "/" + Path.GetFileName(file), true);
                        }
                    }
                }

                /*List<string> models = GetModels();
                foreach(string model in models)
                {
                    Console.WriteLine(model);
                    if (Path.GetFileNameWithoutExtension(model).Contains("bri"))
                    {
                        Console.WriteLine(model);
                        //Directory.CreateDirectory(targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")));
                        File.Copy(model, targetDir + "/" + Path.GetFileName(model), true);
                        //File.Copy(file, targetDir + "/" + Path.GetFileName(arcFile.Replace(".arc", "")) + "/" + Path.GetFileName(file), true);
                    }
                }*/
        }
    }

        private List<string> GetModels()
        {
            List<string> models = new List<string>();

            Console.WriteLine("FOLD1: " + currentFolder);
            foreach (string folder in Directory.GetFiles(currentFolder + "/bmdr"))
            {
                Console.WriteLine("FOLD: " + folder);
                if (Path.GetFileName(folder).Contains("model"))
                {
                    foreach(string model in Directory.GetFiles(folder))
                    {
                       models.Add(model);
                    }
                }
            }

            return models;
        }

        // Handle dropped arc file
        private void Grid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop, true))
            {
                string[] droppedFilePaths = e.Data.GetData(System.Windows.DataFormats.FileDrop, true) as string[];
                if (droppedFilePaths.Length > 0)
                {
                    LoadArcFile(droppedFilePaths[0]);
                }
            }
        }

        public void LoadArcFile(string droppedFilePaths)
        {
            //Dispatcher.BeginInvoke(new Action(() => { status.Content = "Extracting archive..."; }));
            if (Path.GetExtension(droppedFilePaths).Equals(".arc"))
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    status.Content = "Extracting archive...";
                    this.arcStatus.Content = "Processing...";
                    this.Canvas.Visibility = Visibility.Visible;
                }));

                BinaryTextureImage.Copy = true;
                arcPath = droppedFilePaths;
                arcName = Path.GetFileName(arcPath);
                OpenArchive(droppedFilePaths);
            }

            this.contentTree.Background = new SolidColorBrush(Colors.White);
            this.arcStatus.Content = "";
            this.Canvas.Visibility = Visibility.Hidden;

            ExportOptions.SaveSettings();
        }

        // Handle grid enter
        private void Grid_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            this.contentTree.Background = new SolidColorBrush(Colors.Gray);
            this.arcStatus.Content = "Drop .arc file";
            this.Canvas.Visibility = Visibility.Visible;
        }

        // Handle grid leave
        private void Grid_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            this.contentTree.Background = new SolidColorBrush(Colors.White);
            this.arcStatus.Content = "";
            this.Canvas.Visibility = Visibility.Hidden;
        }

        // Handle scroll on tree
        private void ListViewScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        // Handle available options based on clicked node
        private void Handle_AvailableOptions(object sender, ContextMenuEventArgs e)
        {
            // Current item
            string clickedNode = contentTree.SelectedItem + "";

            ContextMenu.Items.Clear();

            if (noItemSelected)
            {
                ContextMenu.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                ContextMenu.Visibility = Visibility.Visible;
            }

            if (clickedNode.Contains(".bck"))
            {
                // Single animation file
                exportAnimation.Click -= HandleClick_ExportAnimation;
                exportAnimation.Click += HandleClick_ExportAnimation;
                ContextMenu.Items.Add(exportAnimation);
            }
            else if (clickedNode.Contains("bck"))
            {
                // Animations files
                exportAnimations.Click -= HandleClick_ExportAnimations;
                exportAnimations.Click += HandleClick_ExportAnimations;
                ContextMenu.Items.Add(exportAnimations);
            }

            if (clickedNode.Contains(".bmd"))
            {
                // Single FBX or BMD file
                exportFbx.Click -= HandleClick_ExportFbx;
                exportFbx.Click += HandleClick_ExportFbx;
                ContextMenu.Items.Add(exportFbx);

                exportBmd.Click -= HandleClick_ExportBmd;
                exportBmd.Click += HandleClick_ExportBmd;
                ContextMenu.Items.Add(exportBmd);
            }
            else if (clickedNode.Contains("bm"))
            {
                // FBX or BMD files
                exportAllFbx.Click -= HandleClick_ExportAllFbx;
                exportAllFbx.Click += HandleClick_ExportAllFbx;
                ContextMenu.Items.Add(exportAllFbx);

                exportAllBmd.Click -= HandleClick_ExportAllBmd;
                exportAllBmd.Click += HandleClick_ExportAllBmd;
                ContextMenu.Items.Add(exportAllBmd);
            }
        }

        // Handle available options based on clicked node
        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tvItem = e.Source as TreeViewItem;
            if (tvItem != null)
            {
                tvItem.IsSelected = true;
                noItemSelected = false;
                e.Handled = true;

                return;
            }
            else
            {
                e.Handled = false;
                noItemSelected = true;

                return;
            }
        }

        // Export all as FBX
        private void HandleClick_ExportAllFbx(object sender, EventArgs e)
        {
            ExportOptions.SaveSettings();

            if (File.Exists(ExportOptions.GetBlenderPath()) &&
                 File.Exists(ExportOptions.GetScriptPath()))
            {

            }
            else
            {
                System.Windows.MessageBox.Show("Either Blender or IOManager path not set");
                return;
            }

            TreeViewItem selected = contentTree.ItemContainerGenerator.ContainerFromItemRecursive(contentTree.SelectedItem);
            string treePath = GetFullPath(selected).Split(new string[] { "arc" }, StringSplitOptions.None)[1];

            string input = "";
            ShowInputDialogBox(ref input, "Name:", "Export", 200, 85);
            if (input.Equals("")) return;

            string bmdFile = currentFolder + treePath;
            foreach (string file in Directory.GetFiles(bmdFile))
            {
                //ExportFile(input, bmdFile);
                List<object> parameter = new List<object>();
                parameter.Add(input);
                parameter.Add(bmdFile);

                this.contentTree.IsEnabled = false;
                status.Content = "Exporting file as fbx...";
                ExportFile(input, file);

                // Convert bmd to fbx using Blemd
            }

            this.contentTree.IsEnabled = true;
            status.Content = "Exported files as fbx";
        }

        // Export as FBX
        private void HandleClick_ExportFbx(object sender, EventArgs e)
        {
            ExportOptions.SaveSettings();

            TreeViewItem selected = contentTree.ItemContainerGenerator.ContainerFromItemRecursive(contentTree.SelectedItem);
            string treePath = GetFullPath(selected).Split(new string[] { "arc" }, StringSplitOptions.None)[1];

            string input = "";
            ShowInputDialogBox(ref input, "Name:", "Export", 200, 85);
            if (input.Equals("")) return;

            string bmdFile = currentFolder + treePath;

            this.contentTree.IsEnabled = false;
            status.Content = "Exporting file as fbx...";

            ExportFileAsFbx(input, bmdFile);
        }

        // Export as BMD !!!!!!!!!!!!
        private void HandleClick_ExportBmd(object sender, EventArgs e)
        {
            ExportOptions.SaveSettings();

            if (Directory.Exists(ExportOptions.GetBlenderPath()) &&
                 File.Exists(ExportOptions.GetScriptPath()))
            {

            }
            else
            {
                System.Windows.MessageBox.Show("Either Blender or IOManager path not set");
                return;
            }

            TreeViewItem selected = contentTree.ItemContainerGenerator.ContainerFromItemRecursive(contentTree.SelectedItem);
            string treePath = GetFullPath(selected).Split(new string[] { "arc" }, StringSplitOptions.None)[1];

            string input = "";
            ShowInputDialogBox(ref input, "Name:", "Export", 200, 85);
            if (input.Equals("")) return;

            string bmdFile = currentFolder + treePath;

            this.contentTree.IsEnabled = false;
            status.Content = "Exporting file as BMD...";

            ExportFile(input, bmdFile);
        }

        // Export all as BMD
        private void HandleClick_ExportAllBmd(object sender, EventArgs e)
        {
            ExportOptions.SaveSettings();

            TreeViewItem selected = contentTree.ItemContainerGenerator.ContainerFromItemRecursive(contentTree.SelectedItem);
            string treePath = GetFullPath(selected).Split(new string[] { "arc" }, StringSplitOptions.None)[1];

            string input = "";
            ShowInputDialogBox(ref input, "Name:", "Export", 200, 85);
            if (input.Equals("")) return;

            string bmdFile = currentFolder + treePath;

            foreach (string file in Directory.GetFiles(bmdFile))
            {
                this.contentTree.IsEnabled = false;
                status.Content = "Exporting file as bmd...";

                ExportFile(input, file);
            }
        }

        // Export all animations
        private void HandleClick_ExportAnimations(object sender, EventArgs e)
        {
            ExportOptions.SaveSettings();

            TreeViewItem selected = contentTree.ItemContainerGenerator.ContainerFromItemRecursive(contentTree.SelectedItem);
            string treePath = GetFullPath(selected).Split(new string[] { "arc" }, StringSplitOptions.None)[1];
            string bmdFile = currentFolder + treePath;

            if (treePath.EndsWith("bck"))
            {
                string input = "";
                var ofd = new CommonOpenFileDialog();
                ofd.Title = "Choose root directory";
                ofd.InitialDirectory = Directory.GetParent(arcPath).FullName;
                ofd.IsFolderPicker = true;
                ofd.AddToMostRecentlyUsedList = false;
                ofd.AllowNonFileSystemItems = false;
                ofd.EnsureFileExists = true;
                ofd.EnsurePathExists = true;
                ofd.EnsureReadOnly = false;
                ofd.EnsureValidNames = true;
                ofd.Multiselect = false;
                ofd.ShowPlacesList = true;

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string rootDir = ofd.FileName;
                    if (!Directory.Exists(rootDir + "/bck/"))
                        Directory.CreateDirectory(rootDir + "/bck/");

                    //ExportFile(input, bmdFile);
                    List<object> parameter = new List<object>();
                    parameter.Add(input);
                    parameter.Add(bmdFile);

                    this.contentTree.IsEnabled = false;
                    status.Content = "Exporting animations...";

                    foreach (string animFile in Directory.GetFiles(bmdFile))
                    {
                        File.Copy(animFile, rootDir + "/bck/" + Path.GetFileName(animFile), true);
                    }

                    this.contentTree.IsEnabled = true;
                    status.Content = "Exported animations";
                }
            }
        }

        // Export animation
        private void HandleClick_ExportAnimation(object sender, EventArgs e)
        {
            ExportOptions.SaveSettings();

            TreeViewItem selected = contentTree.ItemContainerGenerator.ContainerFromItemRecursive(contentTree.SelectedItem);
            string treePath = GetFullPath(selected).Split(new string[] { "arc" }, StringSplitOptions.None)[1];
            string bmdFile = currentFolder + treePath;

            if (treePath.EndsWith(".bck"))
            {
                string input = "";
                var ofd = new CommonOpenFileDialog();
                ofd.Title = "Choose root directory";
                ofd.InitialDirectory = Directory.GetParent(arcPath).FullName;
                ofd.IsFolderPicker = true;
                ofd.AddToMostRecentlyUsedList = false;
                ofd.AllowNonFileSystemItems = false;
                ofd.EnsureFileExists = true;
                ofd.EnsurePathExists = true;
                ofd.EnsureReadOnly = false;
                ofd.EnsureValidNames = true;
                ofd.Multiselect = false;
                ofd.ShowPlacesList = true;

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string rootDir = ofd.FileName;
                    if (!Directory.Exists(rootDir + "/bck/"))
                        Directory.CreateDirectory(rootDir + "/bck/");

                    //ExportFile(input, bmdFile);
                    List<object> parameter = new List<object>();
                    parameter.Add(input);
                    parameter.Add(bmdFile);

                    this.contentTree.IsEnabled = false;
                    status.Content = "Exporting animations...";

                    File.Copy(bmdFile, rootDir + "/bck/" + Path.GetFileName(bmdFile), true);

                    this.contentTree.IsEnabled = true;
                    status.Content = "Exported animation";
                }
            }
        }

        // Copy file from archive to output directory
        private string ExportFile(string input, string bmdFile, string originalName = "")
        {
            ExportOptions.SaveSettings();

            if (ExportOptions.GetUpscaleImages() && ExportOptions.GetUpscalingPath().Equals(""))
            {
                System.Windows.MessageBox.Show("ESRGAN Upscaling path not set");
                return "";
            }

            // Should export animations?
            if (ExportOptions.GetExportAnimations())
            {
                string animationFolder = currentFolder + "\\bck";
                if (Directory.Exists(animationFolder))
                {
                    Console.WriteLine("Found animation directory");

                    string outDirectory = Directory.GetParent(arcPath).FullName + "/" + input;
                    Directory.CreateDirectory(outDirectory);

                    // Create output animation directory
                    Directory.CreateDirectory(outDirectory + "/bck");

                    // Copy animations in output folder
                    foreach (string animationFile in Directory.GetFiles(animationFolder))
                    {
                        string originalFileName = Path.GetFileName(bmdFile).Replace(".bmd", "");
                        originalFileName = Regex.Replace(originalFileName, "d-]", string.Empty).Replace("_", "");
                        string animFile = Regex.Replace(animationFile, @"[\d-]", string.Empty).Replace("_", "");

                        // Belongs animation to model?
                        if (animFile.Contains(originalFileName))
                        {
                            File.Copy(animationFile, outDirectory + "/bck/" + Path.GetFileName(animationFile), true);
                        }
                    }

                    // Create output bmd directory
                    Directory.CreateDirectory(outDirectory + "/bmd");
                    if (originalName.Equals(""))
                        File.Copy(bmdFile, outDirectory + "/bmd/" + input + ".bmd", true);
                    else
                        File.Copy(bmdFile, outDirectory + "/bmd/" + originalName + ".bmd", true);
                }
                else
                {
                    Console.WriteLine("No animation found!");

                    string outDirectory = Directory.GetParent(arcPath).FullName + "/" + input;
                    Directory.CreateDirectory(outDirectory);
                    Directory.CreateDirectory(outDirectory + "/bmd/");
                    // Copy bmd file to arc location
                    File.Copy(bmdFile, Directory.GetParent(arcPath).FullName + "/" + input + "/bmd/" + input + ".bmd", true);
                }
            }


            string dir = Directory.GetParent(arcPath).FullName + "/" + input;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(dir + "/bmd/");
            }

            // Should export texture?
            if (ExportOptions.GetExportTextures())
            {
                // Should upscale textures?
                if (!ExportOptions.GetUpscaleImages())
                {
                    string textureOut = Directory.GetParent(arcPath).FullName + "/" + input;
                    var newModel = new J3D(Path.GetFileName(bmdFile));
                    EndianBinaryReader reader = new EndianBinaryReader(new FileStream(bmdFile, FileMode.Open, FileAccess.Read), Endian.Big);
                    // Extract textures to bmd location
                    newModel.LoadTextures(reader, textureOut + "/bmd");
                    reader.Close();

                    return Directory.GetParent(arcPath).FullName + "/" + input + "/bmd/" + input + ".bmd";
                }
                else
                {
                    if (!Directory.Exists(ExportOptions.GetUpscalingPath() + "/input"))
                        Directory.CreateDirectory(ExportOptions.GetUpscalingPath() + "/input");
                    else
                    {
                        // Clean up files
                        DirectoryInfo d = new DirectoryInfo(ExportOptions.GetUpscalingPath() + "/input");
                        foreach (FileInfo file in d.GetFiles())
                        {
                            file.Delete();
                        }
                    }

                    string textureOut = Directory.GetParent(arcPath).FullName + "/" + input;
                    var newModel = new J3D(Path.GetFileName(bmdFile));
                    EndianBinaryReader reader = new EndianBinaryReader(new FileStream(bmdFile, FileMode.Open, FileAccess.Read), Endian.Big);
                    // Extract textures to ESRGAN locatio
                    newModel.LoadTextures(reader, ExportOptions.GetUpscalingPath() + "/input");
                    reader.Close();

                    if (!ExportOptions.GetExportAnimations())
                        File.Copy(bmdFile, Directory.GetParent(arcPath).FullName + "/" + input + "/bmd/" + input + ".bmd", true);

                    List<string> arguments = new List<string>();
                    arguments.Add(ExportOptions.GetUpscalingPath());
                    arguments.Add(textureOut);

                    worker = new BackgroundWorker();
                    worker.DoWork += UpscalingImages;
                    worker.ProgressChanged += backgroundWorker1_ProgressChanged;
                    worker.WorkerReportsProgress = true;
                    worker.RunWorkerAsync(arguments);

                    // Upscale images
                    /*int count = 0;
                    foreach (string lowRes in Directory.GetFiles(ExportOptions.GetUpscalingPath() + "/input"))
                    {
                        count++;

                        /*Dispatcher.BeginInvoke(new Action(() => {
                            canvasBar.Visibility = Visibility.Visible;
                            upscaleBar.Value = count;
                            upscaleText.Text = "Texture " + count + " / " + upscaleBar.Maximum;
                        }));*/
                    /*ProcessStartInfo ai = new ProcessStartInfo();
                    ai.WorkingDirectory = ExportOptions.GetUpscalingPath();
                    ai.FileName = ExportOptions.GetUpscalingPath() + "/Real-ESRGAN.bat";
                    ai.Arguments = "\"input/" + Path.GetFileName(lowRes) + "\"";
                    ai.UseShellExecute = false;
                    ai.RedirectStandardOutput = true;
                    ai.CreateNoWindow = true;

                    using (Process process = Process.Start(ai))
                    {
                        using (StreamReader r = process.StandardOutput)
                        {
                            string output = r.ReadToEnd();
                            Console.WriteLine("OUTDavor: " + output);
                            Dispatcher.BeginInvoke(new Action(() => {
                                Console.WriteLine("OUT: " + output);
                            }));

                            //Dispatcher.BeginInvoke(new Action(() => { worker.ReportProgress(0); }));
                        }
                    }
                }*/

                    /*Console.WriteLine("Upscaling finished");

                    // Copy upscaled textures back
                    foreach (string highRes in Directory.GetFiles(ExportOptions.GetUpscalingPath() + "/input"))
                    {
                        string fileName = Path.GetFileName(highRes);

                        if (fileName.EndsWith("_4x.png"))
                        {
                            File.Copy(highRes, textureOut + "/bmd/" + fileName.Replace("_4x", ""), true);
                        }
                    }

                    // Clean up files
                    DirectoryInfo diLR = new DirectoryInfo(ExportOptions.GetUpscalingPath() + "/input");
                    foreach (FileInfo file in diLR.GetFiles())
                    {
                        file.Delete();
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        INSTANCE.contentTree.IsEnabled = true;
                        status.Content = "Exported";
                    }));*/
                }
            }

            if (!ExportOptions.GetExportAnimations() && !ExportOptions.GetExportTextures())
            {
                File.Copy(bmdFile, Directory.GetParent(arcPath).FullName + "/" + input + ".bmd", true);
            }

            return Directory.GetParent(arcPath).FullName + "/" + input + ".bmd";
        }

        // Copy file from archive to output directory
        private void ExportFileAsFbx(string input, string bmdFile, string originalName = "")
        {
            string bmdPath = ExportFile(input, bmdFile);
            string converter = ExportOptions.GetScriptPath();

            // In "UpscalineImages" schieben, nach Fertig?

            using (Process sortProcess = new Process())
            {
                sortProcess.StartInfo.WorkingDirectory = (Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\resources\\").Replace("\\", "/");
                sortProcess.StartInfo.FileName = converter;
                //sortProcess.StartInfo.FileName = upscalingPath + "/realesrgan-ncnn-vulkan.exe";
                sortProcess.StartInfo.Arguments = "\"" + bmdPath.Replace("\\", "/") + "\"";
                sortProcess.StartInfo.CreateNoWindow = false;
                sortProcess.StartInfo.UseShellExecute = false;
                sortProcess.StartInfo.RedirectStandardOutput = true;
                // Start the process
                sortProcess.Start();

                sortProcess.WaitForExit();
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                INSTANCE.contentTree.IsEnabled = true;
                status.Content = "Exported";
            }));
        }

        // Invoke upscaling ai
        private void UpscalingImages(object sender, DoWorkEventArgs e)
        {
            List<string> arguments = (List<string>)e.Argument;

            string upscalingPath = arguments[0];
            string textureOut = arguments[1];

            // Upscale images
            int count = 0;
            int length = 0;
            foreach (string lowRes in Directory.GetFiles(upscalingPath + "/input"))
            {
                if (length == 0)
                {
                    length = Directory.GetFiles(upscalingPath + "/input").Length;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        canvasBar.Visibility = Visibility.Visible;
                        upscaleBar.Value = count;
                        upscaleBar.Maximum = length;
                        upscaleText.Text = "Texture " + count + " / " + length;
                    }));
                }
                worker.ReportProgress(count);
                count++;

                using (Process sortProcess = new Process())
                {
                    sortProcess.StartInfo.WorkingDirectory = upscalingPath;
                    sortProcess.StartInfo.FileName = upscalingPath + "/Real-ESRGAN.bat";
                    //sortProcess.StartInfo.FileName = upscalingPath + "/realesrgan-ncnn-vulkan.exe";
                    sortProcess.StartInfo.Arguments = "\"input/" + Path.GetFileName(lowRes) + "\"";
                    //sortProcess.StartInfo.Arguments = "-i \"" + lowRes + "\" -o \"" + lowRes + "_scaled.png\" -n realesrgan-x4plus";
                    sortProcess.StartInfo.CreateNoWindow = true;
                    sortProcess.StartInfo.UseShellExecute = false;
                    sortProcess.StartInfo.RedirectStandardOutput = true;

                    // Start the process
                    sortProcess.Start();

                    sortProcess.WaitForExit();
                }
            }

            Console.WriteLine("Upscaling finished");

            // Copy upscaled textures back
            foreach (string highRes in Directory.GetFiles(upscalingPath + "/input"))
            {
                string fileName = Path.GetFileName(highRes);

                if (fileName.EndsWith("_4x.png"))
                {
                    File.Copy(highRes, textureOut + "/bmd/" + fileName.Replace("_4x", ""), true);
                }
            }

            // Clean up files
            DirectoryInfo diLR = new DirectoryInfo(upscalingPath + "/input");
            foreach (FileInfo file in diLR.GetFiles())
            {
                file.Delete();
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                contentTree.IsEnabled = true;
                status.Content = "Exported";

                canvasBar.Visibility = Visibility.Hidden;
                upscaleBar.Value = 0;
                upscaleBar.Maximum = 0;
                upscaleText.Text = "";
            }));
        }

        // Invoke upscaling ai
        private void UpscalingImagesAsFbx(object sender, DoWorkEventArgs e)
        {
            List<string> arguments = (List<string>)e.Argument;

            string upscalingPath = arguments[0];
            string textureOut = arguments[1];
            string bmdFile = arguments[2];

            // Upscale images
            int count = 0;
            int length = 0;
            foreach (string lowRes in Directory.GetFiles(upscalingPath + "/input"))
            {
                if (length == 0)
                {
                    length = Directory.GetFiles(upscalingPath + "/input").Length;

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        canvasBar.Visibility = Visibility.Visible;
                        upscaleBar.Value = count;
                        upscaleBar.Maximum = length;
                        upscaleText.Text = "Texture " + count + " / " + length;
                    }));
                }
                worker.ReportProgress(count);
                count++;

                using (Process sortProcess = new Process())
                {
                    sortProcess.StartInfo.WorkingDirectory = upscalingPath;
                    sortProcess.StartInfo.FileName = upscalingPath + "/Real-ESRGAN.bat";
                    //sortProcess.StartInfo.FileName = upscalingPath + "/realesrgan-ncnn-vulkan.exe";
                    sortProcess.StartInfo.Arguments = "\"input/" + Path.GetFileName(lowRes) + "\"";
                    //sortProcess.StartInfo.Arguments = "-i \"" + lowRes + "\" -o \"" + lowRes + "_scaled.png\" -n realesrgan-x4plus";
                    sortProcess.StartInfo.CreateNoWindow = true;
                    sortProcess.StartInfo.UseShellExecute = false;
                    sortProcess.StartInfo.RedirectStandardOutput = true;

                    // Start the process
                    //sortProcess.Start();

                    //sortProcess.WaitForExit();
                }
            }

            Console.WriteLine("Upscaling finished");

            // Copy upscaled textures back
            foreach (string highRes in Directory.GetFiles(upscalingPath + "/input"))
            {
                string fileName = Path.GetFileName(highRes);

                if (fileName.EndsWith("_4x.png"))
                {
                    File.Copy(highRes, textureOut + "/bmd/" + fileName.Replace("_4x", ""), true);
                }
            }

            // Clean up files
            DirectoryInfo diLR = new DirectoryInfo(upscalingPath + "/input");
            foreach (FileInfo file in diLR.GetFiles())
            {
                file.Delete();
            }

            string blenderPath = GetPathForExe("blender.exe");
            if (blenderPath.Equals(""))
            {
                System.Windows.MessageBox.Show("Blender installation path could not be found\nCreate it under \"Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\blender.exe\"");
                return;
            }

            // Create bat file, write proper path, run bat file

            using (Process sortProcess = new Process())
            {
                sortProcess.StartInfo.WorkingDirectory = (Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\resources\\").Replace("\\", "/");
                sortProcess.StartInfo.FileName = (Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName + "\\resources\\").Replace("\\", "/") + "/converter.bat";
                //sortProcess.StartInfo.FileName = upscalingPath + "/realesrgan-ncnn-vulkan.exe";
                sortProcess.StartInfo.Arguments = "\"" + blenderPath.Replace("\\", "/") + "\" \"" + bmdFile.Replace("\\", "/") + "\"";
                sortProcess.StartInfo.CreateNoWindow = false;
                sortProcess.StartInfo.UseShellExecute = false;
                sortProcess.StartInfo.RedirectStandardOutput = true;
                System.Windows.MessageBox.Show("\"" + blenderPath.Replace("\\", "/") + "\" \"" + bmdFile.Replace("\\", "/") + "\"");
                // Start the process
                sortProcess.Start();

                sortProcess.WaitForExit();
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                contentTree.IsEnabled = true;
                status.Content = "Exported";

                canvasBar.Visibility = Visibility.Hidden;
                upscaleBar.Value = 0;
                upscaleBar.Maximum = 0;
                upscaleText.Text = "";
            }));
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                canvasBar.Visibility = Visibility.Visible;
                upscaleBar.Value = e.ProgressPercentage;
                upscaleText.Text = "Texture " + e.ProgressPercentage + " / " + upscaleBar.Maximum;
            }));
        }

        private void OpenArchive(string Filename)
        {
            bool IsYaz0 = YAZ0.Check(Filename);
            bool IsYay0 = YAY0.Check(Filename);

            if (IsU8())
                Archive = IsYaz0 ? new U8(YAZ0.DecompressToMemoryStream(Filename), Filename) : (IsYay0 ? new U8(YAY0.DecompressToMemoryStream(Filename), Filename) : new U8(Filename));
            else if (IsRARC())
            {
                Archive = IsYaz0 ? new RARC(YAZ0.DecompressToMemoryStream(Filename), Filename) : (IsYay0 ? new RARC(YAY0.DecompressToMemoryStream(Filename), Filename) : new RARC(Filename));
            }
            else
            {
                //MessageBox.Show("ERROR");
                return;
            }

            ExportOptions.SaveSettings();

            string path = ExportArchiveFile(Archive, Filename);
            currentFolder = path;
            //folders.Add(path);

            Console.WriteLine("FILE: " + Filename);

            if(!Filename.Contains("@")) {
                if(Directory.Exists(Filename)) {
                foreach (string folder in Directory.GetFiles(Filename))
                {
                    Console.WriteLine("FOlder: " + folder);
                        if (folder.Contains("bmd"))
                        {
                            foreach (string bmd in Directory.GetFiles(folder))
                            {
                                Console.WriteLine("bmd: " + bmd);
                                if (bmd.EndsWith(".bmd"))
                                {
                                    string textureOut = Directory.GetParent(bmd).FullName;
                                    Console.WriteLine("TextureOut: " + textureOut);

                                    var newModel = new J3D(Path.GetFileName(bmd));
                                    EndianBinaryReader reader = new EndianBinaryReader(new FileStream(bmd, FileMode.Open, FileAccess.Read), Endian.Big);
                                    // Extract textures to bmd location
                                    newModel.LoadTextures(reader, textureOut + "/bmd");
                                    reader.Close();
                                }
                            }
                        }
                    }
                }
            }


            

            TreeNode[] nodes = Archive.ToTreeNode(0, new ImageList());

            Arc a = new Arc() { ArcName = System.IO.Path.GetFileName(Filename) };
            if (MainWindowViewModel.INSTANCE != null)
            {
                if (MainWindowViewModel.INSTANCE.m_sceneGraphs != null)
                {
                    MainWindowViewModel.INSTANCE.m_sceneGraphs.Clear();
                    MainWindowViewModel.INSTANCE.m_sceneGraphs.Add(new SceneGraphViewModel(a, nodes, this.contentTree, path));
                }
            }

            int Count = Archive.TotalFileCount; //do it here so we don't need to do it twice, as that would be taxing for large archives

            bool IsU8()
            {
                Stream arc;
                if (IsYaz0)
                {
                    arc = YAZ0.DecompressToMemoryStream(Filename);
                }
                else if (IsYay0)
                {
                    arc = YAY0.DecompressToMemoryStream(Filename);
                }
                else
                {
                    arc = new FileStream(Filename, FileMode.Open);
                }
                bool Check = arc.ReadString(4) == U8.Magic;
                arc.Close();
                return Check;
            }
            bool IsRARC()
            {
                Stream arc;
                if (IsYaz0)
                {
                    arc = YAZ0.DecompressToMemoryStream(Filename);
                }
                else if (IsYay0)
                {
                    arc = YAY0.DecompressToMemoryStream(Filename);
                }
                else
                {
                    arc = new FileStream(Filename, FileMode.Open);
                }
                bool Check = arc.ReadString(4) == RARC.Magic;
                arc.Close();
                return Check;
            }

            Dispatcher.BeginInvoke(new Action(() => { status.Content = "Loaded archive"; }));
        }

        private string ExportArchiveFile(Archive archive, string arcPath)
        {
            string path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            //string path = "C:\\Users\\finno\\Desktop\\Extracted\\res\\Object\\_ExtractedTextures\\" + archive.Name;
            //Console.WriteLine("Extracted: " + path);
            Directory.CreateDirectory(path);
            //folders.Add(path);

            Console.WriteLine("Extracted: " + path);
            return archive.Export(path, true);
        }

        static TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i))
                if (i is TreeViewItem)
                    return (TreeViewItem)i;

            return null;
        }

        public string GetFullPath(TreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException();

            var result = Convert.ToString(node.Header);

            for (var i = GetParentItem(node); i != null; i = GetParentItem(i))
                result = i.Header + "\\" + result;

            return result;
        }

        private string keyBase = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";
        private string GetPathForExe(string fileName)
        {
            RegistryKey localMachine = Registry.LocalMachine;
            RegistryKey fileKey = localMachine.OpenSubKey(string.Format(@"{0}\{1}", keyBase, fileName));
            object result = null;
            if (fileKey != null)
            {
                result = fileKey.GetValue(string.Empty);
                fileKey.Close();
            }


            return (string)result;
        }
        private static DialogResult ShowInputDialogBox(ref string input, string prompt, string title = "Title", int width = 300, int height = 200)
        {
            //This function creates the custom input dialog box by individually creating the different window elements and adding them to the dialog box

            //Specify the size of the window using the parameters passed
            System.Drawing.Size size = new System.Drawing.Size(width, height);
            //Create a new form using a System.Windows Form
            Form inputBox = new Form();
            inputBox.StartPosition = FormStartPosition.CenterScreen;

            inputBox.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            //Set the window title using the parameter passed
            inputBox.Text = title;

            //Create a new label to hold the prompt
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = prompt;
            label.Location = new System.Drawing.Point(5, 5);
            label.Width = size.Width - 10;
            inputBox.Controls.Add(label);

            //Create a textbox to accept the user's input
            System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, label.Location.Y + 20);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            //Create an OK Button 
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, size.Height - 30);
            inputBox.Controls.Add(okButton);

            //Create a Cancel Button
            System.Windows.Forms.Button cancelButton = new System.Windows.Forms.Button();
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, size.Height - 30);
            inputBox.Controls.Add(cancelButton);

            //Set the input box's buttons to the created OK and Cancel Buttons respectively so the window appropriately behaves with the button clicks
            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            //Show the window dialog box 
            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            inputBox.StartPosition = FormStartPosition.CenterScreen;

            //After input has been submitted, return the input value
            return result;
        }
    }

    static class ItemContainerGeneratorExtension
    {
        public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item)
        {
            var treeViewItem = root.ContainerFromItem(item) as TreeViewItem;
            if (treeViewItem != null)
                return treeViewItem;
            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
                if (search != null)
                    return search;
            }
            return null;
        }
    }
}
