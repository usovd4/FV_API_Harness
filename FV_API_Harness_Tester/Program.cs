using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BulkInserts;
using FV_API_Harness;
using FV_API_Harness.Model;
using Newtonsoft.Json;
using RestSharp;

namespace FV_API_Harness_Tester
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            if(args.Length == 2)
            {
                try
                {
                    bool.TryParse(args[0], out bool priorityOnly);
                    Int32.TryParse(args[1], out int days);
                    string retVal = FV_GetAll(priorityOnly, days);
                    if (retVal != "")
                    {
                        string[] apiReturn = retVal.Split('|');
                        bizEmail.SendEmailTo("FV API failure", apiReturn[0], apiReturn[1], "", null, null, ConfigurationManager.AppSettings["FvApiEmail"]);
                    }
                }
                catch(Exception e)
                {
                    log.Debug(e.Message);
                }
            }
            
        }

        private static string FV_GetAll(bool priorityOnly, int days)
        {
            DateTime procStart = DateTime.Now;
            log.Debug("FV_API Job Started at " + procStart);
            //Setting up variables for the process
            string retVal = "";
            string stpr_name = "";
            bool dataTransferSuccessful = true;
            int p_id = 0;
            string callFunctionName = "";
            Response jsonResponseObject = new Response();
            var client = new RestClient();
            client.Timeout = 600000;

            List<ProjectInformation> PF_PI = new List<ProjectInformation>();
            List<ProjectFormsListUpdatedInformation> PF_LUI = new List<ProjectFormsListUpdatedInformation>();
            List<ProjectFormsListInformation> PF_LI = new List<ProjectFormsListInformation>();
            List<GeometryTierInformation> PF_Geo = new List<GeometryTierInformation>();
            List<ccProject> existingProjects = new List<ccProject>();
            List<FV_API_Token> tokenList = new List<FV_API_Token>();
            List<string> tokenStringDB = new List<string>();
            FV_API_TokenPool tokenPool = new FV_API_TokenPool();
            List<ccBusinessUnit> BUlist = new List<ccBusinessUnit>();

            List<string> failedBuEmails = new List<string>();
            string failedBuEmail;

            //Creating date string for the API call parameters in the format "yyyy-mm-ddT23:59:59"
            DateTime dayString = DateTime.Now;
            string dayStringTo = dayString.Year + "-" + dayString.Month.ToString("00") + "-" + dayString.Day.ToString("00") + "T23:59:59";
            dayString = dayString.AddDays(-days);
            string dayStringFrom = dayString.Year + "-" + dayString.Month.ToString("00") + "-" + dayString.Day.ToString("00") + "T23:59:59";


            using (AppEntities appDB = new AppEntities())
            {

                existingProjects = (from t in appDB.ccProject
                                    select t).ToList();

                //Setting up the token pool
                tokenStringDB = (from t in appDB.APIToken
                                 select t.APIToken1).ToList();
                foreach (string ts in tokenStringDB)
                {
                    FV_API_Token token = new FV_API_Token(ts);
                    tokenList.Add(token);
                }
                tokenPool = new FV_API_TokenPool(tokenList);

                //Clearing the staging tables
                appDB.Database.ExecuteSqlCommand("DELETE from STG_GetProjects");
                appDB.Database.ExecuteSqlCommand("DELETE from STG_GetGeometryTier");
                appDB.Database.ExecuteSqlCommand("DELETE from STG_GetProjectFormsList");
                appDB.Database.ExecuteSqlCommand("DELETE from STG_GetProjectListUpdated");

                //Getting Business Unit IDs from the db
                BUlist = (from t in appDB.ccBusinessUnit
                          where t.buActive == 1
                          select t
                        ).ToList();
            }

            using (var bk = new BulkInsert())
            {
                //In order to populate the BU_ID column in the database we have to execute API call for each Business Unit even though the call is capable of accepting list of BU_IDs
                foreach (ccBusinessUnit bu in BUlist)
                {
                    List<FV_Call_Param> BU_Call_Params = new List<FV_Call_Param>();
                    BU_Call_Params.Add(new FV_Call_Param("apiToken", tokenPool.GetFreshToken().TokenString));
                    List<string> AfterCare_BUs = new List<string>();
                    AfterCare_BUs.Add(bu.buID.ToString());
                    BU_Call_Params.Add(new FV_Call_Param("businessUnitIDs", FV_Param_Type.@int, AfterCare_BUs));
                    BU_Call_Params.Add(new FV_Call_Param("activeOnly", "true"));
                    BU_Call_Params.Add(new FV_Call_Param("projectName", ""));
                    BU_Call_Params.Add(new FV_Call_Param("startRow", "0"));
                    BU_Call_Params.Add(new FV_Call_Param("pageSize", "1000"));

                    callFunctionName = "GetProjects";

                    FV_API_Call AfterCareProjects = new FV_API_Call(FvReturnType.JSON, FVWebService.CONFIGURATION, callFunctionName, BU_Call_Params, tokenPool);
                    try
                    {
                        jsonResponseObject = AfterCareProjects.GetFVReponse(client);
                    }
                    catch (Exception e)
                    {
                        if (!failedBuEmails.Contains(bu.buEmail))
                        {
                            failedBuEmails.Add(bu.buEmail);
                        }
                        retVal = "Business Unit: " + bu.buName + Environment.NewLine + Environment.NewLine + "Error: Failed to retrieve project list for BU " + bu.buName + " with exception " + e + "." + Environment.NewLine + Environment.NewLine + " Process cancelled." + Environment.NewLine + Environment.NewLine + "Application name: Customer Care.";
                        retVal += "|" + String.Join(",", failedBuEmails.ToArray());
                        return retVal;

                    }

                    List<ProjectInformation> projectInfoForms = jsonResponseObject.ProjectInformation;
                    foreach (ProjectInformation pi in projectInfoForms)
                    {
                        pi.BU_ID = bu.buID;
                    }
                    PF_PI.AddRange(projectInfoForms);
                    log.Debug("Recieved data for " + projectInfoForms.Count + " projects from FieldView.");
                }

                //if priorityOnly is true, filter the results to only include the project IDs for the projects currently set to priority in the system
                if (priorityOnly)
                {
                    List<int> priorityPiD = existingProjects.Where(x => x.priority == 1).Select(x => x.projectID).ToList();
                    PF_PI = PF_PI.Where(x => priorityPiD.Contains(x.ID)).ToList();
                }

                foreach (ProjectInformation p in PF_PI)
                {
                    p.UploadDate = existingProjects.Where(x => x.projectID == p.ID).Select(x => x.lastUploaded).FirstOrDefault();
                    dataTransferSuccessful = true;
                    ++p_id;
                    log.Debug("Fetching data for project " + p_id + " out of " + PF_PI.Count + " projects from FieldView.");
                    log.Debug("Parent geometry...");
                    List<FV_Call_Param> GeoP_Call_Params = new List<FV_Call_Param>();
                    GeoP_Call_Params.Add(new FV_Call_Param("apiToken", tokenPool.GetFreshToken().TokenString));
                    GeoP_Call_Params.Add(new FV_Call_Param("parentID", "0"));
                    GeoP_Call_Params.Add(new FV_Call_Param("projectID", p.ID.ToString()));
                    GeoP_Call_Params.Add(new FV_Call_Param("activeOnly", "true"));

                    callFunctionName = "GetGeometryTier";
                    List<GeometryTierInformation> geo_forms = new List<GeometryTierInformation>();

                    FV_API_Call GeoP_CallParams = new FV_API_Call(FvReturnType.JSON, FVWebService.CONFIGURATION, callFunctionName, GeoP_Call_Params, tokenPool);

                    try
                    {
                        jsonResponseObject = GeoP_CallParams.GetFVReponse(client);
                        geo_forms = jsonResponseObject.GeometryTierInformation;
                    }
                    catch (Exception e)
                    {
                        failedBuEmail = BUlist.Where(x => x.buID == p.BU_ID).Select(x => x.buEmail).First();
                        if (!failedBuEmails.Contains(failedBuEmail))
                        {
                            failedBuEmails.Add(failedBuEmail);
                        }
                        dataTransferSuccessful = false;
                        log.Debug("Exception during API call GetGeometryTier(Parent): " + e);
                        retVal += "Project: " + p.Name + Environment.NewLine + Environment.NewLine + "Error: Failed to retrieve Parent Geometry Data for Project " + p.Name + ".The exception was: " + e + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;
                    }

                    //If parent geometry returned a result perform the call again to get the child geometry
                    if (geo_forms.Count > 0)
                    {
                        log.Debug("Child geometry...");
                        List<FV_Call_Param> GeoC_Call_Params = new List<FV_Call_Param>();
                        GeoC_Call_Params.Add(new FV_Call_Param("apiToken", tokenPool.GetFreshToken().TokenString));
                        GeoC_Call_Params.Add(new FV_Call_Param("parentID", geo_forms[0].ID.ToString()));
                        GeoC_Call_Params.Add(new FV_Call_Param("projectID", p.ID.ToString()));
                        GeoC_Call_Params.Add(new FV_Call_Param("activeOnly", "true"));

                        callFunctionName = "GetGeometryTier";
                        geo_forms = new List<GeometryTierInformation>();

                        FV_API_Call GeoC_CallParams = new FV_API_Call(FvReturnType.JSON, FVWebService.CONFIGURATION, callFunctionName, GeoC_Call_Params, tokenPool);

                        try
                        {
                            jsonResponseObject = GeoC_CallParams.GetFVReponse(client);
                            geo_forms = jsonResponseObject.GeometryTierInformation;
                            foreach (GeometryTierInformation g in geo_forms)
                            {
                                g.ProjectID = p.ID;
                            }

                            PF_Geo.AddRange(geo_forms);
                            log.Debug("Added " + geo_forms.Count + " to PF_Geo list. Total PF_Geo count is " + PF_Geo.Count);
                            log.Debug("STATUS CODE " + jsonResponseObject.Status.Code + ", THE MESSAGE IS " + jsonResponseObject.Status.Message);
                        }
                        catch (Exception e)
                        {
                            failedBuEmail = BUlist.Where(x => x.buID == p.BU_ID).Select(x => x.buEmail).First();
                            if (!failedBuEmails.Contains(failedBuEmail))
                            {
                                failedBuEmails.Add(failedBuEmail);
                            }
                            dataTransferSuccessful = false;
                            log.Debug("Exception during API call GetGeometryTier(Child): " + e);
                            retVal += "Project: " + p.Name + Environment.NewLine + Environment.NewLine + "Error: Failed to retrieve Child Geometry Data for Project " + p.Name + ".The exception was: " + e + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;

                        }

                    }

                    log.Debug("Project Forms List...");
                    List<FV_Call_Param> PF_CallParams = new List<FV_Call_Param>();
                    PF_CallParams.Add(new FV_Call_Param("apiToken", tokenPool.GetFreshToken().TokenString));
                    PF_CallParams.Add(new FV_Call_Param("projectId", p.ID.ToString()));
                    List<string> formTemplateLinkIds = new List<string>();
                    formTemplateLinkIds.Add("3878666");
                    PF_CallParams.Add(new FV_Call_Param("formTemplateLinkIds", FV_Param_Type.@int, formTemplateLinkIds));

                    PF_CallParams.Add(new FV_Call_Param("lastmodifiedDateFrom", dayStringFrom));
                    PF_CallParams.Add(new FV_Call_Param("lastmodifiedDateTo", dayStringTo));

                    callFunctionName = "GetProjectFormsList";
                    List<ProjectFormsListInformation> forms = new List<ProjectFormsListInformation>();

                    log.Debug("Creating FV API CALL object with parameters");
                    FV_API_Call fV_API_Call_PF = new FV_API_Call(FvReturnType.JSON, FVWebService.FORM, callFunctionName, PF_CallParams, tokenPool);
                    log.Debug("FV API CALL object created successfully");

                    try
                    {
                        log.Debug("Attempting to get JSON for Project Forms List call");
                        jsonResponseObject = fV_API_Call_PF.GetFVReponse(client);
                        forms = jsonResponseObject.ProjectFormsListInformation;
                        foreach (ProjectFormsListInformation pi in forms)
                        {
                            pi.ProjectID = p.ID;
                        }
                        PF_LI.AddRange(forms);
                        log.Debug("Added " + forms.Count + " to PF_LI list. Total PF_LI count is " + PF_LI.Count);
                        log.Debug("STATUS CODE " + jsonResponseObject.Status.Code + ", THE MESSAGE IS " + jsonResponseObject.Status.Message);
                    }
                    catch (Exception e)
                    {
                        failedBuEmail = BUlist.Where(x => x.buID == p.BU_ID).Select(x => x.buEmail).First();
                        if (!failedBuEmails.Contains(failedBuEmail))
                        {
                            failedBuEmails.Add(failedBuEmail);
                        }
                        dataTransferSuccessful = false;
                        log.Debug("Exception during API call GetProjectFormsList: " + e);
                        retVal += "Project: " + p.Name + Environment.NewLine + Environment.NewLine + "Error: Failed to retrieve Project Forms List for Project " + p.Name + ".The exception was: " + e + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;

                    }

                    log.Debug("Project Forms List Updated...");

                    List<FV_Call_Param> CallParams = new List<FV_Call_Param>();
                    CallParams.Add(new FV_Call_Param("apiToken", tokenPool.GetFreshToken().TokenString));
                    CallParams.Add(new FV_Call_Param("projectId", p.ID.ToString()));
                    formTemplateLinkIds = new List<string>();
                    formTemplateLinkIds.Add("3878666");
                    CallParams.Add(new FV_Call_Param("formTemplateLinkIds", FV_Param_Type.@int, formTemplateLinkIds));

                    CallParams.Add(new FV_Call_Param("lastmodifiedDateFrom", dayStringFrom/*"2019-11-01T23:59:59"*/));
                    CallParams.Add(new FV_Call_Param("lastmodifiedDateTo", dayStringTo/*"2020-01-19T23:59:59"*/));

                    callFunctionName = "GetProjectFormsListUpdated";
                    List<ProjectFormsListUpdatedInformation> forms_upd = new List<ProjectFormsListUpdatedInformation>();

                    FV_API_Call fV_API_Call = new FV_API_Call(FvReturnType.JSON, FVWebService.FORM, "GetProjectFormsListUpdated", CallParams, tokenPool);

                    try
                    {
                        jsonResponseObject = fV_API_Call.GetFVReponse(client);
                        forms_upd = jsonResponseObject.ProjectFormsListUpdatedInformation;
                        PF_LUI.AddRange(forms_upd);
                        log.Debug("Added " + forms_upd.Count + " to PF_LUI list. Total PF_LUI count is " + PF_LUI.Count);
                        log.Debug("STATUS CODE " + jsonResponseObject.Status.Code + ", THE MESSAGE IS " + jsonResponseObject.Status.Message);
                    }
                    catch (Exception e)
                    {
                        failedBuEmail = BUlist.Where(x => x.buID == p.BU_ID).Select(x => x.buEmail).First();
                        if (!failedBuEmails.Contains(failedBuEmail))
                        {
                            failedBuEmails.Add(failedBuEmail);
                        }
                        dataTransferSuccessful = false;
                        log.Debug("Exception during API call GetProjectFormsListUpdated: " + e);
                        retVal += "Project: " + p.Name + Environment.NewLine + Environment.NewLine + "Error: Failed to retrieve Project Forms List Updated for Project " + p.Name + ".The exception was: " + e + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;
                    }



                    try
                    {
                        bk.Commit(geo_forms);
                        bk.Commit(forms);
                        bk.Commit(forms_upd);
                    }
                    catch (Exception e)
                    {
                        failedBuEmail = BUlist.Where(x => x.buID == p.BU_ID).Select(x => x.buEmail).First();
                        if (!failedBuEmails.Contains(failedBuEmail))
                        {
                            failedBuEmails.Add(failedBuEmail);
                        }
                        dataTransferSuccessful = false;
                        retVal += "Project: " + p.Name + Environment.NewLine + Environment.NewLine + "Error: Failed to upload API provided data to the database for project " + p.Name + ".The exception was: " + e + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;

                    }
                    if (dataTransferSuccessful)
                        p.UploadDate = DateTime.Now;

                }

                try
                {
                    bk.Commit(PF_PI);
                }
                catch (Exception e)
                {
                    foreach (ccBusinessUnit bu in BUlist)
                    {
                        failedBuEmails.Add(bu.buEmail);
                    }
                    retVal += "Failed to upload project list information to the database. The exception was " + e + Environment.NewLine + "Process cancelled." + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;

                    retVal += "|" + String.Join(",", failedBuEmails.ToArray());
                    return retVal;
                }
            }
            if (failedBuEmails.Count > 0)
            {
                retVal += "|" + String.Join(",", failedBuEmails.ToArray());
            }
            try
            {
                using (AppEntities appDB = new AppEntities())
                {
                    stpr_name = "prc_PopulateGetProjects";
                    appDB.Database.ExecuteSqlCommand("EXEC prc_PopulateGetProjects");
                    log.Debug("prc_PopulateGetProjects executed successfully");

                    stpr_name = "prc_PopulateGetGeometryTier";
                    appDB.Database.ExecuteSqlCommand("EXEC prc_PopulateGetGeometryTier");
                    log.Debug("prc_PopulateGetGeometryTier executed successfully");

                    stpr_name = "prc_PopulateGetProjectListUpdated";
                    appDB.Database.ExecuteSqlCommand("EXEC prc_PopulateGetProjectListUpdated");
                    log.Debug("prc_PopulateGetProjectListUpdated executed successfully");

                    stpr_name = "prc_PopulateGetProjectFormsList";
                    appDB.Database.ExecuteSqlCommand("EXEC prc_PopulateGetProjectFormsList");
                    log.Debug("prc_PopulateGetProjectFormsList executed successfully");

                    stpr_name = "prc_UpdateInsertProjects";
                    appDB.Database.ExecuteSqlCommand("EXEC prc_UpdateInsertProjects");
                    log.Debug("prc_UpdateInsertProjects executed successfully");

                    stpr_name = "prc_UpdateInsertLocations";
                    appDB.Database.ExecuteSqlCommand("EXEC prc_UpdateInsertLocations");
                    log.Debug("prc_UpdateInsertLocations executed successfully");

                    stpr_name = "prc_populateCCProjectListUpdated";
                    appDB.Database.CommandTimeout = 600;
                    appDB.Database.ExecuteSqlCommand("EXEC prc_populateCCProjectListUpdated");
                    log.Debug("prc_populateCCProjectListUpdated executed successfully");
                }

            }
            catch (Exception e)
            {
                foreach (ccBusinessUnit bu in BUlist)
                {
                    failedBuEmails.Add(bu.buEmail);
                }
                retVal += "Failed to execute stored procedure " + stpr_name + ". The exception was " + e + Environment.NewLine + "Process cancelled." + Environment.NewLine + Environment.NewLine + "Application name: Customer Care" + Environment.NewLine;

                retVal += "|" + String.Join(",", failedBuEmails.ToArray());
                return retVal;
            }

            DateTime procEnd = DateTime.Now;
            TimeSpan totalRunTime = procEnd - procStart;
            log.Debug("FV_API Job Ended at " + procEnd);
            log.Debug("FV_API Job took " + totalRunTime + " to run!");
            //retVal = true;
            return retVal;
        }


    }
}
