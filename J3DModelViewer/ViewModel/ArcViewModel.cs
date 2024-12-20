﻿using JStudio.J3D;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J3DModelViewer.ViewModel
{
    /// <summary>
    /// Small wrapper class for the <see cref="HierarchyNode"/> class in <see cref="J3D"/>. This allows us to
    /// show the joint names in the UI even though the HierarchyNode doesn't actually have them. It only specifies
    /// the joint index, and we have to look up the actual joint from the <see cref="JNT1"/> section to get its name.
    /// </summary>

    public class ArcViewModel
    {
        public ObservableCollection<ArcViewModel> Test { get; private set; }
        public HierarchyNode Node { get; private set; }
        public string Name { get; private set; }

        public List<Arc> arcList = new List<Arc>();

        public ArcViewModel(J3D model,  HierarchyNode parent, string nodeName)
        {

            Name = nodeName;
            Node = parent;
            Test = new ObservableCollection<ArcViewModel>();

            foreach(var childNode in Node.Children)
            {
                string childNodeName = "";
                if (childNode.Type == HierarchyDataType.Joint)
                {
                    var jnt1 = model.JNT1Tag;
                    childNodeName = jnt1.BindJoints[jnt1.JointRemapTable[childNode.Value]].ToString();
                }
                if(childNode.Type == HierarchyDataType.Material)
                {
                    var mat3 = model.MAT3Tag;
                    childNodeName = mat3.MaterialList[mat3.MaterialRemapTable[childNode.Value]].Name;
                }

                ArcViewModel child = new ArcViewModel(model, childNode, childNodeName);
                Test.Add(child);
            }
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
    }
}
