using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlackAPI
{
    [RequestPath("oauth.access")]
    public class AccessTokenResponse : Response
    {
        public string ok;
        public string access_token;
        public string scope;
        public string user_id;
        public string team_name;
        public string team_id;
        public IncomingWebhook incoming_webhook;
    }
}
