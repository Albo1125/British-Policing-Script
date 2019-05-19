using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Albo1125.Common.CommonLibrary;
using Rage;
using Rage.Native;
using RAGENativeUI;
using RAGENativeUI.Elements;
using RAGENativeUI.PauseMenu;

namespace British_Policing_Script
{
    internal static class Menus
    {
        private static readonly MenuPool MenuPool = new MenuPool();
        
        private static UIMenu ChecksMenu;
        private static UIMenuItem CheckNameItem;
        private static UIMenuItem CheckInsuranceItem;
        private static UIMenuItem CheckCourtResultsItem;

        public static TabView CourtsMenu;
        public static TabSubmenuItem PendingResultsList;
        public static TabSubmenuItem PublishedResultsList;

        private static readonly List<TabItem> EmptyItems = new List<TabItem> {new TabItem(" ")};

        public static void InitialiseMenus()
        {
            InitializeMenuPool();
            InitializeCourtsMenu();

            Game.FrameRender += Process;
            MainLogic();
        }

        private static void InitializeMenuPool()
        {
            MenuPool.Add(ChecksMenu = new UIMenu("Checks", ""));

            ChecksMenu.AddItem(CheckNameItem = new UIMenuItem("Name/Vehicle Records Check"));
            ChecksMenu.AddItem(CheckInsuranceItem = new UIMenuItem("Insurance Checks"));
            ChecksMenu.AddItem(CheckCourtResultsItem = new UIMenuItem("Magistrates' Court Results"));

            ChecksMenu.OnItemSelect += OnItemSelect;
            ChecksMenu.MouseControlsEnabled = false;
            ChecksMenu.AllowCameraMovement = true;

            ChecksMenu.RefreshIndex();
        }

        private static void InitializeCourtsMenu()
        {
            CourtsMenu = new TabView("~b~~h~San Andreas Magistrates' Court");

            // use "AddTab" rather than "Tabs.Add" as it will throw an exception in "TabSubmenuItem.ProcessControls"
            // because the "Parent" won't be set in the submenu items causing a "NullPointerException"
            CourtsMenu.AddTab(PendingResultsList = new TabSubmenuItem("Pending Results", EmptyItems));
            CourtsMenu.AddTab(PublishedResultsList = new TabSubmenuItem("Results", EmptyItems));

            CourtsMenu.RefreshIndex();
        }

        private static void OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (sender != ChecksMenu)
                return;

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

        private static void MainLogic()
        {
            GameFiber.StartNew(delegate
            {
                try
                {
                    while (true)
                    {
                        GameFiber.Yield();
                        if (ExtensionMethods.IsKeyDownComputerCheck(EntryPoint.ToggleMenuKey) &&
                            (ExtensionMethods.IsKeyDownRightNowComputerCheck(EntryPoint.ToggleMenuModifierKey) ||
                             EntryPoint.ToggleMenuModifierKey == Keys.None))
                        {
                            ChecksMenu.Visible = !ChecksMenu.Visible;
                        }


                        if (MenuPool.IsAnyMenuOpen())
                        {
                            NativeFunction.Natives.SET_PED_STEALTH_MOVEMENT(Game.LocalPlayer.Character, 0, 0);
                        }

                        if (CourtsMenu.Visible)
                        {
                            if (ExtensionMethods.IsKeyDownComputerCheck(Keys.Delete))
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
                catch (ThreadAbortException e)
                {
                }
                catch (Exception e)
                {
                    Game.LogTrivial(e.ToString());
                }
            });
        }

        private static void Process(object sender, GraphicsEventArgs e)
        {
            try
            {
                MenuPool.ProcessMenus();

                if (CourtsMenu.Visible)
                    CourtsMenu.Update();
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"An exception occurred while processing the menu with error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}