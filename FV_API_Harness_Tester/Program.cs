using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FV_API_Harness;
namespace FV_API_Harness_Tester
{
    class Program
    {
        static void Main(string[] args)
        {


            List<FV_Call_Param> BU_Call_Params = new List<FV_Call_Param>();
            BU_Call_Params.Add(new FV_Call_Param("apiToken", "14D69DD24BA51C4A5B51EF1AF334D44C832DCDF"));
            List<string> AfterCare_BUs = new List<string>();
            AfterCare_BUs.Add("25781");
            AfterCare_BUs.Add("25782");
            BU_Call_Params.Add(new FV_Call_Param("businessUnitIDs", FV_Param_Type.@int, AfterCare_BUs));
            BU_Call_Params.Add(new FV_Call_Param("activeOnly", "true"));
            BU_Call_Params.Add(new FV_Call_Param("projectName", ""));
            BU_Call_Params.Add(new FV_Call_Param("startRow", "0"));
            BU_Call_Params.Add(new FV_Call_Param("pageSize", "1000"));

            FV_API_Call AfterCareProjects = new FV_API_Call(FvReturnType.XML, FVWebService.CONFIGURATION, "GetProjects", BU_Call_Params);
            string mystring = AfterCareProjects.GetFVReponse();

            



            List<FV_Call_Param> CallParams = new List<FV_Call_Param>();
            CallParams.Add(new FV_Call_Param("apiToken", "14D69DD24BA51C4A5B51EF1AF334D44C832DCDF"));
            CallParams.Add(new FV_Call_Param("projectId", "7829"));
            List<string> formTemplateLinkIds = new List<string>();
            formTemplateLinkIds.Add("3878666");
            CallParams.Add(new FV_Call_Param("formTemplateLinkIds", FV_Param_Type.@int, formTemplateLinkIds));

            CallParams.Add(new FV_Call_Param("lastmodifiedDateFrom", "2019-07-07T23:59:59"));
            CallParams.Add(new FV_Call_Param("lastmodifiedDateTo", "2019-07-18T23:59:59"));

            FV_API_Call fV_API_Call = new FV_API_Call(FvReturnType.XML, FVWebService.FORM, "GetProjectFormsListUpdated", CallParams);
            //string mystring = fV_API_Call.GetFVReponse();

        }




    }
}
