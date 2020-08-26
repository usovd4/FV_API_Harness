using FV_API_Harness.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using RestSharp;
using System.Xml;
using Newtonsoft.Json;
using System.Collections;
using System.Configuration;

namespace FV_API_Harness
{
    public class FV_API_Call
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public FvReturnType FVReturnType { get; }
        public FVWebService FVWebService { get; }

        public List<FV_Call_Param> CallParams { get; }
        string FunctionName { get; }
        public FV_API_TokenPool TokenPool { get; }

        /// <summary>
        /// Constructor to set-up a new request
        /// </summary>
        /// <param name="fVReturnType"></param>
        /// <param name="fVWebService"></param>
        /// <param name="FunctionName"></param>
        /// <param name="callParams"></param>
        public FV_API_Call(FvReturnType fVReturnType, FVWebService fVWebService, string functionName, List<FV_Call_Param> callParams, FV_API_TokenPool tokenPool)
        {
            FVReturnType = fVReturnType;
            FVWebService = fVWebService;
            CallParams = callParams;
            FunctionName = functionName;
            TokenPool = tokenPool;
        }


        private string ConvertServiceToString(FVWebService fVWebService)
        {
            switch (fVWebService)
            {
                case FVWebService.FORM:
                    return "API_FormsServices.asmx";
                case FVWebService.CONFIGURATION:
                    return "API_ConfigurationServices.asmx";
                case FVWebService.PROJECT:
                    return "";
                case FVWebService.TASK:
                    return "";

                default:
                    throw new Exception();
            }
        }


        /// <summary>
        /// Builds the soap envelope using the data given in the constructor of the FV Call
        /// </summary>
        /// <returns></returns>
        private string SoapEnvelope()
        {
            string SoapEnvelope = "";


            //Loop over the parameters provided to this call and get the parameter defintion for each. 
            //Build this together to produce the section for parameters in the soap envelope
            string EnvelopeParams = "";
            foreach (FV_Call_Param param in CallParams)
            {
                EnvelopeParams += param.GetParamString() + Environment.NewLine;
            }


            SoapEnvelope = string.Format(Resources.SoapEnvelopeSkeleton, FunctionName, FVReturnType, EnvelopeParams);

            return SoapEnvelope;
        }





        public Response GetFVReponse(RestClient client)
        {
            string fv_url_basepath = ConfigurationManager.AppSettings["FV_API_URL"];
            string soap_action_url = ConfigurationManager.AppSettings["SOAP_ACTION_URL"];
            string callURI = $"{fv_url_basepath}{FVReturnType}/{ConvertServiceToString(FVWebService)}";

            //var client = new RestClient(callURI);
            client.BaseUrl = new Uri(callURI);
            log.Debug("Changed the Uri of the client to "+callURI);


            var request = new RestRequest(Method.POST);

            // Json to post.
            string postBody = SoapEnvelope();
            request.AddHeader("Content-Type", "text/xml; charset=utf-8");
            request.AddHeader("SOAPAction", $"{soap_action_url}{FVReturnType}/{FunctionName}");
            request.AddParameter("text/xml", postBody, "text/xml", ParameterType.RequestBody);
            request.AddXmlBody(postBody);

            log.Debug("Executing the request");
            var response = client.Execute(request);
            string apiResponseString = response.Content;
            log.Debug("Response received");

            XmlDocument myxml = new XmlDocument();
            myxml.LoadXml(apiResponseString);
            string fvJsonResponse = myxml.GetElementsByTagName(FunctionName + "Result").Item(0).InnerXml;
            Response jsonResponseObject = JsonConvert.DeserializeObject<Response>(fvJsonResponse);

            //Status Code for token timeout
            if (jsonResponseObject.Status.Code == 11)
            {
                log.Debug("Token expired - attempting to find new token...");
                int index = CallParams.FindIndex(x => x.ParamName == "apiToken");
                string currentToken = CallParams[index].ParamVal;
                TokenPool.SetExpirationTime(currentToken);
                CallParams.RemoveAt(index);
                CallParams.Insert(index, new FV_Call_Param("apiToken", TokenPool.GetFreshToken().TokenString));
                return GetFVReponse(client);

            //Status Code for success
            }else if(jsonResponseObject.Status.Code == 2)
            {
                log.Debug("Result successful - removing special characters...");
                RemoveSpCharFromResponse(jsonResponseObject);
            }
            else
            {
                Exception e = new Exception("Unknown response status code "+jsonResponseObject.Status.Code + ". The message was "+jsonResponseObject.Status.Message);
                throw e;
            }


            return jsonResponseObject;
        }

        private void RemoveSpCharFromResponse(Response jsonResponse)
        {
            foreach (var objectList in jsonResponse.GetType().GetProperties())
            {
                if (objectList.PropertyType.IsGenericType && (objectList.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                {
                    var objectToList = (IEnumerable)objectList.GetValue(jsonResponse, null);
                    foreach(var objectInList in objectToList)
                    {
                        foreach (var property in objectInList.GetType().GetProperties())
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                try
                                {
                                    if (property.GetValue(objectInList, null) != null)
                                        property.SetValue(objectInList, WebUtility.HtmlDecode(property.GetValue(objectInList, null).ToString()));
                                }
                                catch (Exception e)
                                {
                                    string exp = e.ToString();
                                }

                            }
                        }
                    }
                }
            }

        }

    }


    public enum FvReturnType
    {
        JSON = 0,
        XML = 1
    }

    public enum FVWebService
    {
        CONFIGURATION = 0,
        FORM = 1,
        TASK = 2,
        PROCESS = 3,
        ASSET = 4,
        PROJECT = 5
    }


}
