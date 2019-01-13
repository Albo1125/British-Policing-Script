using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LSPD_First_Response.Mod.Callouts;
using LSPD_First_Response.Mod.API;
using Rage;
using Albo1125.Common.CommonLibrary;
using static Albo1125.Common.CommonLibrary.SpawnPointExtensions;

namespace British_Policing_Script.Callouts
{
    internal abstract class BritishCallout : Callout
    {
        public string[] GroundVehiclesToSelectFrom = new string[] {"DUKES", "BALLER", "BALLER2", "BISON", "BISON2", "BJXL", "CAVALCADE", "CHEETAH", "COGCABRIO", "ASEA", "ADDER", "FELON", "FELON2", "ZENTORNO",
        "WARRENER", "RAPIDGT", "INTRUDER", "FELTZER2", "FQ2", "RANCHERXL", "REBEL", "SCHWARZER", "COQUETTE", "CARBONIZZARE", "EMPEROR", "SULTAN", "EXEMPLAR", "MASSACRO",
        "DOMINATOR", "ASTEROPE", "PRAIRIE", "NINEF", "WASHINGTON", "CHINO", "CASCO", "INFERNUS", "ZTYPE", "DILETTANTE", "VIRGO", "F620", "PRIMO", "SULTAN", "EXEMPLAR", "F620", "FELON2", "FELON", "SENTINEL", "WINDSOR",
            "DOMINATOR", "DUKES", "GAUNTLET", "VIRGO", "ADDER", "BUFFALO", "ZENTORNO", "MASSACRO",
        "BATI", "BATI2", "AKUMA", "BAGGER", "DOUBLE", "NEMESIS", "HEXER"};
        public string[] CarsToSelectFrom = new string[] {"DUKES", "BALLER", "BALLER2", "BISON", "BISON2", "BJXL", "CAVALCADE", "CHEETAH", "COGCABRIO", "ASEA", "ADDER", "FELON", "FELON2", "ZENTORNO",
        "WARRENER", "RAPIDGT", "INTRUDER", "FELTZER2", "FQ2", "RANCHERXL", "REBEL", "SCHWARZER", "COQUETTE", "CARBONIZZARE", "EMPEROR", "SULTAN", "EXEMPLAR", "MASSACRO",
        "DOMINATOR", "ASTEROPE", "PRAIRIE", "NINEF", "WASHINGTON", "CHINO", "CASCO", "INFERNUS", "ZTYPE", "DILETTANTE", "VIRGO", "F620", "PRIMO", "SULTAN", "EXEMPLAR", "F620", "FELON2", "FELON", "SENTINEL", "WINDSOR",
            "DOMINATOR", "DUKES", "GAUNTLET", "VIRGO", "ADDER", "BUFFALO", "ZENTORNO", "MASSACRO" };
        public string[] MotorBikesToSelectFrom = new string[] { "BATI", "BATI2", "AKUMA", "BAGGER", "DOUBLE", "NEMESIS", "HEXER" };
        public Ped Suspect;
        public Vehicle SuspectCar;
        public SpawnPoint sp;
        public Blip SuspectBlip;
        public LHandle Pursuit;
        public bool CalloutFinished = false;
        public Vector3 SearchAreaLocation;
        public Blip SearchArea;
        public enum SuspectStates { InPursuit, Arrested, Dead, Escaped };
        public BritishPersona SuspectBritishPersona;
        public VehicleRecords SuspectCarRecords;

        public override void OnCalloutNotAccepted()
        {

            base.OnCalloutNotAccepted();
            if (Suspect.Exists()) { Suspect.Delete(); }
            if (SuspectCar.Exists()) { SuspectCar.Delete(); }
            if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
            //if (EntryPoint.OtherUnitTakingCallAudio)
            //{
            //    Functions.PlayScannerAudio("OTHER_UNIT_TAKING_CALL");
            //}

        }
        public override bool OnBeforeCalloutDisplayed()
        {

            return base.OnBeforeCalloutDisplayed();
        }


        public override void End()
        {
            base.End();
            try
            {
                if (SearchArea.Exists()) { SearchArea.Delete(); }
                if (SuspectBlip.Exists()) { SuspectBlip.Delete(); }
                if (!CalloutFinished)
                {
                    if (Suspect.Exists()) { Suspect.Delete(); }
                    if (SuspectCar.Exists()) { SuspectCar.Delete(); }

                }
                else
                {
                    if (Suspect.Exists()) { Suspect.Dismiss(); }
                    if (SuspectCar.Exists()) { SuspectCar.Dismiss(); }

                }
            }
            catch (Exception e) { }
        }

        protected bool FindSpawnPoint(float MinDistance, float MaxDistance, float MinimumDistanceCheck)
        {
            int WaitCount = 0;
            while (true)
            {
                if (World.GetNextPositionOnStreet(Game.LocalPlayer.Character.Position.Around(MinDistance, MaxDistance)).GetClosestVehicleSpawnPoint(out sp))
                {
                    if (Vector3.Distance(Game.LocalPlayer.Character.Position, sp) > MinimumDistanceCheck)
                    {
                        return true;
                    }
                }
                GameFiber.Yield();
                WaitCount++;
                if (WaitCount > 10)
                {
                    Game.LogTrivial("No valid spawnpoint found");
                    return false;
                }
            }
        }
    }
}
