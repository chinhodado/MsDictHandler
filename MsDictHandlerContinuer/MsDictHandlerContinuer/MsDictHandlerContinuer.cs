﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace MsDictHandlerContinuer {
    public enum ActivateOptions {
        None = 0x00000000,  // No flags set
        DesignMode = 0x00000001,  // The application is being activated for design mode, and thus will not be able to
        // to create an immersive window. Window creation must be done by design tools which
        // load the necessary components by communicating with a designer-specified service on
        // the site chain established on the activation manager.  The splash screen normally
        // shown when an application is activated will also not appear.  Most activations
        // will not use this flag.
        NoErrorUI = 0x00000002,  // Do not show an error dialog if the app fails to activate.
        NoSplashScreen = 0x00000004,  // Do not show the splash screen when activating the app.
    }

    [ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IApplicationActivationManager {
        // Activates the specified immersive application for the "Launch" contract, passing the provided arguments
        // string into the application.  Callers can obtain the process Id of the application instance fulfilling this contract.
        IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] ActivateOptions options, [Out] out UInt32 processId);
        IntPtr ActivateForFile([In] String appUserModelId, [In] IntPtr /*IShellItemArray* */ itemArray, [In] String verb, [Out] out UInt32 processId);
        IntPtr ActivateForProtocol([In] String appUserModelId, [In] IntPtr /* IShellItemArray* */itemArray, [Out] out UInt32 processId);
    }

    [ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]//Application Activation Manager
    class ApplicationActivationManager : IApplicationActivationManager {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)/*, PreserveSig*/]
        public extern IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] ActivateOptions options, [Out] out UInt32 processId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern IntPtr ActivateForFile([In] String appUserModelId, [In] IntPtr /*IShellItemArray* */ itemArray, [In] String verb, [Out] out UInt32 processId);
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        public extern IntPtr ActivateForProtocol([In] String appUserModelId, [In] IntPtr /* IShellItemArray* */itemArray, [Out] out UInt32 processId);
    }

    /**
     * Monitor MsDictHandler.exe and start it up if it's not running. Required since MsDictHandler needs to be restarted every 500 words.
     * Get the app's id at
     * HKEY_CURRENT_USER\Software\Classes\ActivatableClasses\Package\28587606-2a78-4929-9f33-7a8c1e7102f7_1.0.0.0_x86__bhmrb0r2ehb8t\Server\App.AppXa0wwp936wckwwjxd2ax1q9mmay3zzknn.mca
     * -> AppUserModelId
     * (the key above may not be exact - just an example)
     */
    public static class MsDictHandlerContinuer {
        static void Main(string[] args) {
            int count = 0;
            while (true) {
                Process[] pname = Process.GetProcessesByName("MsDictHandler");
                if (pname.Length == 0) {
                    count++;
                    Console.WriteLine("Iteration " + count + ", index [" + (count - 1) * 500 + ", " + count * 500 + "]");
                    ApplicationActivationManager appActiveManager = new ApplicationActivationManager();
                    uint pid;
                    appActiveManager.ActivateApplication(@"28587606-2a78-4929-9f33-7a8c1e7102f7_bhmrb0r2ehb8t!App", null,
                        ActivateOptions.None, out pid);
                }
                else {
                    Thread.Sleep(2000);
                }
            }
        }
    }
}
