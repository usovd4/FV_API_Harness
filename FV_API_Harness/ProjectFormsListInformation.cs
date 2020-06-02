using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace FV_API_Harness
{
    [Table("dbo.STG_GetProjectFormsList")]
    public class ProjectFormsListInformation
    {
        public ProjectFormsListInformation()
        {
            this.UploadDate = DateTime.Now;
        }

        [Column("ProjectID")]
        public int ProjectID { get; set; }
        [Column("FormID")]
        public string FormID { get; set; }
        [Column("FormTemplateLinkID")]
        public int? FormTemplateLinkID { get; set; }
        [Column("Deleted")]
        public bool? Deleted { get; set; }
        [Column("FormType")]
        public string FormType { get; set; }
        [Column("FormName")]
        public string FormName { get; set; }
        [Column("FormTitle")]
        public string FormTitle { get; set; }
        [Column("CreatedDate")]
        public string CreatedDate { get; set; }
        [Column("OwnedBy")]
        public string OwnedBy { get; set; }
        [Column("OwnedByOrganisation")]
        public string OwnedByOrganisation { get; set; }
        [Column("IssuedToOrganisation")]
        public string IssuedToOrganisation { get; set; }
        [Column("Status")]
        public string Status { get; set; }
        [Column("StatusColour")]
        public string StatusColour { get; set; }
        [Column("StatusDate")]
        public string StatusDate { get; set; }
        [Column("Location")]
        public string Location { get; set; }
        [Column("OpenTasks")]
        public int? OpenTasks { get; set; }
        [Column("ClosedTasks")]
        public int? ClosedTasks { get; set; }
        [Column("FormExpiryDate")]
        public string FormExpiryDate { get; set; }
        [Column("Overdue")]
        public bool? OverDue { get; set; }
        [Column("Complete")]
        public bool? Complete { get; set; }
        [Column("Closed")]
        public bool? Closed { get; set; }
        [Column("ParentFormID")]
        public string ParentFormID { get; set; }
        [Column("LastModified")]
        public string LastModified { get; set; }
        //public DateTime LastModifiedOnServer { get; set; }
        [Column("UploadDate")]
        public DateTime UploadDate { get; set; }
    }
}
