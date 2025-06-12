namespace Incident_Analyzer_Bff.Models
{
    public class DetailsModel
    {
        public string CID { get; set; }
        public List<string> AfftectedServivces{ get; set; }

        public string ErrorInService { get; set; }
        public string ErrorMessage { get; set; }
        public string RootCause { get; set; }
        public string Solution { get; set; }
        public PersonDetails ContactDetails{ get; set; }

    }
}
