﻿using System;
using System.ComponentModel;
using J3DModelViewer.View;
using JStudio.OpenGL;

namespace J3DModelViewer.ViewModel
{
    public class ModelRenderOptionsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IgnoreAlphaTest
        {
            get { return m_ignoreAlphaTest; }
            set
            {
                Console.WriteLine("SET TO " + value);
                m_ignoreAlphaTest = value;
                OnPropertyChanged("IgnoreAlphaTest");

                Shader.IgnoreAlphaTest = value;
                Console.WriteLine("LOAD: " + ArcImporter.currentBMD);
                if (!ArcImporter.currentBMD.Equals(""))
                {
                    Console.WriteLine("LOAD: " + ArcImporter.currentBMD);
                    MainWindowViewModel.INSTANCE.LoadAssetFromFilepath(ArcImporter.currentBMD, true);
                }
            }
        }

        public bool ShowPivot
        {
            get { return m_showPivot; }
            set
            {
                m_showPivot = value;
                OnPropertyChanged("ShowPivot");
            }
        }

        public bool ShowGrid
        {
            get { return m_showGrid; }
            set
            {
                m_showGrid = value;
                OnPropertyChanged("ShowGrid");
            }
        }

        public bool AnimateLight
        {
            get { return m_animateLight; }
            set
            {
                m_animateLight = value;
                OnPropertyChanged("AnimateLight");
            }
        }


        public bool ShowBoundingBox
        {
            get { return m_showBoundingBox; }
            set
            {
                m_showBoundingBox = value;
                OnPropertyChanged("ShowBoundingBox");
            }
        }

        public bool ShowBoundingSphere
        {
            get { return m_showBoundingSphere; }
            set
            {
                m_showBoundingSphere = value;
                OnPropertyChanged("ShowBoundingSphere");
            }
        }

        public bool ShowBoneBoundingBox
        {
            get { return m_showBoneBoundingBox; }
            set
            {
                m_showBoneBoundingBox = value;
                OnPropertyChanged("ShowBoneBoundingBox");
            }
        }

        public bool ShowBoneBoundingSphere
        {
            get { return m_showBoneBoundingSphere; }
            set
            {
                m_showBoneBoundingSphere = value;
                OnPropertyChanged("ShowBoneBoundingSphere");
            }
        }

        public bool ShowBones
        {
            get { return m_showBones; }
            set
            {
                m_showBones = value;
                OnPropertyChanged("ShowBones");
            }
        }

		public bool DepthPrePass
		{
			get { return m_depthPrePass; }
			set
			{
				m_depthPrePass = value;
				OnPropertyChanged("DepthPrePass");
			}
		}

        private bool m_ignoreAlphaTest;
        private bool m_showPivot;
        private bool m_showGrid;
        private bool m_animateLight;
        private bool m_showBoundingBox;
        private bool m_showBoundingSphere;
        private bool m_showBoneBoundingBox;
        private bool m_showBoneBoundingSphere;
        private bool m_showBones;
        private bool m_depthPrePass;

		public ModelRenderOptionsViewModel()
        {
            IgnoreAlphaTest = Properties.Settings.Default.IgnoreAlphaTest;
            ShowPivot = Properties.Settings.Default.ShowPivot;
            ShowGrid = Properties.Settings.Default.ShowGrid;
            AnimateLight = Properties.Settings.Default.AnimateLight;
            ShowBoundingBox = Properties.Settings.Default.ShowBoundingBox;
            ShowBoundingSphere = Properties.Settings.Default.ShowBoundingSphere;
            ShowBoneBoundingBox = Properties.Settings.Default.ShowBoneBoundingBox;
            ShowBoneBoundingSphere = Properties.Settings.Default.ShowBoneBoundingSphere;
            ShowBones = Properties.Settings.Default.ShowBones;
			DepthPrePass = Properties.Settings.Default.DepthPrePass;
		}

		public void SaveSettings()
        {
            Properties.Settings.Default.IgnoreAlphaTest = IgnoreAlphaTest;
            Properties.Settings.Default.ShowPivot = ShowPivot;
            Properties.Settings.Default.ShowGrid = ShowGrid;
            Properties.Settings.Default.AnimateLight = AnimateLight;
            Properties.Settings.Default.ShowBoundingBox = ShowBoundingBox;
            Properties.Settings.Default.ShowBoundingSphere = ShowBoundingSphere;
            Properties.Settings.Default.ShowBoneBoundingBox = ShowBoneBoundingBox;
            Properties.Settings.Default.ShowBoneBoundingSphere = ShowBoneBoundingSphere;
            Properties.Settings.Default.ShowBones = ShowBones;
            Properties.Settings.Default.DepthPrePass = DepthPrePass;
			Properties.Settings.Default.Save();
        }

        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
