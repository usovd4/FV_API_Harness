using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FV_API_Harness
{
    public class FV_API_TokenPool
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            foreach (string tokenString in fV_API_Token_Strings)
            {
                _TokenList.Add(new FV_API_Token(tokenString));
            }

        }

        public void AddTokenToPool(FV_API_Token NewToken)
        {
            _TokenList.Add(NewToken);
        }

        public void SetExpirationTime(string tokenString)
        {
            _TokenList.Where(x => x.TokenString == tokenString).First().ExpirationTime = DateTime.Now;
        }

        /// <summary>
        /// Returns a fieldview call token. Handles trying to get a token known to be fresh
        /// </summary>
        /// <returns></returns>
        public FV_API_Token GetFreshToken()
        {
            //get a token that has a null expiry date or get a token that has an expiry date more than a minute ago
            FV_API_Token newtoken = new FV_API_Token("");
            if (_TokenList.Where(t => t.ExpirationTime == null).Count() > 0)
            {//Check for any tokens with null expiry times and provide them first
                newtoken = _TokenList.FirstOrDefault(t => t.ExpirationTime == null);
                log.Debug("Unused token found, token name is " + newtoken.TokenString);
                return newtoken;
            }
            else if (_TokenList.Where(t => (DateTime.Now - t.ExpirationTime).Value.TotalSeconds > 60).Count() > 0)
            {//check for any tokens that expired more than 1 minute ago
                log.Debug("No new tokens available, searching for a token that expired more than 1 minute ago");
                newtoken = _TokenList.FirstOrDefault(t => (DateTime.Now - t.ExpirationTime).Value.TotalSeconds > 60);
                log.Debug("Current time is " + DateTime.Now);
                log.Debug("Token expiration time is " + newtoken.ExpirationTime);
                log.Debug("Time difference is " + (DateTime.Now - newtoken.ExpirationTime).Value.TotalSeconds);
                return newtoken;
            }
            else
            {//Wait until the next token to refresh comes back online
                //since we known that none of the tokens have a null time we can safely cast nullable datetime to datetime
                //double secondsToWait = (DateTime.Now - (DateTime)_TokenList.OrderByDescending(t => t.ExpirationTime).Last().ExpirationTime).TotalSeconds;

                //System.Threading.Thread.Sleep((int)Math.Ceiling(secondsToWait) * 1000);
                System.Threading.Thread.Sleep(60000);
                return _TokenList.OrderByDescending(t => t.ExpirationTime).Last();
            }




        }


    }
}
