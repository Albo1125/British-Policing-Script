using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace British_Policing_Script.API
{
    public static class Functions
    {
        /// <summary>
        /// Returns the BritishPersona for the ped.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        public static BritishPersona GetBritishPersona(Ped ped)
        {
            return BritishPersona.GetBritishPersona(ped);
        }
        /// <summary>
        /// Returns the VehicleRecords for the vehicle.
        /// </summary>
        /// <param name="veh"></param>
        /// <returns></returns>
        public static VehicleRecords GetVehicleRecords(Vehicle veh)
        {
            return VehicleRecords.GetVehicleRecords(veh);
        }

        #region courtsystem
        

        /// <summary>
        /// Adds a new court case to the court system.
        /// </summary>
        /// <param name="DefendantPersona">British Persona of the defendant.</param>
        /// <param name="Crime">String describing the crime committed, e.g. 'stealing a police vehicle'.</param>
        /// <param name="GuiltyChance">100 = always guilty, 0 = never guilty.</param>
        /// <param name="CourtVerdict">The decision the court will come to, e.g. 'Sentenced to 5 months in prison'</param>
        public static void CreateNewCourtCase(BritishPersona DefendantPersona, string Crime, int GuiltyChance, string CourtVerdict)
        {
             CourtSystem.CreateNewCourtCase(DefendantPersona.FullName, DefendantPersona.LSPDFRPersona.Birthday, Crime, DateTime.Now, GuiltyChance, CourtVerdict, CourtSystem.DetermineCourtHearingDate(), false);
        }

        /// <summary>
        /// Adds a new court case to the court system (not recommended in release builds. Use this overload only to set instant publish time for testing).
        /// </summary>
        /// <param name="Defendant"></param>
        /// <param name="Crime">String describing the crime committed, e.g. 'stealing a police vehicle'.</param>
        /// <param name="GuiltyChance">100 = always guilty, 0 = never guilty.</param>
        /// <param name="CourtVerdict">The decision the court will come to, e.g. 'Sentenced to 5 months in prison'</param>
        /// <param name="ResultsPublishTime">The DateTime when the results will become available to the player.</param>
        public static void CreateNewCourtCase(BritishPersona Defendant, string Crime, int GuiltyChance, string CourtVerdict, DateTime ResultsPublishTime)
        {
            CourtSystem.CreateNewCourtCase(Defendant.FullName, Defendant.LSPDFRPersona.Birthday, Crime, DateTime.Now, GuiltyChance, CourtVerdict, ResultsPublishTime, false);
        }

        /// <summary>
        /// Returns a court verdict for a prison sentence depending on the parameters.
        /// </summary>
        /// <param name="MinMonths"></param>
        /// <param name="MaxMonths"></param>
        /// <param name="SuspendedChance">Percentage based chance of the sentence being suspended. 100 = always suspended, 0 = never suspended.</param>
        /// <returns></returns>
        public static string DeterminePrisonSentence(int MinMonths, int MaxMonths, int SuspendedChance)
        {
            return CourtSystem.DeterminePrisonSentence(MinMonths, MaxMonths, SuspendedChance);
        }

        /// <summary>
        /// Returns a court verdict for a fine depending on the parameters.
        /// </summary>
        /// <param name="MinFine"></param>
        /// <param name="MaxFine"></param>
        /// <returns></returns>
        public static string DetermineFineSentence(int MinFine, int MaxFine)
        {
            return CourtSystem.DetermineFineSentence(MinFine, MaxFine);
        }
        #endregion

        /// <summary>
        /// Raised whenever the player orders a ped out of a vehicle on a traffic stop (also raised by LSPDFR+, rather use LSPDFR+'s event).
        /// </summary>
        [Obsolete("Use LSPDFR+'s event.")]
        public static event LSPDFR_.API.PedEvent PedOrderedOutOfVehicle;
        internal static void OnPedOrderedOutOfVehicle(Ped ped)
        {

            if (PedOrderedOutOfVehicle != null)
            {
                PedOrderedOutOfVehicle(ped);
            }
        }

    }
}
