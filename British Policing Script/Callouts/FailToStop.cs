using System;
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
using Albo1125.Common.CommonLibrary;

namespace British_Policing_Script.Callouts
{
    [CalloutInfo("Fail to stop", CalloutProbability.Medium)]
    internal class FailToStop : BritishCallout
    {
        
        private SuspectStates SuspectState;
        private SuspectStates PassengerState;
        private bool CalloutRunning = false;
        private Ped Passenger;
        private string msg;
        
        

        public override bool OnBeforeCalloutDisplayed()
        {
            Game.LogTrivial("Creating BritishCallout.FailToStop");
            if (!FindSpawnPoint(325, 375, 180)) { return false; }


            ShowCalloutAreaBlipBeforeAccepting(sp, 70f);
            CalloutMessage = "Fail to Stop";
            CalloutPosition = sp;
            

            Functions.PlayScannerAudioUsingPosition("WE_HAVE_01 BRITISHCRIME_FAILTOSTOP IN_OR_ON_POSITION", sp);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            SuspectCar = new Vehicle(GroundVehiclesToSelectFrom[EntryPoint.rnd.Next(GroundVehiclesToSelectFrom.Length)], sp,sp);
            SuspectCar.IsPersistent = true;
            Suspect = SuspectCar.CreateRandomDriver();
            Suspect.MakeMissionPed();
            NativeFunction.Natives.SET_VEHICLE_FORWARD_SPEED(Suspect.CurrentVehicle, 20f);
            Suspect.Tasks.CruiseWithVehicle(50f, VehicleDrivingFlags.Emergency);
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
                    
                    GameFiber.Wait(1000);
                    Pursuit = Functions.CreatePursuit();
                    SuspectBritishPersona = BritishPersona.GetBritishPersona(Suspect);
                    
                    
                    Functions.AddPedToPursuit(Pursuit, Suspect);
                    
                    if (EntryPoint.rnd.Next(3) == 0)
                    {
                        Passenger = new Ped(Vector3.Zero);
                        Passenger.MakeMissionPed();

                        Passenger.WarpIntoVehicle(SuspectCar, 0);
                        Functions.AddPedToPursuit(Pursuit, Passenger);
                    }
                    
                    Functions.SetPursuitIsActiveForPlayer(Pursuit, true);
                    
                    GameFiber.Wait(3000);
                    Vehicle Backupveh = Functions.RequestBackup(Suspect.Position, LSPD_First_Response.EBackupResponseType.Pursuit, LSPD_First_Response.EBackupUnitType.LocalUnit);
                    Backupveh.Position = sp;
                    Backupveh.Heading = sp;
                    Game.DisplayNotification("~b~Pursuing Officer: ~s~Suspect is on ~b~" + World.GetStreetName(Suspect.Position) + ". ~s~Speed is ~r~" + Math.Round(MathHelper.ConvertMetersPerSecondToMilesPerHour(Suspect.Speed)).ToString() + " MPH.");
                    SuspectBlip = Suspect.AttachBlip();
                    SuspectBlip.Scale = 0.1f;
                    SuspectBlip.IsRouteEnabled = true;
                    SuspectBlip.RouteColor = Color.Red;



                    if (!Passenger.Exists())
                    {
                        while (CalloutRunning)
                        {
                            GameFiber.Yield();
                            
                            Rage.Native.NativeFunction.Natives.SET_AI_MELEE_WEAPON_DAMAGE_MODIFIER( 5.5f);
                            if (!Suspect.Exists())
                            {
                                msg = "Control, the ~r~suspect~s~ has ~r~escaped.~s~ I'm state 2, over.";
                                break;
                            }
                            else if (Functions.IsPedArrested(Suspect))
                            {
                                msg = "Control, the ~r~suspect~s~ is ~g~under arrest.~s~ I'm state 2, over.";
                                string sentence;
                                if (EntryPoint.rnd.Next(6) < 3)
                                {
                                    int HoursUnpaidWork = (int)Math.Round(((float)EntryPoint.rnd.Next(150, 300)) / 5.0f) * 5;

                                    int Costs = (int)Math.Round(((float)EntryPoint.rnd.Next(165)) / 5.0f) * 5;
                                    sentence = "Community order made with " + HoursUnpaidWork.ToString() + " hours unpaid work. Disqualified from driving for " + EntryPoint.rnd.Next(12, 24).ToString() + " months. " + Costs.ToString() + " pounds in costs.";
                                }
                                else
                                {
                                    int JailMonths = (int)Math.Round(((float)EntryPoint.rnd.Next(4, 19)) / 5.0f) * 5;
                                    int Costs = (int)Math.Round(((float)EntryPoint.rnd.Next(165)) / 5.0f) * 5;
                                    sentence = "Sentenced to  " + JailMonths.ToString() + " months in prison. Disqualified from driving for " + EntryPoint.rnd.Next(12, 24).ToString() + " months. " + Costs.ToString() + " pounds in costs.";
                                }
                                
                                
                                
                                CourtSystem.CreateNewCourtCase(SuspectBritishPersona, "failing to stop for police and dangerous driving", 100, sentence);
                                break;
                            }
                            else if (Suspect.IsDead)
                            {
                                msg = "Control, the ~r~suspect~s~ is ~o~dead.~s~ I'm state 2, over.";
                                break;
                            }

                            if (Vector3.Distance(Suspect.Position, Game.LocalPlayer.Character.Position) < 60f)
                            {
                                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            }
                        }
                    }
                    else if (CalloutRunning)
                    {
                        Functions.AddPedToPursuit(Pursuit, Passenger);
                        PassengerState = SuspectStates.InPursuit;
                        SuspectState = SuspectStates.InPursuit;

                        while (CalloutRunning)
                        {
                            GameFiber.Yield();
                            
                            if (SuspectState == SuspectStates.InPursuit)
                            {
                                if (!Suspect.Exists())
                                {
                                    SuspectState = SuspectStates.Escaped;
                                }
                                else if (Suspect.IsDead)
                                {
                                    SuspectState = SuspectStates.Dead;
                                }
                                else if (Functions.IsPedArrested(Suspect))
                                {
                                    SuspectState = SuspectStates.Arrested;
                                }
                            }



                            if (PassengerState == SuspectStates.InPursuit)
                            {
                                if (!Passenger.Exists())
                                {
                                    PassengerState = SuspectStates.Escaped;
                                }
                                else if (Passenger.IsDead)
                                {
                                    PassengerState = SuspectStates.Dead;
                                }
                                else if (Functions.IsPedArrested(Passenger))
                                {
                                    PassengerState = SuspectStates.Arrested;
                                }



                            }
                            if ((SuspectState != SuspectStates.InPursuit) && (PassengerState != SuspectStates.InPursuit))
                            {
                                break;
                            }

                            if (Vector3.Distance(Suspect.Position, Game.LocalPlayer.Character.Position) < 60f)
                            {
                                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                            }
                        }
                        msg = "Control, the driver's ";
                        if (SuspectState == SuspectStates.Arrested)
                        {
                            msg += "~g~under arrest.";
                        }
                        else if (SuspectState == SuspectStates.Dead)
                        {
                            msg += "~o~dead.";
                        }
                        else if (SuspectState == SuspectStates.Escaped)
                        {
                            msg += "~r~escaped.";
                        }

                        msg += "~s~ The passenger's ";
                        if (PassengerState == SuspectStates.Arrested)
                        {
                            msg += "~g~under arrest.";
                        }
                        else if (PassengerState == SuspectStates.Dead)
                        {
                            msg += "~o~dead.";
                        }
                        else if (PassengerState == SuspectStates.Escaped)
                        {
                            msg += "~r~escaped.";
                        }
                        msg += "~s~ I'm state 2, over.";
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
                        Game.DisplayNotification("~O~Failtostop~s~ callout crashed, sorry. Please send me your log file.");
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
                if (Passenger.Exists()) { Passenger.Dismiss(); }
            }
            else
            {
                if (Passenger.Exists()) { Passenger.Delete(); }
            }
            base.End();
        }
    }
}
