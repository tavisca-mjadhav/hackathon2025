namespace Incident_Analyzer_Bff.Models
{
    public class DetailsModel
    {
        public string CID { get; set; }
        public string AffectedServices { get; set; }
        public string ErrorSummary { get; set; }
        public string ErrorInService { get; set; }
        public string ErrorMessage { get; set; }
        public string RootCause { get; set; }
        public string Solution { get; set; }
        public string Impact{ get; set; }

        public string ContactDetails { get; set; }
        //  public PersonDetails ContactDetails{ get; set; }

    }
}
