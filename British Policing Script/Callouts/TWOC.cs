using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using Rage;

using System.Drawing;
using Rage.Native;
using Albo1125.Common;
using System;
using Albo1125.Common.CommonLibrary;

namespace British_Policing_Script.Callouts
{
    [CalloutInfo("TWOC", CalloutProbability.Medium)]
    internal class TWOC : BritishCallout
    {
        private bool CalloutRunning = false;
        private string msg;

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("Creating BritishCallout.TWOC");
            
            if (!FindSpawnPoint(400,475,180)) { return false; }
            

            ShowCalloutAreaBlipBeforeAccepting(sp.Position.Around(30f, 50f), 180f);
            CalloutMessage = "TWOC";
            CalloutPosition = sp;


            Functions.PlayScannerAudioUsingPosition("WE_HAVE_01 BRITISHCRIME_TWOC IN_OR_ON_POSITION", sp);
            return base.OnBeforeCalloutDisplayed();
        }
        public override bool OnCalloutAccepted()
        {
            CalloutHandler();


            return base.OnCalloutAccepted();
        }
        private void CalloutHandler()
        {
            CalloutRunning = true;
            GameFiber.StartNew(delegate
            {

                try
                {
                    SuspectCar = new Vehicle(GroundVehiclesToSelectFrom[EntryPoint.rnd.Next(GroundVehiclesToSelectFrom.Length)], sp, sp);
                    SuspectCar.IsPersistent = true;
                    Suspect = SuspectCar.CreateRandomDriver();
                    Suspect.MakeMissionPed();
                    SuspectBritishPersona = BritishPersona.GetBritishPersona(Suspect);
                    SuspectCarRecords = VehicleRecords.GetVehicleRecords(SuspectCar);
                    SuspectCarRecords.RegisteredOwner = BritishPersona.GetRandomBritishPersona();
                    SuspectCarRecords.AddCustomFlag("~r~WANTED FOR TWOC ");
                    Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    GameFiber.Wait(2000);
                    Game.DisplayNotification("~b~Control: ~s~TWOC report's on a ~b~" + SuspectCarRecords.CarColour + "~b~ " + SuspectCarRecords.ModelName + "~s~. Licence plate: ~b~" + SuspectCarRecords.LicencePlate + ".");
                    GameFiber.Wait(4000);
                    Game.DisplayNotification("Vehicle has just been taken from ~b~" + World.GetStreetName(SuspectCar.Position) + "~s~. ~b~" + SuspectCarRecords.LicencePlate + "~s~ added to ~o~ANPR system.");
                    GameFiber.Wait(2000);
                    Game.DisplayHelp("Locate and stop the suspect's vehicle.");
                    HandleSearchForVehicleWithANPR();
                    while (CalloutRunning)
                    {
                        GameFiber.Yield();
                        if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                        {
                            break;
                        }
                        if (Functions.IsPlayerPerformingPullover())
                        {

                            if (Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == Suspect)

                            {
                                break;
                            }
                        }
                    }
                    if (Functions.IsPlayerPerformingPullover())
                    {
                        GameFiber.Wait(4000);
                    }
                    if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                    if ((EntryPoint.rnd.Next(11) <= 6) || (!Game.LocalPlayer.Character.IsInAnyVehicle(false)))
                    {

                        Pursuit = Functions.CreatePursuit();
                        Functions.AddPedToPursuit(Pursuit, Suspect);
                        Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                        if (Functions.IsPlayerPerformingPullover()) { Functions.ForceEndCurrentPullover(); }
                        Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESIST_ARREST IN_OR_ON_POSITION", Game.LocalPlayer.Character.Position);

                        Game.DisplayNotification("Control, the vehicle is ~r~making off.~b~ Giving chase.");
                        

                        while (Functions.IsPursuitStillRunning(Pursuit))
                        {
                            GameFiber.Yield();
                            if (!CalloutRunning) { break; }
                        }
                    }
                    else
                    {
                        while (CalloutRunning)
                        {
                            GameFiber.Yield();
                            if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                            {
                                GameFiber.Wait(1000);
                                if (EntryPoint.rnd.Next(5) == 0)
                                {
                                    Pursuit = Functions.CreatePursuit();
                                    Functions.AddPedToPursuit(Pursuit, Suspect);
                                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                                    if (Functions.IsPlayerPerformingPullover()) { Functions.ForceEndCurrentPullover(); }

                                    Functions.PlayScannerAudioUsingPosition("WE_HAVE CRIME_RESIST_ARREST IN_OR_ON_POSITION", Game.LocalPlayer.Character.Position);

                                    Game.DisplayNotification("Control, the vehicle is ~r~making off.~b~ Giving chase.");
                                    
                                    
                                }
                                break;

                            }
                        }
                        while (CalloutRunning)
                        {
                            GameFiber.Yield();
                            if (Suspect.Exists())
                            {
                                if (Functions.IsPedArrested(Suspect))
                                {
                                    break;
                                }
                                if (Suspect.IsDead)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    if (Suspect.Exists())
                    {
                        if (Functions.IsPedArrested(Suspect))
                        {
                            msg = "Control, suspect is ~g~under arrest. ~s~Show me state 2, over.";
                            int HoursUnpaidWork = (int)Math.Round(((float)EntryPoint.rnd.Next(100, 200)) / 5.0f) * 5;
                            int Costs = (int)Math.Round(((float)EntryPoint.rnd.Next(86)) / 5.0f) * 5;
                            string sentence = " Community order made with " + HoursUnpaidWork.ToString() + " hours unpaid work. Disqualified from driving for " + EntryPoint.rnd.Next(4, 13).ToString() + " months. " + Costs.ToString() + " pounds in costs.";
                            CourtSystem.CreateNewCourtCase(SuspectBritishPersona, "taking a vehicle without the owner's consent", 100, sentence);
                        }
                        else if (Suspect.IsDead)
                        {
                            msg = "Control, suspect is ~r~dead. ~s~Show me state 2, over";
                        }
                    }
                    else
                    {
                        msg = "Control, the suspects ~r~have escaped. ~s~Show me state 2, over";
                    }
                    DisplayCodeFourMessage();

                }
                catch (System.Threading.ThreadAbortException e)
                {
                    End();
                }
                catch (Exception e)
                {

                    if (CalloutRunning)
                    {
                        Game.LogTrivial(e.ToString());
                        Game.LogTrivial("British Policing Script handled the exception successfully.");
                        Game.DisplayNotification("~O~TWOC~s~ callout crashed, sorry. Please send me your log file.");
                        Game.DisplayNotification("Full LSPDFR crash prevented ~g~successfully.");
                        End();
                    }
                }
            });
        }
        private void DisplayCodeFourMessage()
        {
            if (CalloutRunning)
            {
                GameFiber.Sleep(4000);
                Game.DisplayNotification(msg);

                Functions.PlayScannerAudio("ATTENTION_THIS_IS_DISPATCH_HIGH NO_FURTHER_UNITS_REQUIRED");
                CalloutFinished = true;
                End();
            }
        }
        public override void Process()
        {
            base.Process();
            if (Game.LocalPlayer.Character.Exists())
            {
                if (Game.LocalPlayer.Character.IsDead)
                {

                    GameFiber.StartNew(End);
                }
            }
            else
            {
                GameFiber.StartNew(End);
            }
        }
        public override void End()
        {

            CalloutRunning = false;

            if (Game.LocalPlayer.Character.Exists())
            {
                if (Game.LocalPlayer.Character.IsDead)
                {
                    GameFiber.Wait(1500);
                    Functions.PlayScannerAudio("OFFICER HAS_BEEN_FATALLY_SHOT NOISE_SHORT OFFICER_NEEDS_IMMEDIATE_ASSISTANCE");
                    GameFiber.Wait(3000);


                }
            }
            else
            {
                GameFiber.Wait(1500);
                Functions.PlayScannerAudio("OFFICER HAS_BEEN_FATALLY_SHOT NOISE_SHORT OFFICER_NEEDS_IMMEDIATE_ASSISTANCE");
                GameFiber.Wait(3000);
            }
            if (CalloutFinished)
            {
               
            }
            else
            {
                
            }
            base.End();
        }

        private void HandleSearchForVehicleWithANPR()
        {
            float Radius = 160f;
            SearchArea = new Blip(SuspectCar.Position.Around(35f), Radius);
            SearchArea.Color = System.Drawing.Color.Yellow;
            SearchArea.Alpha = 0.5f;
            int WaitCount = 0;
            int WaitCountTarget = 2100;
            bool RouteEnabled = false;
            int audiocount = 0;
            while (CalloutRunning)
            {
                GameFiber.Yield();
                WaitCount++;
                Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(Suspect, 786603);

                if (Functions.IsPlayerPerformingPullover())
                {
                    if (Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == Suspect)
                    {
                        Game.DisplayNotification("Control, I have located the ~b~" + SuspectCarRecords.ModelName + ".");
                        Game.DisplayNotification("I'm preparing to ~b~stop them,~s~ over.");
                        SuspectBlip = Suspect.AttachBlip();
                        if (SearchArea.Exists()) { SearchArea.Delete(); }
                        Functions.PlayScannerAudio("BRITISH_DISPATCH_SUSPECT_LOCATED_ENGAGE");
                        break;
                    }
                }


                if (Vector3.Distance(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront * 9f), SuspectCar.Position) < 9f)
                {
                    GameFiber.Sleep(3000);
                    if (Vector3.Distance(Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront * 9f), SuspectCar.Position) < 9f)
                    {
                        Game.DisplayNotification("Control, I have located the ~b~" + SuspectCarRecords.ModelName + ".");
                        Game.DisplayNotification("I'm preparing to ~b~stop them,~s~ over.");
                        SuspectBlip = Suspect.AttachBlip();
                        if (SearchArea.Exists()) { SearchArea.Delete(); }
                        Functions.PlayScannerAudio("BRITISH_DISPATCH_SUSPECT_LOCATED_ENGAGE");

                        break;
                    }

                }
                else if (((Vector3.Distance(SuspectCar.Position, SearchArea.Position) > Radius + 20f) && (WaitCount > 400)) || (WaitCount > WaitCountTarget))
                {
                    Game.DisplayNotification("~b~Control: ~s~We have an ~o~ANPR Hit ~s~on the ~b~" + SuspectCarRecords.CarColour + " " + SuspectCarRecords.ModelName + ", ~s~plate ~b~" + SuspectCarRecords.LicencePlate + ".");
                    audiocount++;
                    if (audiocount >= 3)
                    {
                        Functions.PlayScannerAudioUsingPosition("WE_HAVE_01 CRIME_TRAFFIC_ALERT IN_OR_ON_POSITION", SuspectCar.Position);
                        audiocount = 0;
                    }
                    SearchArea.Delete();
                    Radius -= 20f;
                    if (Radius < 90f) { Radius = 90f; }
                    SearchArea = new Blip(SuspectCar.Position.Around(5f, 15f), Radius);
                    SearchArea.Color = System.Drawing.Color.Yellow;
                    SearchArea.Alpha = 0.5f;
                    

                    RouteEnabled = false;
                    if (WaitCount > WaitCountTarget) { Game.LogTrivial("Updated for waitcount"); }
                    WaitCount = 0;
                    
                    Suspect.Tasks.CruiseWithVehicle(SuspectCar, 20f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.DriveAroundPeds);
                    WaitCountTarget -= EntryPoint.rnd.Next(200, 500);
                    if (WaitCountTarget < 1200) { WaitCountTarget = 1200; }
                    SuspectBlip = new Blip(Suspect.Position);
                    SuspectBlip.Color = Color.Red;
                    GameFiber.Wait(4000);
                    SuspectBlip.Delete();

                }
                if (Vector3.Distance(Game.LocalPlayer.Character.Position, SearchArea.Position) > Radius + 90f)
                {
                    if (!RouteEnabled)
                    {
                        SearchArea.IsRouteEnabled = true;
                        RouteEnabled = true;
                    }
                }
                else
                {
                    if (RouteEnabled)
                    {
                        SearchArea.IsRouteEnabled = false;
                        RouteEnabled = false;
                    }
                }
            }
        }
    }

}
