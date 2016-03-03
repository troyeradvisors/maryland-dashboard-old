namespace Dashboard.Web
{
    using System.Runtime.Serialization;
    using System.ServiceModel.DomainServices.Server.ApplicationServices;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Class containing information about the authenticated user.
    /// </summary>
    public partial class HomeSummary
    {
        [Key]
        [Editable(false)]
        public int PIN { get; set; }
        [Key]
        [Editable(false)]
        public int FiscalYear { get; set; }
        public string Name {get;set;}
		public int CountyCode { get; set; }
    }

}
