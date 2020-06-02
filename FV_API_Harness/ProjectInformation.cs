using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FV_API_Harness
{
    [Table("dbo.STG_GetProjects")]
    public class ProjectInformation
    {
        public ProjectInformation()
        {
            // Unique ID
            this.UploadDate = DateTime.Now;
        }

        [Column("BU_UID")]
        public int? BU_ID { get; set; }
        [Column("ID")]
        public int ID { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Reference")]
        public string Reference { get; set; }
        [Column("ProjectOwnerID")]
        public int? ProjectOwnerID { get; set; }
        [Column("ProjectOwner")]
        public string ProjectOwner { get; set; }
        [Column("BusinessUnitTypeID")]
        public int? BusinessUnitTypeID { get; set; }
        [Column("BusinessUnitType")]
        public string BusinessUnitType { get; set; }
        [Column("ProjectTypeID")]
        public int? ProjectTypeID { get; set; }
        [Column("ProjectType")]
        public string ProjectType { get; set; }
        [Column("StartDate")]
        public string StartDate { get; set; }
        [Column("FinishDate")]
        public object FinishDate { get; set; }
        [Column("TimeZoneOffset")]
        public int? TimeZoneOffset { get; set; }
        [Column("CultureID")]
        public int? CultureID { get; set; }
        [Column("Culture")]
        public string Culture { get; set; }
        [Column("ResolutionDays")]
        public int? ResolutionDays { get; set; }
        [Column("Active")]
        public bool? Active { get; set; }
        [Column("UploadDate")]
        public DateTime? UploadDate { get; set; }
    }
}
