using SlackAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SlackAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                //ManualResetEventSlim clientReady = new ManualResetEventSlim(false);
                //SlackSocketClient client2 = new SlackSocketClient("xoxb-146443698115-MntL68DGkXMjU2hyuHEWJ70H");
                
                //client2.Connect((connected) => {
                //    // This is called once the client has emitted the RTM start command
                //    clientReady.Set();
                //    foreach (var a in connected.channels)
                //    {
                //        Console.WriteLine(a.id + " " + a.name);
                //    }
                //}, () => {
                //    var id = client2.Channels.Where(x => x.name == "general").First();
                //    // This is called once the RTM client has connected to the end point
                //    client2.SendMessage((m)=> 
                //    {
                //        Console.WriteLine("sent message" + m.error);
                //    }, id.id, "This should work");
                //});
                //client2.OnMessageReceived += (message) =>
                //{
                //    Console.WriteLine(message);
                //    // Handle each message as you receive them
                //};
                //clientReady.Wait();

                var clientId = "146556577221.145791896241";
                var clientSecret = "4a9c2119c6705ff83462cbc8fde45333";
                var redirectUri = "http://www.thisisatest.com/somethinggood/";

                Console.WriteLine("------------------------------------------------------------------");
                Console.WriteLine("This app will open your web browser pointing at an authentication");
                Console.WriteLine("page. When you complete authentication, you'll be sent back to ");
                Console.WriteLine("whatever 'redirectUri' is above, plus some query-string values. ");
                Console.WriteLine("Paste the URI into the console window when prompted.");
                Console.WriteLine();
                Console.WriteLine("In a proper web application, the user experience will obviously");
                Console.WriteLine("be more sensible...");
                Console.WriteLine("------------------------------------------------------------------");

                // start...
                var state = Guid.NewGuid().ToString();
                var uri = SlackClient.GetAuthorizeUri(clientId, SlackScope.Identify | SlackScope.Read | SlackScope.Post, redirectUri, state, "TestingSlack");
                Console.WriteLine("Directing to: " + uri);
                Process.Start(uri.ToString());

                // read the result -- in a web application you can pick this up directly, here we're fudging it...
                Console.WriteLine("Paste in the URL of the authentication result...");
                var asString = Console.ReadLine();
                var index = asString.IndexOf('?');
                if (index != -1)
                    asString = asString.Substring(index + 1);

                // parse...
                var qs = HttpUtility.ParseQueryString(asString);
                var code = qs["code"];
                var newState = qs["state"];

                // validate the state. this isn't required, but it's makes sure the request and response line up...
                if (state != newState)
                    throw new InvalidOperationException("State mismatch.");

                // then get the token...
                Console.WriteLine("Requesting access token...");
                SlackClient.GetAccessToken((response) =>
                    {
                        var accessToken = response.access_token;
                        Console.WriteLine("Got access token '{0}'...", accessToken);

                        // post...
                        //var client = new SlackClient(accessToken);
                        //client.PostMessage(null, "#general", "Test", "Jo the Robot");

                        // Try this other thing:


                    }, clientId, clientSecret, redirectUri, code);

                // finished...
                Console.WriteLine("Done.");

                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
