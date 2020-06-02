using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace FV_API_Harness
{
    [Table("dbo.STG_GetProjectListUpdated")]
    public class ProjectFormsListUpdatedInformation
    {
        public ProjectFormsListUpdatedInformation()
        {
            this.UploadedDate = DateTime.Now;
        }

        [Column("FormTemplateID")]
        public int? FormTemplateID { get; set; }
        [Column("FormID")]
        public string FormID { get; set; }
        [Column("FormAnswerID")]
        public string FormAnswerID { get; set; }
        [Column("FormAnswerLinkID")]
        public string FormAnswerLinkID { get; set; }
        [Column("QuestionType")]
        public string QuestionType { get; set; }
        [Column("DataType")]
        public string DataType { get; set; }
        [Column("Question")]
        public string Question { get; set; }
        [Column("Answer")]
        public string Answer { get; set; }
        [Column("Alias")]
        public string Alias { get; set; }
        [Column("SortOrder")]
        public int? SortOrder { get; set; }
        [Column("Answeredby")]
        public string AnsweredBy { get; set; }
        [Column("AnsweredDateTime")]
        public string AnsweredDateTime { get; set; }
        [Column("isTableGroup")]
        public bool? IsTableGroup { get; set; }
        [Column("Deleted")]
        public bool? Deleted { get; set; }
        [Column("HasActions")]
        public bool? HasActions { get; set; }
        [Column("HasImages")]
        public bool? HasImages { get; set; }
        [Column("HasComments")]
        public bool? HasComments { get; set; }
        [Column("HasDocuments")]
        public bool? HasDocuments { get; set; }
        //public DateTime AnswerLastModifiedOnServerDateTime { get; set; }
        [Column("UploadedDate")]
        public DateTime UploadedDate { get; set; }
    }
}
