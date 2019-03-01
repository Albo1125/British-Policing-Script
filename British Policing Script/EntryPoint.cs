using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.API;
using Rage;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using RAGENativeUI;
using RAGENativeUI.Elements;
using System.Drawing;
using System.Threading;
using System.Management;
using System.Net;
using Rage.Native;

namespace British_Policing_Script
{
    internal static class EntryPoint
    {
        public static Keys ToggleMenuKey = Keys.F9;
        public static Keys ToggleMenuModifierKey = Keys.None;

        public static Guid LSPDFRPlusSecurityGuid;

        public static KeysConverter kc = new KeysConverter();
        public static void Initialise()
        {
            //ini stuff

            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/British Policing Script.ini");
            ini.Create();
            try
            {
                ToggleMenuKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "ToggleMenuKey", "F9"));
                ToggleMenuModifierKey = (Keys)kc.ConvertFromString(ini.ReadString("General", "ToggleMenuModifierKey", "None"));
                FailtostopEnabled = ini.ReadBoolean("Callouts", "FailToStopEnabled", true);
                FailtostopFrequency = ini.ReadInt32("Callouts", "FailToStopFrequency", 2);
                ANPRHitEnabled = ini.ReadBoolean("Callouts", "ANPRHitEnabled", true);
                ANPRHitFrequency = ini.ReadInt32("Callouts", "ANPRHitFrequency", 2);
                TWOCEnabled = ini.ReadBoolean("Callouts", "TWOCEnabled", true);
                TWOCFrequency = ini.ReadInt32("Callouts", "TWOCFrequency", 2);
                CourtSystem.RealisticCourtDates = ini.ReadBoolean("General", "RealisticCourtDates", true);
            }
            catch(Exception e)
            {
                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Error loading British Policing Script INI file. Loading defaults");
                Game.DisplayNotification("~r~Error loading British Policing Script INI file. Loading defaults");
            }
            BetaCheck();
        }
        private static void MainLoop()
        {
            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("British Policing Script has been initialised successfully and is now loading INI, XML and dependencies. Standby...");
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);
                
                //LSPDFRPlusSecurityGuid = LSPDFR_.API.ProtectedFunctions.GenerateSecurityGuid(Assembly.GetExecutingAssembly(), "British Policing Script", "Albo1125", "u3dDhOrbc91cUST+XROf0Wd/MFZXG7D5ufrg4QJCX5bvAJxITxFgtmiwecxqL9kawDa5rU6JgdVFqszg1XES8T0X107MhMrbZDgm6v46iYhD07bdnjBgMSaTGzIHgLFV/PAAvTjdDy5fLmAkxU1jX7w2pabZpD9BFW5hWpPZTs8=");
                Menus.InitialiseMenus();
                RegisterCallouts();
                CourtSystem.CourtSystemMainLogic();
                Game.LogTrivial("British Policing Script has been fully initialised successfully and is now working.");

                while (true)
                {
                    GameFiber.Yield();

                    //if (Game.IsKeyDown(Keys.F10))
                    //{
                    //    Functions.StartCallout("ANPR Hit");
                    //}
                }
            });

        }

        private static bool FailtostopEnabled = true;
        private static int FailtostopFrequency = 2;
        private static bool TWOCEnabled = true;
        private static int TWOCFrequency = 2;
        private static bool ANPRHitEnabled = true;
        private static int ANPRHitFrequency = 2;

        private static void RegisterCallouts()
        {
            if (FailtostopEnabled)
            {
                Functions.RegisterCallout(typeof(Callouts.FailToStop));
                for (int i = 1; i < FailtostopFrequency; i++)
                {
                    Functions.RegisterCallout(typeof(Callouts.FailToStop));
                }
            }
            if (TWOCEnabled)
            {
                Functions.RegisterCallout(typeof(Callouts.TWOC));
                for (int i = 1; i < TWOCFrequency; i++)
                {
                    Functions.RegisterCallout(typeof(Callouts.TWOC));
                }
            }
            if (ANPRHitEnabled)
            {
                Functions.RegisterCallout(typeof(Callouts.ANPRHit));
                for (int i = 1; i < ANPRHitFrequency; i++)
                {
                    Functions.RegisterCallout(typeof(Callouts.ANPRHit));
                }
            }
        }
        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName();
                if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0) { return true; }
                }
            }
            return false;
        }
        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args) { foreach (Assembly assembly in Functions.GetAllUserPlugins()) { if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower())) { return assembly; } } return null; }

        public static Random rnd = new Random();

        public static void BetaCheck()
        {

            GameFiber.StartNew(delegate
            {

                Game.LogTrivial("British Policing Script, developed by Albo1125, has been loaded successfully!");
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~British Policing Script~s~, developed by ~b~Albo1125, ~s~has been loaded ~g~successfully.");


            });

            MainLoop();

        }
    }
}

