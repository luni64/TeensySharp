using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static lunOptics.libUsbTree.NativeWrapper;

namespace lunOptics.libUsbTree
{
    public class InfoNode
    {
        #region properties ----------------------------------------------------
        public int node { get; private set; }
        public string devInstId { get; private set; }
        public string enumerator { get; private set; }
        public string serNumStr { get; private set; }
        public int vid { get; private set; } = -1;
        public int pid { get; private set; } = -1;
        public int mi { get; private set; } = -1;
        public bool isInterface { get; private set; }
        public bool isUsbFunction { get; private set; }
        public List<InfoNode> children { get; private set; } = new List<InfoNode>();
        #endregion

        #region construction --------------------------------------------------

        internal InfoNode(IEnumerable<int> roots) // recursively constructs the tree of device nodes starting with the passed in root nodes
        {
            if (roots == null) throw new ArgumentNullException(nameof(roots));

            node = -1;
            devInstId = "lunOptics.InfoNodeTree";
            foreach (var node in roots)
            {
                children.Add(new InfoNode(node));
            }
        }

        private InfoNode(int node)                // quickly build tree structure (will be called frequently to check for changes)
        {
            this.node = node;
            foreach (var child in cmGetChildNodes(node))
            {
                children.Add(new InfoNode(child));
            }
        }

        internal void readDetails()               // reads required details from the system
        {
            if (node != -1) parseInfo(node);
            foreach (var child in children)
            {
                child.readDetails();
            }
        }

        private void parseInfo(int node)
        {
            this.node = node;
            devInstId = cmGetNodePropStrg(node, DevPropKeys.DeviceInstanceID);
            if (!String.IsNullOrEmpty(devInstId)) // driver not yet installed ?
            {
                var p = devInstId.Split('\\');

                enumerator = p[0];
                serNumStr = p[2];

                Match mVID = Regex.Match(p[1], @"VID_([0-9A-F]{4})", RegexOptions.IgnoreCase);
                if (mVID.Success) vid = Convert.ToInt32(mVID.Groups[1].Value, 16);

                Match mPID = Regex.Match(p[1], @"PID_([0-9A-F]{4})", RegexOptions.IgnoreCase);
                if (mPID.Success) pid = Convert.ToInt32(mPID.Groups[1].Value, 16);

                Match mMI = Regex.Match(p[1], @"MI_([0-9A-F]{2})", RegexOptions.IgnoreCase);
                if (mMI.Success) mi = Convert.ToInt32(mMI.Groups[1].Value, 16);

                isUsbFunction = mi != -1 && enumerator == "USB";

                isInterface = !isUsbFunction && (mi != -1 || enumerator != "USB");
            }
        }
        #endregion

        internal bool isEqual(InfoNode other)
        {
            if (other == null || other.node != node || other.children.Count != children.Count) return false;
            for (int i = 0; i < children.Count; i++)
            {
                if (!children[i].isEqual(other.children[i])) return false;                
            }
            return true;
        }  // compares trees by comparing the node structure to check if any devices where added/removed
    }
}

