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

namespace British_Policing_Script
{
    public class BritishPersona : Persona
    {
        public enum LicenceTypes { Full, Provisional, None}
        public enum LicenceStatuses { Valid, Expired, Revoked, Disqualified, None}


        internal static string[] WantedReasons = new string[] {"DRUNK AND DISORDERLY", "CRIMINAL DAMAGE", "COMMON ASSAULT", "ACTUAL BODILY HARM", "BURGLARY", "BREACH OF BAIL CONDITIONS", "DOMESTIC ABUSE", "HARASSMENT", "SHOPLIFTING",
        "GOING EQUIPPED TO STEAL", "HANDLING STOLEN GOODS", "INTERFERING WITH A MOTOR VEHICLE", "ARSON", "ESCAPE FROM LAWFUL CUSTODY", "FEAR AND PROVOCATION OF VIOLENCE", "GRIEVOUS BODILY HARM", "MAKING OFF WITHOUT PAYMENT",
        "TRESPASSING", "FRAUD", "THEFT FROM A MOTOR VEHICLE", "AGGRAVATED VEHICLE TAKING", "TAKING A MOTOR VEHICLE WITHOUT THE OWNER'S CONSENT"};

        internal static string[] DrugsReasons = new string[] {"POSSESSION OF CLASS A DRUGS", "POSSESSION OF CLASS B DRUGS", "POSSESSION OF CLASS C DRUGS", "POSSESSION OF PSYCHOACTIVE SUBSTANCES WITH INTENT TO SUPPLY",
        "POSSESSION OF CLASS A DRUGS WITH INTENT TO SUPPLY", "POSSESSION OF CLASS B DRUGS WITH INTENT TO SUPPLY", "POSSESSION OF CLASS C DRUGS WITH INTENT TO SUPPLY" };

        internal static string[] GunReasons = new string[] {"POSSESSION OF A FIREARM", "ARMED ROBBERY", "POSSESSION OF AN IMITATION FIREARM", "USE OF FIREARM TO RESIST ARREST", "CARRYING A FIREARM WITH CRIMINAL INTENT",
        "CARRYING A FIREARM IN A PUBLIC PLACE"};

        internal static List<BritishPersona> AllBritishPersona = new List<BritishPersona>();
        internal static BritishPersona GetBritishPersona(string name)
        {
            if (VerifyBritishPersonaExist(name))
            {
                foreach (BritishPersona pers in AllBritishPersona.ToArray())
                {
                    if (pers.FullName.ToLower() == name.ToLower())
                    {
                        return pers;
                    }
                }
            }
            return null; //should neva happen
        }
        
        internal static BritishPersona GetBritishPersona(Ped ped)
        {
            if (VerifyBritishPersonaExist(ped))
            {
                foreach (BritishPersona pers in AllBritishPersona.ToArray())
                {
                    if (pers.ped == ped)
                    {
                        return pers;
                    }
                }
            }
            return null; //should neva happen
        }
        internal static bool VerifyBritishPersonaExist(string name)
        {
            
            foreach (Ped ped in World.GetAllPeds())
            {
                if (ped.Exists())
                {
                    if (Functions.GetPersonaForPed(ped).FullName.ToLower() == name.ToLower())
                    {
                        
                        return VerifyBritishPersonaExist(ped);
                        
                    }
                }
            }
            return false;
        }
        internal static bool VerifyBritishPersonaExist(Ped ped)
        {
            if (ped.Exists())
            {
                
                foreach (BritishPersona pers in AllBritishPersona.ToArray())
                {
                    if (pers.ped == ped)
                    {
                        
                        return true;
                    }
                }
                
                new BritishPersona(ped);
                
                return true;
            }
            else
            {
                return false;
            }
        }
        internal static void RunLicenceCheckOnName(string name)
        {
            GameFiber.Wait(2500);
            if (VerifyBritishPersonaExist(name))
            {
                Game.LogTrivial("DEBUG 1 BPS");
                GetBritishPersona(name).RunLicenceCheck();

            }
            
        }
        internal static void RunInsuranceCheckOnName(string name)
        {
            GameFiber.Wait(2500);
            if (VerifyBritishPersonaExist(name))
            {

                GetBritishPersona(name).CheckForCarInsurancePolicies();

            }
        }
        internal static BritishPersona GetRandomBritishPersona()
        {
            Ped randped = World.GetAllPeds()[0];
            if (randped.Exists())
            {
                return GetBritishPersona(randped);
            }
            return null; //should never happen
        }


