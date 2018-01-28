using System;
using System.Threading;
using System.Web.Http;
using FoodaBot.Database;
using FoodaBot.Entites;
using SlackAPI;

namespace FoodaBot.Controllers
{
    public class AuthController : ApiController
    {
        private const string ClientId = "4999081824.144165270454";
        private const string ClientSecret = "819bebccb63920c7bbd912de7477ba70";

        /// <summary>
        /// Called with temporary token.  Will be used to generate authorization token and persisted for future use
        /// </summary>
        /// <param name="code">tempory token</param>
        /// <param name="state">Not used but included for future development</param>        
        public void Get(string code, string state = "")
        {
            try
            {
                // Like await - but old school
                var ewh = new EventWaitHandle(false, EventResetMode.ManualReset, code);
                SlackClient.GetAccessToken(response =>
                {
                    using (var da = new DataAccess())
                    {
                        da.InsertAuthorization(new AuthorizationEntity
                            {
                                AccessToken = response.access_token,
                                Channel = response.incoming_webhook.channel,
                                ChannelId = response.incoming_webhook.channel_id,
                                TeamId = response.team_id,
                                TeamName = response.team_name,
                                Url = response.incoming_webhook.url
                            }.SetKeys()
                        );
                    }

                    ewh.Set();

                }, ClientId, ClientSecret, "", code);

                ewh.WaitOne(50000);
            }
            catch (Exception e)
            {
                var error = e;
            }
        }

        public string Get()
        {            
            return "";
        }
    }
}
