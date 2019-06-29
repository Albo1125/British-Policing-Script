using Albo1125.Common;
using Albo1125.Common.CommonLibrary;
using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Reflection;

namespace British_Policing_Script
{
    internal class Main : Plugin
    {

        public Main()
        {
            UpdateChecker.VerifyXmlNodeExists(PluginName, FileID, DownloadURL, Path);
            DependencyChecker.RegisterPluginForDependencyChecks(PluginName);
        }


        public override void Finally()
        {

        }


        public override void Initialize()
        {
            //Event handler for detecting if the player goes on duty

            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
            Game.LogTrivial("British Policing Script " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", developed by Albo1125, has been initialised.");
            Game.LogTrivial("Go on duty to start British Policing Script.");


        }
        //Dependencies


        internal static Version Albo1125CommonVer = new Version("6.6.4.0");
        internal static Version MadeForGTAVersion = new Version("1.0.1604.1");
        internal static float MinimumRPHVersion = 0.51f;
        internal static string[] AudioFilesToCheckFor = new string[] { "LSPDFR/audio/scanner/British Policing Script Audio/Crimes/BRITISHCRIME_FAILTOSTOP.wav" };
        internal static string[] OtherFilesToCheckFor = new string[] { "Plugins/LSPDFR/Traffic Policer.dll", "Plugins/LSPDFR/Arrest Manager.dll", "Plugins/LSPDFR/LSPDFR+.dll" };
        internal static Version RAGENativeUIVersion = new Version("1.6.3.0");
        internal static Version MadeForLSPDFRVersion = new Version("0.4.2");


        internal static string DownloadURL = "http://bit.ly/BritishScript";
        internal static string FileID = "11468";

        internal static string PluginName = "British Policing Script";
        internal static string Path = "Plugins/LSPDFR/British Policing Script.dll";

        internal static Version TrafficPolicerVersion = new Version("6.15.0.0");
        internal static Version ArrestManagerVersion = new Version("7.10.0.0");
        internal static Version LSPDFRPlusVersion = new Version("1.7.0.0");


        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            if (onDuty)
            {
                UpdateChecker.InitialiseUpdateCheckingProcess();
                if (DependencyChecker.DependencyCheckMain(PluginName, Albo1125CommonVer, MinimumRPHVersion, MadeForGTAVersion, MadeForLSPDFRVersion, RAGENativeUIVersion, AudioFilesToCheckFor, OtherFilesToCheckFor))
                {

                    if (!DependencyChecker.CheckIfFileExists("Plugins/LSPDFR/Traffic Policer.dll", TrafficPolicerVersion))
                    {
                        Game.LogTrivial("Traffic Policer is out of date for BPS. Aborting. Required version: " + TrafficPolicerVersion.ToString());
                        Game.DisplayNotification("~r~~h~BritishP.S. detected Traffic Policer version lower than ~b~" + TrafficPolicerVersion.ToString());
                        ExtensionMethods.DisplayPopupTextBoxWithConfirmation("British Policing Script Dependencies", "BritishP.S. didn't detect Traffic Policer or detected Traffic Policer version lower than " + TrafficPolicerVersion.ToString() + ". Please install the appropriate version of Traffic Policer (link on the BPS download page under Requirements). Unloading British Policing Script...", true);
                        return;
                    }
                    if (!DependencyChecker.CheckIfFileExists("Plugins/LSPDFR/Arrest Manager.dll", ArrestManagerVersion))
                    {
                        Game.LogTrivial("Arrest Manager is out of date for BPS. Aborting. Required version: " + ArrestManagerVersion.ToString());
                        Game.DisplayNotification("~r~~H~BritishP.S. detected Arrest Manager version lower than ~b~" + ArrestManagerVersion.ToString());
                        ExtensionMethods.DisplayPopupTextBoxWithConfirmation("British Policing Script Dependencies", "BritishP.S. didn't detect Arrest Manager or detected Arrest Manager version lower than " + ArrestManagerVersion.ToString() + ". Please install the appropriate version of Arrest Manager (link on the BPS download page under Requirements). Unloading British Policing Script...", true);
                        return;
                    }
                    if (!DependencyChecker.CheckIfFileExists("Plugins/LSPDFR/LSPDFR+.dll", LSPDFRPlusVersion))
                    {
                        Game.LogTrivial("LSPDFR+ is out of date for BPS. Aborting. Required version: " + LSPDFRPlusVersion.ToString());
                        Game.DisplayNotification("~r~~H~BritishP.S. detected LSPDFR+ version lower than ~b~" + LSPDFRPlusVersion.ToString());
                        ExtensionMethods.DisplayPopupTextBoxWithConfirmation("British Policing Script Dependencies", "BritishP.S. didn't detect LSPDFR+ or detected LSPDFR+ version lower than " + LSPDFRPlusVersion.ToString() + ". Please install the appropriate version of LSPDFR+ (link on the BPS download page under Requirements). Unloading British Policing Script...", true);
                        return;
                    }
                    GameFiber.StartNew(delegate
                    {
                        int WaitCount = 0;
                        while (!IsLSPDFRPluginRunning("Traffic Policer", TrafficPolicerVersion) || !IsLSPDFRPluginRunning("Arrest Manager", ArrestManagerVersion) || !IsLSPDFRPluginRunning("LSPDFR+", LSPDFRPlusVersion))
                        {
                            GameFiber.Yield();
                            WaitCount++;
                            if (WaitCount > 1500)
                            {

                                Game.DisplayNotification("B.P.Script unable to find correct version Traffic Policer/Arrest Manager/LSPDFR+");
                                Game.LogTrivial("B.P.Script unable to find correct version of Traffic Police/Arrest Manager/LSPDFR+");
                                Game.LogTrivial("TP: " + IsLSPDFRPluginRunning("Traffic Policer", TrafficPolicerVersion).ToString());
                                Game.LogTrivial("AM: " + IsLSPDFRPluginRunning("Arrest Manager", ArrestManagerVersion).ToString());
                                Game.LogTrivial("LSPDFRPLUS: " + IsLSPDFRPluginRunning("LSPDFR + ", LSPDFRPlusVersion).ToString());
                                return;
                            }
                        }
                        EntryPoint.Initialise();
                        ;
                    });

                }
                else
                {

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
    }

}
