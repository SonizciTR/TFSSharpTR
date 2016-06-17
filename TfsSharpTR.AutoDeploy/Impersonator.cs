using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace TfsSharpTR.AutoDeploy
{
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }



    // Test harness.
    // If you incorporate this code into a DLL, be sure to demand FullTrust.
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    internal class Impersonator
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
        int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public extern static bool CloseHandle(IntPtr handle);

        private static WindowsImpersonationContext impersonatedUser = null;
        private static SafeTokenHandle safeTokenHandle = null;

        public static string Change(string userName, string passWord, string domainName)
        {
            try
            {
                // Get the user token for the specified user, domain, and password using the
                // unmanaged LogonUser method.
                // The local machine name can be used for the domain name to impersonate a user on this machine.
                const int LOGON32_PROVIDER_DEFAULT = 0;
                //This parameter causes LogonUser to create a primary token.
                const int LOGON32_LOGON_INTERACTIVE = 2;

                const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

                // Call LogonUser to obtain a handle to an access token.
                bool returnValue = LogonUser(userName, domainName, passWord,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);


                if (false == returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    return new System.ComponentModel.Win32Exception(ret).ToString();
                }

                Console.WriteLine("Did LogonUser Succeed? " + (returnValue ? "Yes" : "No"));
                Console.WriteLine("Value of Windows NT token: " + safeTokenHandle);

                // Check the identity.
                Console.WriteLine("Before impersonation: "
                                  + WindowsIdentity.GetCurrent().Name);
                // Use the token handle returned by LogonUser.
                impersonatedUser = WindowsIdentity.Impersonate(safeTokenHandle.DangerousGetHandle());

                // Check the identity.
                Console.WriteLine("After impersonation: " + WindowsIdentity.GetCurrent().Name);

                return null;
            }
            catch (Exception ex)
            {
                return ("Impersonator.Close failed. Ex = " + ex.Message);
            }
        }

        public static string Close()
        {
            try
            {
                var ptr = safeTokenHandle.DangerousGetHandle();
                if (ptr != IntPtr.Zero)
                {
                    CloseHandle(ptr);
                    ptr = IntPtr.Zero;
                    impersonatedUser.Undo();
                }
            }
            catch(Exception ex)
            {
                return "Impersonator.Close failed. Ex = " + ex.ToString();
            }
            return null;
        }

    }
}
