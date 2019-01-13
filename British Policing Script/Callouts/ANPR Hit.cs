using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using Rage;
using Albo1125.Common.CommonLibrary;
using Rage.Native;
using System.Drawing;

namespace British_Policing_Script.Callouts
{
    [CalloutInfo("ANPR Hit", CalloutProbability.High)]
    internal class ANPRHit : BritishCallout
    {
        private bool CalloutRunning = false;
        enum ANPRHitTypes { NoInsurance, NoMOT, NoTax, DisqualifiedOwner, Stolen, RevokedLicence }
        private bool PursuitCreated = false;
        private string msg = "ANPR Hit dealt with. Show me state 2, over.";
        ANPRHitTypes HitType;

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("Creating BritishCallout.ANPR_Hit");
            
            if (!FindSpawnPoint(325, 375, 180)) { return false; }
            HitType = (ANPRHitTypes)Enum.GetValues(typeof(ANPRHitTypes)).GetValue(EntryPoint.rnd.Next(Enum.GetValues(typeof(ANPRHitTypes)).Length));
            
            CalloutMessage = "~o~ANPR Hit: ~r~" + ANPRHitReasonString(HitType);
            CalloutPosition = sp;
            Functions.PlayScannerAudioUsingPosition("WE_HAVE_01 BRITISHCRIME_ANPRHIT IN_OR_ON_POSITION", sp);
            ShowCalloutAreaBlipBeforeAccepting(sp, 200f);

            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            SuspectCar = new Vehicle(GroundVehiclesToSelectFrom[EntryPoint.rnd.Next(GroundVehiclesToSelectFrom.Length)], sp, sp);
            SuspectCar.IsPersistent = true;
            Suspect = SuspectCar.CreateRandomDriver();
            Suspect.MakeMissionPed();
            NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(Suspect.CurrentVehicle, 20f);
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
                    Suspect.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.AllowMedianCrossing | VehicleDrivingFlags.YieldToCrossingPedestrians);
                    SuspectBritishPersona = BritishPersona.GetBritishPersona(Suspect);
                    SuspectCarRecords = VehicleRecords.GetVehicleRecords(SuspectCar);
                    SetANPRHitDetails(HitType);
                    Game.DisplayNotification("~b~Control: ~o~ANPR Hit is on a ~b~" + SuspectCarRecords.CarColour + "~b~ " + SuspectCarRecords.ModelName + "~s~, plate ~b~" + SuspectCarRecords.LicencePlate + ".");
                    GameFiber.Wait(4000);
                    
                    GameFiber.Wait(2000);
                    Game.DisplayHelp("Locate and stop the suspect's vehicle.");
                    HandleSearchForVehicleWithANPR();
                    BeforeTrafficStopDrive();
                    while (CalloutRunning)
                    {
                        GameFiber.Yield();
                        if (!Functions.IsPlayerPerformingPullover() && !PursuitCreated)
                        {
                            GameFiber.Wait(1000);
                            if (Functions.GetActivePursuit() == null)
                            {
                                break;
                            }
                            else
                            {
                                PursuitCreated = true;
                                Pursuit = Functions.GetActivePursuit();
                            }
                        }
                        if (PursuitCreated && !Functions.IsPursuitStillRunning(Pursuit))
                        {
                            break;
                        }
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
                        Game.DisplayNotification("~O~ANPRHIT~s~ callout crashed, sorry. Please send me your log file.");
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
                //if (Passenger.Exists()) { Passenger.Dismiss(); }
            }
            else
            {
                //if (Passenger.Exists()) { Passenger.Delete(); }
            }
            base.End();
        }

        private void BeforeTrafficStopDrive()
        {
            while (CalloutRunning)
            {

                GameFiber.Yield();
                Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(Suspect, 786603);
                if (Functions.IsPlayerPerformingPullover() && Functions.GetPulloverSuspect(Functions.GetCurrentPullover()) == Suspect)
                {


                    break;

                }

                if (Vector3.Distance(Game.LocalPlayer.Character.Position, SuspectCar.Position) < 13f)
                {
                    if (!Game.LocalPlayer.Character.IsInAnyVehicle(false))
                    {

                        break;
                    }
                }
            }
        }

        private string ANPRHitReasonString(ANPRHitTypes HitType)
        {
            string s = "";
            if (HitType == ANPRHitTypes.DisqualifiedOwner)
            {
                s = "Owner Disqualified";
            }
            else if (HitType == ANPRHitTypes.RevokedLicence)
            {
                s = "Owner's Licence Revoked";
            }
            else if (HitType == ANPRHitTypes.NoInsurance)
            {
                s = "No Insurance";
            }
            else if (HitType == ANPRHitTypes.NoMOT)
            {
                s = "No MOT";
            }
            else if (HitType == ANPRHitTypes.NoTax)
            {
                s = "No Tax";
            }
            else if (HitType == ANPRHitTypes.Stolen)
            {
                s = "Reported Stolen";
            }
            return s;
        }

        private void SetANPRHitDetails(ANPRHitTypes HitType)
        {
            if (HitType == ANPRHitTypes.DisqualifiedOwner)
            {
                SuspectBritishPersona.LicenceStatus = BritishPersona.LicenceStatuses.Disqualified;
            }
            else if (HitType == ANPRHitTypes.RevokedLicence)
            {
                SuspectBritishPersona.LicenceStatus = BritishPersona.LicenceStatuses.Revoked;
            }
            else if (HitType == ANPRHitTypes.NoInsurance)
            {
                SuspectCarRecords.Insured = false;
            }
            else if (HitType == ANPRHitTypes.NoMOT)
            {
                SuspectCarRecords.HasMOT = false;
            }
            else if (HitType == ANPRHitTypes.NoTax)
            {
                SuspectCarRecords.IsTaxed = false;
            }
            else if (HitType == ANPRHitTypes.Stolen)
            {
                SuspectCar.IsStolen = true;
            }
        }

        private void HandleSearchForVehicleWithANPR()
        {
            float Radius = 180f;
            SearchArea = new Blip(SuspectCar.Position.Around(35f), Radius);
            SearchArea.Color = System.Drawing.Color.Yellow;
            SearchArea.Alpha = 0.5f;
            int WaitCount = 0;
            int WaitCountTarget = 1900;
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
                    Radius -= 10f;
                    if (Radius < 110f) { Radius = 110f; }
                    SearchArea = new Blip(SuspectCar.Position.Around(5f, 15f), Radius);
                    SearchArea.Color = System.Drawing.Color.Yellow;
                    SearchArea.Alpha = 0.5f;


                    RouteEnabled = false;
                    if (WaitCount > WaitCountTarget) { Game.LogTrivial("Updated for waitcount"); }
                    WaitCount = 0;

                    Suspect.Tasks.CruiseWithVehicle(SuspectCar, 20f, VehicleDrivingFlags.DriveAroundVehicles | VehicleDrivingFlags.DriveAroundObjects | VehicleDrivingFlags.DriveAroundPeds);
                    WaitCountTarget -= EntryPoint.rnd.Next(200, 500);
                    if (WaitCountTarget < 1000) { WaitCountTarget = 1000; }
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

