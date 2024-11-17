using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace J3DModelViewer
{
    static class FileAssociationHelper
    {
        public static void AssociateFileExtension
        (string fileExtension, string name, string description, string appPath)
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.LocalMachine;
            RegistrySecurity rs = new RegistrySecurity();
            rs = key.GetAccessControl();
            string currentUserStr = Environment.UserDomainName + "\\" + Environment.UserName;
            rs.AddAccessRule(
                new RegistryAccessRule(
                    currentUserStr,
                    RegistryRights.WriteKey
                    | RegistryRights.ReadKey
                    | RegistryRights.Delete
                    | RegistryRights.FullControl,
                    AccessControlType.Allow));

            //Create a key with specified file extension
            RegistryKey _extensionKey = Registry.ClassesRoot.CreateSubKey(fileExtension);
            _extensionKey.SetValue("", name);

            //Create main key for the specified file format
            RegistryKey _formatNameKey = Registry.ClassesRoot.CreateSubKey(name);
            _formatNameKey.SetValue("", description);
            _formatNameKey.CreateSubKey("DefaultIcon").SetValue("", "\"" + appPath + "\",0");

            //Create the 'Open' action under 'Shell' key
            RegistryKey _shellActionsKey = _formatNameKey.CreateSubKey("Shell");
            _shellActionsKey.CreateSubKey("open").CreateSubKey("command").SetValue
                                         ("", "\"" + appPath + "\" \"%1\"");

            _extensionKey.Close();
            _formatNameKey.Close();
            _shellActionsKey.Close();

            // Update Windows Explorer windows for this new file association
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify
                (uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
