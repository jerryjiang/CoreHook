﻿
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using CoreHook.IPC.Platform;
using CoreHook.UWP.FileMonitor.Pipe;

namespace CoreHook.UWP.FileMonitor
{
    public class PipePlatform : IPipePlatform
    {
        private static PipeSecurity CreateUWPPipeSecurity()
        {
            const PipeAccessRights access = PipeAccessRights.ReadWrite;

            var pipeSecurity = new PipeSecurity();

            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                pipeSecurity.AddAccessRule(
                    new PipeAccessRule(identity.User, access, AccessControlType.Allow)
                );

                if (identity.User != identity.Owner)
                {
                    pipeSecurity.AddAccessRule(
                        new PipeAccessRule(identity.Owner, access, AccessControlType.Allow)
                    );
                }
                /*
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    // Allow the Administrators group full access to the pipe.
                    pipeSecurity.AddAccessRule(new PipeAccessRule(
                        new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null).Translate(typeof(NTAccount)),
                        PipeAccessRights.FullControl, AccessControlType.Allow));
                }
                else
                {
                    // Allow the current user read/write access to the pipe.
                    pipeSecurity.AddAccessRule(new PipeAccessRule(
                        WindowsIdentity.GetCurrent().User,
                        PipeAccessRights.ReadWrite, AccessControlType.Allow));
                }
                */
            }

            // Allow all app packages to connect.
            pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier("S-1-15-2-1"), access, AccessControlType.Allow));
            return pipeSecurity;
        }

        public NamedPipeServerStream CreatePipeByName(string pipeName)
        {

            // Unfortunately, .NET Core does not support passing in a PipeSecurity object into the constructor for
            // NamedPipeServerStream so we are creating native Named Pipes and securing them using native APIs. The
            // issue on .NET Core regarding Named Pipe security is here: https://github.com/dotnet/corefx/issues/30170
            // 99% of this code was borrowed from PowerShell here:
            // https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/remoting/common/RemoteSessionNamedPipe.cs#L124-L256
            return NamedPipeNative.CreateNamedPipe(".", "pipe", pipeName, CreateUWPPipeSecurity());
        }

    }
}
