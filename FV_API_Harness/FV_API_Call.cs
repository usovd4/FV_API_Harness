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

namespace FV_API_Harness
{
    public class FV_API_Call
    {

        public FvReturnType FVReturnType { get; }
        public FVWebService FVWebService { get; }

        public List<FV_Call_Param> CallParams { get; }
        string FunctionName { get; }

        /// <summary>
        /// Constructor to set-up a new request
        /// </summary>
        /// <param name="fVReturnType"></param>
        /// <param name="fVWebService"></param>
        /// <param name="FunctionName"></param>
        /// <param name="callParams"></param>
        public FV_API_Call(FvReturnType fVReturnType, FVWebService fVWebService, string functionName, List<FV_Call_Param> callParams)
        {
            FVReturnType = fVReturnType;
            FVWebService = fVWebService;
            CallParams = callParams;
            FunctionName = functionName;
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





        public string GetFVReponse()
        {

            string callURI = $"https://www.priority1.uk.net/FieldViewWebServices/WebServices/{FVReturnType}/{ConvertServiceToString(FVWebService)}";

            var client = new RestClient(callURI);

            var request = new RestRequest(Method.POST);

            // Json to post.
            string postBody = SoapEnvelope();
            request.AddHeader("Content-Type", "text/xml; charset=utf-8");
            request.AddHeader("SOAPAction", $"https://localhost.priority1.uk.net/Priority1WebServices/{FVReturnType}/{FunctionName}");
            request.AddParameter("text/xml", postBody, "text/xml", ParameterType.RequestBody);
            request.AddXmlBody(postBody);

            var response = client.Execute(request);



            return response.Content;
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


