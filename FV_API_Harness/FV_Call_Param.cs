using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FV_API_Harness
{
    public class FV_Call_Param
    {
        public string ParamName { get; }
        public FV_Param_Type ParamType { get; }
        public List<String> ParamVals { get; }
        public string ParamVal { get; }

        /// <summary>
        /// Constructor for an array parameter
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramType"></param>
        /// <param name="paramVals"></param>
        public FV_Call_Param(string paramName, FV_Param_Type paramType, List<string> paramVals)
        {
            ParamName = paramName;
            ParamType = paramType;
            ParamVals = paramVals;
        }

        /// <summary>
        /// Constructor for a single item parameter
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramVal"></param>
        public FV_Call_Param(string paramName, string paramVal)
        {
            ParamName = paramName;
            ParamVal = paramVal;
        }

        /// <summary>
        /// Returns the string for this parameter as it should go into the soap envelope
        /// </summary>
        /// <returns></returns>
        public string GetParamString()
        {

            if (ParamVals == null)
            {
                return $"<{ParamName}>{ParamVal}</{ParamName}>";
            }
            else
            {

                string arrayParamString = "";

                arrayParamString += $"<{ParamName}>";

                foreach (string param in ParamVals)
                {
                    arrayParamString += $"<{ParamType.ToString()}>{param}</{ParamType.ToString()}>";

                }
                arrayParamString += $"</{ParamName}>";


                return arrayParamString;
            }
        }
    }

    public enum FV_Param_Type
    {
        @int = 0,
        @string = 1
    }


}
