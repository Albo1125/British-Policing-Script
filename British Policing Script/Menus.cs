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
using Rage.Native;
using RAGENativeUI.PauseMenu;

namespace British_Policing_Script
{
    internal static class Menus
    {
        private static UIMenu ChecksMenu;

        private static UIMenuItem CheckNameItem;
        //private static UIMenuItem CheckPlateItem;
        private static UIMenuItem CheckInsuranceItem;
        private static UIMenuItem CheckCourtResultsItem;
        private static MenuPool _MenuPool;

        public static TabView CourtsMenu;

        public static TabSubmenuItem PendingResultsList;
        public static TabSubmenuItem PublishedResultsList;

        public static List<TabItem> EmptyItems = new List<TabItem>() { new TabItem(" ") };        

        public static void InitialiseMenus()
        {


            //bigMessage = new BigMessageThread(true);
            Game.FrameRender += Process;
            _MenuPool = new MenuPool();
            ChecksMenu = new UIMenu("Checks", "");
            _MenuPool.Add(ChecksMenu);            
           
            ChecksMenu.AddItem(CheckNameItem = new UIMenuItem("Name/Vehicle Records Check"));
            
            ChecksMenu.AddItem(CheckInsuranceItem = new UIMenuItem("Insurance Checks"));
            ChecksMenu.AddItem(CheckCourtResultsItem = new UIMenuItem("Magistrates' Court Results"));
            //ChecksMenu.SetMenuWidthOffset(0)
            ChecksMenu.RefreshIndex();
            ChecksMenu.OnItemSelect += OnItemSelect;
            //ChecksMenu.OnListChange += OnListChange;
            //_MenuPool.ProcessMenus();
            ChecksMenu.MouseControlsEnabled = false;
            ChecksMenu.AllowCameraMovement = true;

            CourtsMenu = new TabView("~b~~h~San Andreas Magistrates' Court");

            
            
            CourtsMenu.Tabs.Add(PendingResultsList = new TabSubmenuItem("Pending Results", EmptyItems));
            CourtsMenu.Tabs.Add(PublishedResultsList = new TabSubmenuItem("Results", EmptyItems));
            
            CourtsMenu.RefreshIndex();
            MainLogic();
            //CourtSystem.CreateNewCourtCase("Zach Houseknecht", new DateTime(1990, 2, 1), "speeding at 82MPH in a 30MPH limit", DateTime.Now.AddDays(-1), true, "Fined $500 and licence suspended for 56 days.", DateTime.Now, true);
            //CourtSystem.CreateNewCourtCase("Jeff Favignano", new DateTime(1990, 2, 1), "Armed Robbery", DateTime.Now, true, "Sentenced to 6 years in prison", DateTime.Now.AddDays(1), false);



        }
        private static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender == ChecksMenu)
            {
                if (selectedItem == CheckNameItem)
                {
                    RadioChecker.DisplayKeyboard_RunCheckOnName();
                }
                
                else if (selectedItem == CheckInsuranceItem)
                {
                    //Insurance checks
                    RadioChecker.DisplayKeyboard_RunInsuranceCheckOnName();
                }
                else if (selectedItem == CheckCourtResultsItem)
                {
                    sender.Visible = false;
                    CourtsMenu.Visible = true;
                    
                }
            }
           
        }

        private static void MainLogic()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(EntryPoint.ToggleMenuKey) && (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.ToggleMenuModifierKey) || EntryPoint.ToggleMenuModifierKey == Keys.None))
                        {
                            ChecksMenu.Visible = !ChecksMenu.Visible;
                        }


                        
                        if (_MenuPool.IsAnyMenuOpen()) { Rage.Native.NativeFunction.Natives.SET_PED_STEALTH_MOVEMENT(Game.LocalPlayer.Character, 0, 0); }

                        if (CourtsMenu.Visible)
                        {


                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(Keys.Delete))
                            {
                                if (PendingResultsList.Active)
                                {
                                    if (CourtCase.PendingResultsMenuCleared)
                                    {
                                        CourtSystem.DeleteCourtCase(CourtSystem.PendingCourtCases[PendingResultsList.Index]);
                                        PendingResultsList.Index = 0;
                                    }
                                }
                                else if (PublishedResultsList.Active)
                                {
                                    if (CourtCase.ResultsMenuCleared)
                                    {
                                        CourtSystem.DeleteCourtCase(CourtSystem.PublishedCourtCases[PublishedResultsList.Index]);
                                        PublishedResultsList.Index = 0;
                                    }
                                }
                            }
                        }


                    }
                }
                catch (System.Threading.ThreadAbortException e) { }
                catch (Exception e) { Game.LogTrivial(e.ToString()); }
            });
        }


        




        public static float TrafficStopMenuDistance = 3.7f;
        //static bool pulloveractive = false;
        private static void Process(object sender, GraphicsEventArgs e)
        {
            _MenuPool.ProcessMenus();
            if (CourtsMenu.Visible)
            {
                
                CourtsMenu.Update();
            }
        }

        private static void ToggleUIMenuEnabled(UIMenu menu, bool Enabled)
        {

            foreach (UIMenuItem item in menu.MenuItems)
            {
                item.Enabled = Enabled;                
            }

        }
    }
}
