using J3DModelViewer.View;
using JStudio.J3D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static OpenTK.Graphics.OpenGL.GL;

namespace J3DModelViewer.ViewModel
{
    /// <summary>
    /// Small wrapper class for the <see cref="HierarchyNode"/> class in <see cref="J3D"/>. This allows us to
    /// show the joint names in the UI even though the HierarchyNode doesn't actually have them. It only specifies
    /// the joint index, and we have to look up the actual joint from the <see cref="JNT1"/> section to get its name.
    /// </summary>
    /// 

    public class SceneGraphViewModel
    {
        public static SceneGraphViewModel INSTANCE;

        public ObservableCollection<Arc> Children { get; private set; }
        public HierarchyNode Node { get; private set; }
        public string Name { get; private set; }

        public List<Arc> arcList = new List<Arc>();

        public Arc root;
        //public TreeNode[] nodes;
        public TreeNode bmd;
        public List<String> bmds = new List<String>();

        private List<String> files = new List<String>();

        private List<TreeViewItem> items = new List<TreeViewItem>();

        public ObservableCollection<TreeNode> TreeNodes { get; private set; }

        public SceneGraphViewModel(Arc root, string fullPath)
        {
            this.root = root;
            INSTANCE = this;

            Children = new ObservableCollection<Arc>();

            string folderPath = Directory.GetParent(fullPath) + "/archive";
            if (Directory.Exists(folderPath))
            {
                Console.WriteLine("Delete present archive");
                Directory.Delete(folderPath);
            }

            string arg = "\"E:\\Meine Ablage\\Extracted\\WiiExplorer.V1.5.0.5\\WiiExplorer.exe\" -u \"" + fullPath + "\"";

            /*System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C \"E:\\Meine Ablage\\Extracted\\WiiExplorer.V1.5.0.5\\WiiExplorer.exe\" -u \"" + fullPath + "\"";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();*/

            Process p = System.Diagnostics.Process.Start("CMD.exe", arg);

            DirectoryInfo startDir = new DirectoryInfo(folderPath);

            //TraverseDirectory(startDir);

            foreach (string bmd in files)
            {
                Children.Add(new Arc() { ArcName = bmd });
            }
        }

        private string folderPath;

        public SceneGraphViewModel(Arc root, TreeNode[] nodes, System.Windows.Controls.TreeView contentTree, string folderPath)
        {
            this.root = root;
            this.folderPath = folderPath;
            Children = new ObservableCollection<Arc>();
            TreeNodes = new ObservableCollection<TreeNode>();

            contentTree.Items.Clear();

            TreeViewItem arcRoot = new TreeViewItem();
            arcRoot.Header = root.ArcName;

            foreach (TreeNode node in nodes)
            {
                TreeViewItem subItem = new TreeViewItem();
                subItem.Header = node.Text;

                CollectBmdNodes(node, subItem);

                arcRoot.Items.Add(subItem);
            }
            contentTree.Items.Add(arcRoot);


            arcRoot.IsExpanded = true;
            foreach (TreeViewItem sub in arcRoot.Items)
            {
                if (sub.Header.ToString().Contains("bm"))
                {
                    sub.IsExpanded = true;
                }
            }

            bool autoLoaded = false;

            // Auto load first model
            foreach (TreeViewItem category in arcRoot.Items)
            {
                if (category.Header.ToString().Contains("bm"))
                {
                    TreeViewItem first = (TreeViewItem)category.Items.GetItemAt(0);
                    string path = folderPath + "/" + category.Header.ToString() + "/" + first.Header;
                    ArcImporter.currentBMD = path;
                    MainWindowViewModel.INSTANCE.LoadAssetFromFilepath(path, true);
                    autoLoaded = true;

                    foreach (TreeViewItem defaultItem in items)
                    {
                        defaultItem.Foreground = Brushes.Black;
                    }

                    first.Foreground = Brushes.Red;

                    break;
                }
            }

            if (!autoLoaded)
            {
                MainWindowViewModel.INSTANCE.Unload();
            }
        }

        private void CollectBmdNodes(TreeNode parent, TreeViewItem item)
        {
            foreach (TreeNode child in parent.Nodes)
            {
                if (child.Nodes.Count > 0)
                {
                    TreeViewItem subItem = new TreeViewItem();
                    subItem.Header = child.Text;
                    item.Items.Add(subItem);

                    CollectBmdNodes(child, subItem);
                }
                else
                {
                    if (child.Text.EndsWith(".bmd"))
                    {
                        Console.WriteLine("ADD: " + child.Text);
                        bmds.Add(child.Text);

                        //Children.Add(new Arc() { ArcName = child.Text });
                    }

                    TreeViewItem content = new TreeViewItem();
                    content.Header = child.Text;
                    item.Items.Add(content);
                    items.Add(content);

                    content.MouseLeftButtonUp += treeItem_MouseLeftButtonUp;

                    //Children.Add(new Arc() { ArcName = child.Text });
                }
            }
        }

        void treeItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            string path = GetFullPath(item).Split(new string[] { ".arc" }, StringSplitOptions.None)[1];

            string filePath = folderPath + path;
            Console.WriteLine("LOAD: " + filePath);
            if (item.Header.ToString().Contains(".bm"))
                ArcImporter.currentBMD = filePath;
            MainWindowViewModel.INSTANCE.LoadAssetFromFilepath(filePath, true);

            foreach (TreeViewItem defaultItem in items)
            {
                defaultItem.Foreground = Brushes.Black;
            }

            item.Foreground = Brushes.Red;
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

        static TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i))
                if (i is TreeViewItem)
                    return (TreeViewItem)i;

            return null;
        }

        public override string ToString()
        {
            string nodeStr = Node.ToString();

            // Override our name handling for nodes that have names.
            if (!string.IsNullOrEmpty(Name))
            {
                return string.Format("{0} [{1}]", nodeStr, Name);
            }

            return nodeStr;
        }

        public void TraverseDirectory(DirectoryInfo directoryInfo)
        {
            var subdirectories = directoryInfo.EnumerateDirectories();

            foreach (var subdirectory in subdirectories)
            {
                TraverseDirectory(subdirectory);
            }

            var files = directoryInfo.EnumerateFiles();

            foreach (var file in files)
            {
                HandleFile(file);
            }
        }

        void HandleFile(FileInfo file)
        {
            if (file.Extension.Contains("bmd"))
            {
                files.Add(file.FullName);
            }
        }
    }
}
