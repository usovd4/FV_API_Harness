using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FV_API_Harness
{
    public class Response
    {
        public Response()
        {
            ProjectInformation = new List<ProjectInformation>();
            GeometryTierInformation = new List<GeometryTierInformation>();
            ProjectFormsListInformation = new List<ProjectFormsListInformation>();
            ProjectFormsListUpdatedInformation = new List<ProjectFormsListUpdatedInformation>();
            Status = new Status();
        }
        public List<ProjectFormsListUpdatedInformation> ProjectFormsListUpdatedInformation { get; set; }
        public List<ProjectInformation> ProjectInformation { get; set; }
        public List<ProjectFormsListInformation> ProjectFormsListInformation { get; set; }
        public List<GeometryTierInformation> GeometryTierInformation { get; set; }
        public Status Status { get; set; }
    }
}
