using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FV_API_Harness
{
    class FV_API_TokenPool
    {
        private List<FV_API_Token> _TokenList;


        /// <summary>
        /// empty constructor for making an empty token pool
        /// </summary>
        public FV_API_TokenPool()
        {

        }

        /// <summary>
        /// Create a new token pool with a known list of tokens
        /// </summary>
        /// <param name="fV_API_Tokens"></param>
        public FV_API_TokenPool(List<FV_API_Token> fV_API_Tokens)
        {
            _TokenList = fV_API_Tokens;
        }
        public FV_API_TokenPool(List<string> fV_API_Token_Strings)
        {
            foreach(string tokenString in fV_API_Token_Strings)
            {
                _TokenList.Add(new FV_API_Token(tokenString));
            }

        }

        public void AddTokenToPool(FV_API_Token NewToken)
        {
            _TokenList.Add(NewToken);
        }

        /// <summary>
        /// Returns a fieldview call token. Handles trying to get a token known to be fresh
        /// </summary>
        /// <returns></returns>
        public FV_API_Token GetFreshToken()
        {
            //get a token that has a null expiry date or get a token that has an expiry date more than a minute ago

            if (_TokenList.Count(t => t.ExpirationTime == null) > 0)
            {//Check for any tokens with null expiry times and provide them first
                return _TokenList.FirstOrDefault(t => t.ExpirationTime == null);
            }
            else if (_TokenList.Count(t => t.ExpirationTime > DateTime.Now.AddMinutes(-1)) > 0)
            {//check for any tokens that expired more than 1 minute ago
                return _TokenList.FirstOrDefault(t => t.ExpirationTime > DateTime.Now.AddMinutes(-1));
            }
            else
            {//Wait until the next token to refresh comes back online
                //since we known that none of the tokens have a null time we can safely cast nullable datetime to datetime
                double secondsToWait = (DateTime.Now - (DateTime)_TokenList.OrderByDescending(t => t.ExpirationTime).Last().ExpirationTime).TotalSeconds ;

                System.Threading.Thread.Sleep((int)Math.Ceiling(secondsToWait) * 1000);
                return _TokenList.OrderByDescending(t => t.ExpirationTime).Last();
            }




        }


    }
}
