using System;
using System.Collections.Generic;
using System.Linq;
using Rage;

using Albo1125.Common.CommonLibrary;

namespace British_Policing_Script
{
    public class VehicleRecords
    {



        internal static List<VehicleRecords> AllVehicleRecords = new List<VehicleRecords>();

        
        internal static bool VerifyVehicleRecordsExist(string LicencePlate)
        {
            List<Vehicle> VehiclesWithLicencePlate = new List<Vehicle>();
            foreach (Vehicle vehi in World.GetAllVehicles())
            {
                if (vehi.Exists())
                {
                    if (vehi.LicensePlate.ToLower() == LicencePlate.ToLower() && !vehi.IsPoliceVehicle)
                    {
                        VehiclesWithLicencePlate.Add(vehi);
                        
                    }
                }
            }
            if (VehiclesWithLicencePlate.Count > 0)
            {
                return VerifyVehicleRecordsExist((from x in VehiclesWithLicencePlate orderby x.DistanceTo(Game.LocalPlayer.Character.Position) select x).FirstOrDefault());
            }
            else
            {
                return false;
            }
        }

        internal static bool VerifyVehicleRecordsExist(Vehicle vehi)
        {
            if (vehi.Exists())
            {
                foreach (VehicleRecords recs in AllVehicleRecords.ToArray())
                {
                    if (recs.veh == vehi)
                    {
                        return true;
                    }
                }

                new VehicleRecords(vehi);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static VehicleRecords GetVehicleRecords(string plate)
        {
            if (VerifyVehicleRecordsExist(plate))
            {
                foreach (VehicleRecords recs in AllVehicleRecords.ToArray())
                {
                    if (recs.LicencePlate.ToLower() == plate.ToLower())
                    {
                        return recs;
                    }
                }
            }
            return null; //should not happen
        }

        internal static VehicleRecords GetVehicleRecords(Vehicle veh)
        {
            if (VerifyVehicleRecordsExist(veh))
            {
                foreach (VehicleRecords recs in AllVehicleRecords.ToArray())
                {
                    if (recs.veh == veh)
                    {
                        return recs;
                    }
                }
            }
           
            return null; //should not happen
        }

        internal static void RunPlateCheckOnPlate(string plate)
        {
            GameFiber.Wait(2500);
            if (VerifyVehicleRecordsExist(plate))
            {
                
                GetVehicleRecords(plate).RunPlateCheck();
            }
        }


        public Vehicle veh
        {
            get;
        }

        
        /// <summary>
        /// Automatically gets/sets insurance status using Traffic Policer.
        /// </summary>
        public bool Insured
        {
            get
            {
                if (veh.Exists())
                {
                    return Traffic_Policer.API.Functions.IsVehicleInsured(veh);
                }
                else
                {
                    return true;
                }
            }
            set 
            {
                if (veh.Exists())
                {
                    Traffic_Policer.API.Functions.SetVehicleInsuranceStatus(veh, value);
                    RegisteredOwner.SetIsInsuredToDriveVehicle(this, value);
                }
            }
        }



        public bool HasMOT;
        public bool IsTaxed;
        public bool HasSORN;
        
        public BritishPersona RegisteredOwner;

        private string _CarColour = "weirdly-coloured";
        public string CarColour
        {
            get
            {
                try
                {
                    if (veh.Exists())
                    {
                        _CarColour = veh.GetColors().PrimaryColorName + "-coloured";
                        return veh.GetColors().PrimaryColorName + "-coloured";
                    }
                    else
                    {
                        return _CarColour;
                    }
                }
                catch (Exception e)
                {
                    return "weirdly-coloured";
                }
            }
        }



        private string _LicencePlate = "-";
        public string LicencePlate
        {
            get
            {
                if (veh.Exists())
                {
                    _LicencePlate = veh.LicensePlate;
                    return veh.LicensePlate;
                }
                else
                {
                    return _LicencePlate;
                }
            }
            
        }

        /// <summary>
        /// Uses the Rage.Vehicle.IsStolen property. If this is changed, the registered owner is altered appropriately.
        /// </summary>
        public bool Stolen
        {
            get
            {
                if (veh.Exists())
                {
                    return veh.IsStolen;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (veh.Exists())
                {
                    veh.IsStolen = value;
                    if (veh.HasDriver && value)
                    {
                        RegisteredOwner = BritishPersona.GetBritishPersona(veh.Driver);
                    }
                    else
                    {
                        RegisteredOwner = BritishPersona.GetRandomBritishPersona();
                    }
                }
            }
        }

        public string ModelName
        {
            get
            {
                return char.ToUpper(veh.Model.Name[0]) + veh.Model.Name.ToLower().Substring(1);
            }
           
        }



        private VehicleRecords()
        {
            AllVehicleRecords.Add(this);
        }

        public VehicleRecords(Vehicle _veh, bool _HasMOT, bool _IsTaxed, BritishPersona _RegisteredOwner) : this()
        {
            if (!_veh.Exists()) { AllVehicleRecords.Remove(this); return; }
            foreach (VehicleRecords recs in AllVehicleRecords.ToArray())
            {
                if (recs.veh == _veh && recs != this)
                {
                    AllVehicleRecords.Remove(recs);
                }
            }
            veh = _veh;
            foreach (VehicleRecords vehrecs in AllVehicleRecords.ToArray())
            {
                if (vehrecs.LicencePlate == LicencePlate && vehrecs != this)
                {
                    AllVehicleRecords.Remove(vehrecs);
                }
            }
            HasMOT = _HasMOT;
            IsTaxed = _IsTaxed;
            HasSORN = (!IsTaxed && EntryPoint.rnd.Next(6) < 2);
            
            RegisteredOwner = _RegisteredOwner;
            if (Insured)
            {
                RegisteredOwner.SetIsInsuredToDriveVehicle(this, true);
            }
        }

        /// <summary>
        /// Constructor that set values based off the LSPDFR API.
        /// </summary>
        /// <param name="_veh"></param>
        public VehicleRecords(Vehicle _veh) : this()
        {
            if (!_veh.Exists()) { AllVehicleRecords.Remove(this); return; }
            foreach (VehicleRecords recs in AllVehicleRecords.ToArray())
            {
                if (recs.veh == _veh && recs != this)
                {
                    AllVehicleRecords.Remove(recs);
                }
            }
            veh = _veh;
            foreach (VehicleRecords vehrecs in AllVehicleRecords.ToArray())
            {
                if (vehrecs.LicencePlate == LicencePlate && vehrecs != this)
                {
                    AllVehicleRecords.Remove(vehrecs);
                }
            }
            HasMOT = EntryPoint.rnd.Next(8) != 0;
            IsTaxed = EntryPoint.rnd.Next(8) != 0;
            HasSORN = (!IsTaxed && EntryPoint.rnd.Next(6) < 2);
            if (veh.HasDriver && !Stolen)
            {
                RegisteredOwner = BritishPersona.GetBritishPersona(veh.Driver);
            }
            else
            {
                RegisteredOwner = BritishPersona.GetRandomBritishPersona();
            }

            if (Insured)
            {
                RegisteredOwner.SetIsInsuredToDriveVehicle(this, true);
            }
            
        }
        
        

        private string CustomFlags="";
        public void AddCustomFlag(string Flag)
        {
            CustomFlags += Flag;
        }
        public void ResetCustomFlags()
        {
            CustomFlags = "";
        }
        public string DetermineFlags()
        {
            string Flags = "";
            if (RegisteredOwner.Wanted)
            {
                if (RegisteredOwner.WantedForDrugs)
                {
                    Flags += "~r~DRUGS MARKER ";
                }
                else if (RegisteredOwner.WantedForGuns)
                {
                    Flags += "~r~GUN MARKER ";
                }
                else
                {
                    Flags += "~r~REG. OWNER WANTED ";
                }
            }
            if (!Insured)
            {
                Flags += "~r~NO INSURANCE ";
            }
            if (Stolen)
            {
                Flags += "~r~STOLEN ";
            }

            if (HasSORN)
            {
                Flags += "~r~SORN ACTIVE ";
            }

            if (RegisteredOwner.LicenceStatus == BritishPersona.LicenceStatuses.Disqualified)
            {
                Flags += "~r~REG. OWNER DISQUALIFIED ";
            }
            else if (RegisteredOwner.LicenceStatus == BritishPersona.LicenceStatuses.Revoked)
            {
                Flags += "~r~REG. OWNER'S LICENCE REVOKED ";
            }
           
            
            Flags += CustomFlags;
            if (Flags == "") { Flags += "~g~NO TRACE"; }
            return Flags;
        }



        public void RunPlateCheck()
        {
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~DVLA Records", "Vehicle Check: ~b~" + LicencePlate, "-~b~" + ModelName + "~n~~s~-Reg. Owner: ~b~" + RegisteredOwner.FullName + 
                "~n~~s~-MOT: " + (HasMOT ? "~g~Valid" : "~r~Invalid") + "~n~~s~-" + (IsTaxed ? "~g~Taxed" : "~r~Untaxed"));
            Game.DisplayNotification("~b~VEHICLE-PNC: ~s~" + DetermineFlags());
        }


    }
}