        public LicenceTypes LicenceType;
        public LicenceStatuses LicenceStatus;
        public bool CarryingLicence;
        Persona _LSPDFRPersona;
        public Persona LSPDFRPersona
        {
            get
            {
                if (ped.Exists())
                {
                    _LSPDFRPersona = Functions.GetPersonaForPed(ped);
                    return _LSPDFRPersona;
                }
                else
                {
                    return _LSPDFRPersona;
                }
            }
            
        }

        public new string Forename
        {
            get
            {
                return LSPDFRPersona.Forename;
            }
        }

        public new string Surname
        {
            get
            {
                return LSPDFRPersona.Surname;
            }
        }

        public new string FullName
        {
            get
            {
                return LSPDFRPersona.FullName;
            }
        }

        public new DateTime BirthDay
        {
            get
            {
                return LSPDFRPersona.BirthDay;
            }
        }

        public new int Citations
        {
            get
            {
                return LSPDFRPersona.Citations;
            }
        }

        public new LSPD_First_Response.Gender Gender
        {
            get
            {
                return LSPDFRPersona.Gender;
            }
        }

        public new bool IsAgent
        {
            get
            {
                return LSPDFRPersona.IsAgent;
            }
        }

        public new bool IsCop
        {
            get
            {
                return LSPDFRPersona.IsCop;
            }
        }

        public new ELicenseState LicenseState
        {
            get
            {
                return LSPDFRPersona.LicenseState;
            }
        }

        public new EPedAge ModelAge
        {
            get
            {
                return LSPDFRPersona.ModelAge;
            }
        }

        public new int TimesStopped
        {
            get
            {
                return LSPDFRPersona.TimesStopped;
            }
        }

        public new bool Wanted
        {
            get
            {
                return LSPDFRPersona.Wanted;
            }
        }



        int _PenaltyPoints;        
        public int PenaltyPoints
        {
            get { return this._PenaltyPoints; }
            set
            {
                if (value >= 0)
                {
                    this._PenaltyPoints = value;
                }
            }   
        }

        private string _WantedReason;
        public string WantedReason
        {
            get
            {
                if (_WantedReason == null)
                {
                    if (Wanted)
                    {
                        int roll = EntryPoint.rnd.Next(9);
                        if (roll < 5)
                        {
                           
                            WantedForDrugs = false;
                            WantedForGuns = false;
                        }
                        else if (roll < 7)
                        {
                            
                            
                            WantedForGuns = false;
                            WantedForDrugs = true;
                        }
                        else
                        {
                            
                            
                            WantedForDrugs = false;
                            WantedForGuns = true;
                        }
                    }
                    else
                    {
                        _WantedReason = "";
                    }
                    
                }
                return _WantedReason;
            }
        }

        private bool _WantedForDrugs = false;
        public bool WantedForDrugs
        {
            get { return _WantedForDrugs; }
            set
            {
                if (value)
                {
                    _WantedReason = DrugsReasons[EntryPoint.rnd.Next(DrugsReasons.Length)];
                }
                else if (Wanted)
                {
                    _WantedReason = WantedReasons[EntryPoint.rnd.Next(WantedReasons.Length)];
                }
                else
                {
                    _WantedReason = "";
                }
                _WantedForDrugs = value;
            }
        }

        private bool _WantedForGuns = false;
        public bool WantedForGuns
        {
            get { return _WantedForGuns; }
            set
            {
                if (value)
                {
                    _WantedReason = GunReasons[EntryPoint.rnd.Next(GunReasons.Length)];
                }
                
                else if (Wanted)
                {
                    _WantedReason = WantedReasons[EntryPoint.rnd.Next(WantedReasons.Length)];
                }
                else
                {
                    _WantedReason = "";
                }
                _WantedForGuns = value;
            }
        }




