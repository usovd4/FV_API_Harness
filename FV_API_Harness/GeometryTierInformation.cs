using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace FV_API_Harness
{
    [Table("dbo.STG_GetGeometryTier")]
    public class GeometryTierInformation
    {
        [Column("Id")]
        public int ID { get; set; }
        [Column("ParentId")]
        public int? ParentID { get; set; }
        [Column("Description")]
        public string Description { get; set; }
        [Column("SortOrder")]
        public int? SortOrder { get; set; }
        [Column("HasChildren")]
        public bool? HasChildren { get; set; }
        //public bool Calibrated { get; set; }
        //public int GeometryTypeID { get; set; }
        //public string GeometryType { get; set; }
        //public object GeometryClassificationID { get; set; }
        //public object GeometryClassification { get; set; }
        [Column("Status")]
        public object Status { get; set; }
        //public object StatusColour { get; set; }
        //public object GeometryImageID { get; set; }
        //public object CalibratedGeometryImageID { get; set; }
        [Column("Active")]
        public bool? Active { get; set; }
        [Column("Deleted")]
        public bool? Deleted { get; set; }
        [Column("ProjectID")]
        public int? ProjectID { get; set; }
    }
}
