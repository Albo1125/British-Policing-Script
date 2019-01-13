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
using LSPD_First_Response.Engine.Scripting.Entities;
using Rage.Native;

namespace British_Policing_Script
{
    internal static class RadioChecker
    {
        
        public static bool IsKeyboardActive = false;
        public static void DisplayKeyboard_RunCheckOnName()
        {
            if (!IsKeyboardActive)
            {
                GameFiber.StartNew(delegate
                {
                    IsKeyboardActive = true;
                    Traffic_Policer.API.Functions.SetAutomaticVehicleDeatilsChecksEnabled(false);
                    NativeFunction.Natives.DISPLAY_ONSCREEN_KEYBOARD(0, "FMMC_KEY_TIP", "", "", "", "", "", 20);
                    Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);
                    while (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 0)
                    {
                        GameFiber.Yield();
                        //Game.LogTrivial("UPDATEONSCREENKEYBOARD: " + NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>().ToString());
                        if (Functions.IsPlayerPerformingPullover())
                        {
                            Game.DisplaySubtitle("~h~Stopped Vehicle: " + Functions.GetPulloverSuspect(Functions.GetCurrentPullover()).CurrentVehicle.LicensePlate, 50);
                        }
                    }
                    
                    IsKeyboardActive = false;
                    if (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 1)
                    {

                        string input = NativeFunction.Natives.GET_ONSCREEN_KEYBOARD_RESULT<string>();
                        input = input.ToLower();
                        //Game.LogTrivial("INPUT: " + input);
                        if (input.Any(char.IsDigit))
                        {
                            VehicleRecords.RunPlateCheckOnPlate(input);
                        }
                        else
                        {
                            BritishPersona.RunLicenceCheckOnName(input);
                        }
                    }
                    GameFiber.Wait(1000);
                    Game.LocalPlayer.Character.Tasks.Clear();
                    Traffic_Policer.API.Functions.SetAutomaticVehicleDeatilsChecksEnabled(true);

                });
            }
        }

        public static void DisplayKeyboard_RunInsuranceCheckOnName()
        {
            if (!IsKeyboardActive)
            {
                GameFiber.StartNew(delegate
                {
                    IsKeyboardActive = true;
                    Traffic_Policer.API.Functions.SetAutomaticVehicleDeatilsChecksEnabled(false);
                    NativeFunction.Natives.DISPLAY_ONSCREEN_KEYBOARD(0, "FMMC_KEY_TIP", "", "", "", "", "", 20);
                    Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.5f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask | AnimationFlags.Loop);
                    while (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 0)
                    {
                        GameFiber.Yield();
                        //Game.LogTrivial("UPDATEONSCREENKEYBOARD: " + NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>().ToString());
                        Game.DisplaySubtitle("~h~Enter a name", 50);
                    }
                    
                    IsKeyboardActive = false;
                    if (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 1)
                    {

                        string input = NativeFunction.Natives.GET_ONSCREEN_KEYBOARD_RESULT<string>();
                        input = input.ToLower();
                        //Game.LogTrivial("INPUT: " + input);
                        BritishPersona.RunInsuranceCheckOnName(input);
                    }
                    GameFiber.Wait(1000);
                    Game.LocalPlayer.Character.Tasks.Clear();
                    Traffic_Policer.API.Functions.SetAutomaticVehicleDeatilsChecksEnabled(true);
                });
            }
        }
        //[]
        //public static void DisplayKeyboard_RunCheckOnPlate()
        //{
        //    if (!IsKeyboardActive)
        //    {
        //        GameFiber.StartNew(delegate
        //        {
        //            IsKeyboardActive = true;
        //            Traffic_Policer.API.Functions.SetAutomaticInsuranceChecksEnabled(false);
        //            NativeFunction.Natives.DISPLAY_ONSCREEN_KEYBOARD(0, "FMMC_KEY_TIP", "", "", "", "", "", 20);
        //            while (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 0)
        //            {
        //                GameFiber.Yield();
        //                Game.LogTrivial("UPDATEONSCREENKEYBOARD: " + NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>().ToString());
        //            }
        //            IsKeyboardActive = false;
        //            if (NativeFunction.Natives.UPDATE_ONSCREEN_KEYBOARD<int>() == 1)
        //            {
        //                string plate = NativeFunction.Natives.GET_ONSCREEN_KEYBOARD_RESULT<string>();
        //                plate = plate.ToLower();
        //                Game.LogTrivial("Plate: " + plate);
                        
        //            }
        //            Traffic_Policer.API.Functions.SetAutomaticInsuranceChecksEnabled(true);

        //        });
        //    }
        //}
    }
}