        private List<VehicleRecords> VehiclesInsuredToDrive = new List<VehicleRecords>();
        public bool FullyCompInsured;
        public bool IsInsuredToDriveVehicle(VehicleRecords vehrecs)
        {
            if (FullyCompInsured || VehiclesInsuredToDrive.Contains(vehrecs))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool HasSpecificInsuranceForVehicle(VehicleRecords vehrecs)
        {
            return VehiclesInsuredToDrive.Contains(vehrecs);
        }
        public void SetIsInsuredToDriveVehicle(VehicleRecords vehrecs, bool Insured)
        {
            if (Insured)
            {
                if (!VehiclesInsuredToDrive.Contains(vehrecs))
                {
                    VehiclesInsuredToDrive.Add(vehrecs);
                }
            }
            else
            {
                if (VehiclesInsuredToDrive.Contains(vehrecs))
                {
                    VehiclesInsuredToDrive.Remove(vehrecs);
                }
            }
        }



        public Ped ped
        {
            get; 
        }
        private BritishPersona() : base (World.EnumeratePeds().FirstOrDefault(), LSPD_First_Response.Gender.Random, new DateTime(), 0,  "", "", ELicenseState.None, 0, false, false, false)
        {         
            AllBritishPersona.Add(this);
        }



        public BritishPersona(Ped _ped, LicenceTypes _LicenceType, LicenceStatuses _LicenceStatus, int _PenaltyPoints, bool _CarryingLicence) : this(_ped)
        {
            if (!_ped.Exists()) { AllBritishPersona.Remove(this); return; }
            foreach (BritishPersona pers in AllBritishPersona.ToArray())
            {
                if (pers.ped == _ped && pers != this)
                {
                    AllBritishPersona.Remove(pers);
                }
            }
            ped = _ped;
            if (ped.Exists())
            {
                
                _LSPDFRPersona = Functions.GetPersonaForPed(ped);
                if (ped.IsInAnyVehicle(false))
                {
                    VehicleRecords.VerifyVehicleRecordsExist(ped.CurrentVehicle);
                }
                foreach (BritishPersona pers in AllBritishPersona.ToArray())
                {
                    if (pers.ped == ped && pers != this)
                    {
                        AllBritishPersona.Remove(pers);
                    }
                }
            }
            LicenceType = _LicenceType;
            LicenceStatus = _LicenceStatus;
            PenaltyPoints = _PenaltyPoints;
            CarryingLicence = _CarryingLicence;
            
        }
        /// <summary>
        /// Constructor that sets values based off the LSPDFR API
        /// </summary>
        /// <param name="_ped"></param>
        public BritishPersona (Ped _ped) : this() 
        {
            if (!_ped.Exists()) { Game.LogTrivial("PED DOESNT EXIST"); AllBritishPersona.Remove(this); return; }

            foreach (BritishPersona pers in AllBritishPersona.ToArray())
            {
                if (pers.ped == _ped && pers != this)
                {
                    AllBritishPersona.Remove(pers);
                }
            }
            ped = _ped;
            if (ped.Exists())
            {
                _LSPDFRPersona = Functions.GetPersonaForPed(ped);
                if (ped.IsInAnyVehicle(false))
                {
                    VehicleRecords.VerifyVehicleRecordsExist(ped.CurrentVehicle);
                }
                foreach (BritishPersona pers in AllBritishPersona.ToArray())
                {
                    if (pers.ped == ped && pers != this)
                    {
                        AllBritishPersona.Remove(pers);
                    }
                }
                
            }
            int roll = EntryPoint.rnd.Next(10);
            if (roll < 6)
            {
                LicenceType = LicenceTypes.Full;
            }
            else
            {
                //If ped is driving a vehicle with a passenger
                if (_ped.IsInAnyVehicle(false))
                {
                    if (_ped.CurrentVehicle.Driver == _ped)
                    {
                        if (!_ped.CurrentVehicle.IsSeatFree(0))
                        {
                            if (EntryPoint.rnd.Next(4) < 3)
                            {


                                LicenceType = LicenceTypes.Provisional;
                            }
                            else
                            {
                                LicenceType = LicenceTypes.None;
                            }
                                  
                        }
                    }
                }
                else
                {
                    roll = EntryPoint.rnd.Next(4);
                    if (roll < 2)
                    {
                        LicenceType = LicenceTypes.Provisional;
                    }
                    else
                    {
                        LicenceType = LicenceTypes.None;
                    }
                }

            }
            FullyCompInsured = EntryPoint.rnd.Next(8) == 0;
            int Citations = LSPDFRPersona.Citations;
            PenaltyPoints = Citations * 3;
            
            if (LSPDFRPersona.LicenseState == ELicenseState.Valid)
            {
                LicenceStatus = LicenceStatuses.Valid;
            }
            else if (LSPDFRPersona.LicenseState == ELicenseState.Suspended)
            {
                if (EntryPoint.rnd.Next(6) < 4)
                {
                    LicenceStatus = LicenceStatuses.Disqualified;
                }
                else
                {
                    LicenceStatus = LicenceStatuses.Revoked;
                }
            }
            else if (LSPDFRPersona.LicenseState == ELicenseState.Expired)
            {
                LicenceStatus = LicenceStatuses.Expired;
            }
            else
            {
                LicenceStatus = LicenceStatuses.Valid;
            }
            
            if (LicenceStatus == LicenceStatuses.Disqualified || LicenceStatus == LicenceStatuses.Revoked || LicenceType == LicenceTypes.None)
            {
                CarryingLicence = false;
            }
            else
            {

                CarryingLicence = EntryPoint.rnd.Next(6) < 4;
                
            }
            
            

        }

        public void RequestDetails()
        {
            if (CarryingLicence)
            {
                DisplayLicence();
            }
            else
            {
                GiveDetails();
            }
        }
        private void DisplayLicence()
        {
            Game.DisplayNotification("3dtextures", "mp_generic_avatar", "~b~Driving Licence", FullName, "~b~" + LSPDFRPersona.Gender + ", ~s~DOB: ~b~" + LSPDFRPersona.BirthDay.ToShortDateString() 
                + "~n~~s~Licence Type: ~b~" + LicenceType.ToString()); 
        }
        private void GiveDetails()
        {
            Game.DisplayNotification("~b~Suspect: ~s~My name is ~b~" + FullName + "~s~. I was born on ~b~" + LSPDFRPersona.BirthDay.ToShortDateString() + "~s~.");
        }

        private string CustomFlags = "";
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
            if (Wanted)
            {
                Flags += "~r~WANTED - " + WantedReason;
            }
            
            Flags += CustomFlags;
            if (Flags == "") { Flags += "~g~NO TRACE"; }
            return Flags;
        }
        public void RunLicenceCheck()
        {
            
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~DVLA Records", FullName, "~y~" + LSPDFRPersona.Gender + ", ~s~Born ~y~" + LSPDFRPersona.BirthDay.ToShortDateString()
                + (LicenceType == LicenceTypes.None ? "~r~ No licence records." : "~n~~b~" + LicenceType.ToString() + " ~s~licence: ~b~" + LicenceStatus.ToString() + "~n~~y~" + PenaltyPoints.ToString() + " ~s~points." ));
            Game.DisplayNotification("~b~PERSON-PNC: ~s~" + DetermineFlags());
        }

        internal void CheckForCarInsurancePolicies()
        {
            string licenceplates = "";
            foreach (VehicleRecords vehrecs in VehiclesInsuredToDrive)
            {
                licenceplates += vehrecs.LicencePlate.ToUpper() + " ";
            }
            if (licenceplates == "") { licenceplates = "~r~None"; }
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "~b~VID Records", FullName, (FullyCompInsured ? "~g~Fully comp. insurance" : "") + "~b~Insured to drive: ~s~" + licenceplates);
        }
    } 
}
