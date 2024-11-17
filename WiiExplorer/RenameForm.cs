﻿using Hack.io.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WiiExplorer.Properties.Languages;

namespace WiiExplorer
{ 
    public partial class RenameForm : Form
    {
        public RenameForm(TreeView treeview, Archive archive)
        {
            InitializeComponent();
            CenterToParent();
            MaximumSize = new Size(Screen.GetBounds(Location).Width, Height);
            MinimumSize = Size;
            Archive = archive;
            ArchiveTreeView = treeview;
            CurrentItem = Archive[treeview.SelectedNode.FullPath];
            string OGName = CurrentItem.Name;
            Text = string.Format(Strings.RenameWindowTitle, OGName);
            if (!(CurrentItem is RARC.Directory))
            {
                FileInfo fi = new FileInfo(OGName);
                NameTextBox.Text = Path.GetFileNameWithoutExtension(OGName);
                ExtensionTextBox.Text = fi.Extension;
            }
            else
            {
                ExtensionTextBox.Enabled = false;
                ExtensionLabel.Enabled = false;
                NameTextBox.Text = OGName;
            }

            ForeColor = Program.ProgramColours.TextColour;
            BackColor = Program.ProgramColours.ControlBackColor;
            OKButton.BackColor = DiscardButton.BackColor = Program.ProgramColours.ControlBackColor;
            NameTextBox.BackColor = ExtensionTextBox.BackColor = Program.ProgramColours.WindowColour;
            NameTextBox.ForeColor = OKButton.ForeColor = DiscardButton.ForeColor = ExtensionTextBox.ForeColor = Program.ProgramColours.TextColour;
            NameTextBox.BorderColor = ExtensionTextBox.BorderColor = Program.ProgramColours.BorderColour;
        }

        dynamic CurrentItem;
        Archive Archive;
        TreeView ArchiveTreeView;

        private void RenameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                string prevname = ArchiveTreeView.SelectedNode.Text;
                string prevpath = ArchiveTreeView.SelectedNode.FullPath;
                string finalname = NameTextBox.Text + (CurrentItem is RARC.File ? ExtensionTextBox.Text : "");
                ArchiveTreeView.SelectedNode.Text = finalname;
                if (Archive.ItemExists(ArchiveTreeView.SelectedNode.FullPath) && Archive[prevpath] != Archive[ArchiveTreeView.SelectedNode.FullPath])
                {
                    ArchiveTreeView.SelectedNode.Text = prevname;
                    MessageBox.Show(Strings.ItemAlreadyExists, Strings.DuplicateName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }
                Archive.MoveItem(prevpath, ArchiveTreeView.SelectedNode.FullPath);
                CurrentItem.Name = finalname;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DiscardButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
