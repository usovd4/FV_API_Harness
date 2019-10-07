using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FV_API_Harness
{
    class FV_API_Token
    {
        /// <summary>
        /// The string data of the token
        /// </summary>
        string TokenString { get; }
        /// <summary>
        /// The time at which the token last expired.
        /// </summary>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// Constructs a new API Token with an unknown expiration time
        /// </summary>
        /// <param name="tokenString"></param>
        public FV_API_Token(string tokenString)
        {
            TokenString = tokenString;
            ExpirationTime = null;
        }
    }
}
