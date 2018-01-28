using System;
using System.Linq;
using System.Threading;
using System.Web.Http;
using FoodaBot.Database;
using FoodaBot.Entites;
using SlackAPI;
using FoodaCore;

namespace FoodaBot.Controllers
{
    public class FoodaController : ApiController
    {
        public string Get()
        {
            if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday)
                return string.Empty;

            var db = new DataAccess();
            var userAlerts = db.GetAllUsersAlerts();

            foreach (var team in db.GetAllAuthorizations())
            {
                try
                {
                    var client = new SlackClient(team.AccessToken);
                    var message = Fooda.GetFoodaMenu(team.FoodaId);
                    client.PostMessage(null, team.ChannelId, message, "FoodaBot");
                    
                    foreach (var userAlert in userAlerts.Where(x=>x.AlertText != null && x.TeamId == team.TeamId))
                    {
                        if (message.IndexOf(userAlert.AlertText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            client.PostMessage(null,userAlert.UserId, $"Good news!  {userAlert.AlertText} is on Today's Fooda menu!", "FoodaBot");
                            Thread.Sleep(1000);
                        }
                    }                    
                }
                catch (Exception e)
                {                    
                }
                
            }

            return "hello";
        }

        public object Post(SlashCommand cmd)
        {
            string result;
            AuthorizationEntity teamAuth;
            var db = new DataAccess();            

            teamAuth = db.GetAuthorizationForTeam(cmd.team_id);
            if (teamAuth == null)
            {
                return new {text = "Your team is not recognized.  Please re-install the FoodaBot."};
            }

            var command = "HELP";
            var text = "";
            try
            {
                var splitResult = cmd.text.ToUpper().Split(' ');
                command = splitResult[0];
                text = string.Join(" ", splitResult.Skip(1));
            } catch { }

            if ((string.IsNullOrEmpty(teamAuth.FoodaId) || teamAuth.FoodaId == "NotSet") && command != "REGISTER")
            {
                return new { text = "Before we get started, I need to know where you are so I can show you the correct Fooda menu.  You'll need to give me one of your Fooda Popup Ids.  If you don't know what that is, click <http://www.google.com|here>.\r\n*Example*\r\n_/foodabot register 12345_" };
            }

            switch (command)
            {
                case "REGISTER":
                {
                    int value;
                    if (!int.TryParse(text.Trim(), out value))
                    {
                        return
                            new
                            {
                                text =
                                "I'm afraid I'm not understanding you.  To register, you'll need to give me a Fooda Popup Id.  \r\n*Example*\r\n_/foodabot register 12345_"
                            };
                    }

                    teamAuth.FoodaId = value.ToString();
                    db.UpdateAuthorization(teamAuth);
                    return
                        new
                        {
                            text =
                            "Perfect!  I'm now configured.  I will post your Fooda menu to Slack each day.  If you'd like to hear more about what I can do - type /foodabot help"
                        };
                }

                case "HELP":
                {
                    return
                        new
                        {
                            text =
                            "I'm the FoodaBot!  Each day I will post the Fooda menu to Slack.  You can also create and manage custom alerts.  Want to know when Pizza is on the menu?\r\n*Example*\r\n_/foodabot alert pizza_\r\n\r\nYou can also view and remove your alerts using _/foodabot list alerts_ and _/foodabot remove pizza_"
                        };
                }

                case "ALERT":
                {
                    db.InsertUserAlert((new UserAlertsEntity {TeamId = cmd.team_id, UserId = cmd.user_id, AlertText = text}).SetKeys());
                    return new {text = $"I'll let you know anytime '{text}' is on the menu."};
                }

                case "LIST":
                {
                    var alerts = db.GetAllUsersAlerts(cmd.team_id, cmd.user_id).Select(x=>x.AlertText);
                    var alertsText = string.Join("\r\n", alerts);
                    return new { text = $"Here are all of your current alerts:\r\n{alertsText}" };
                }

                case "REMOVE":
                {
                    var alertToRemove =
                        db.GetAllUsersAlerts(cmd.team_id, cmd.user_id)
                            .Where(x => string.Equals(x.AlertText, text, StringComparison.CurrentCultureIgnoreCase)).ToList();

                    if (!alertToRemove.Any())
                    {
                        return new { text = $"Ummm....I didn't find any alerts matching '{text}'.  Try using _/foodabot list alerts_ to see what alerts you've got configured." };
                    }
                    else
                    {
                        db.RemoveUserAlert(alertToRemove.First());
                        return new { text = $"Okay - I've removed '{text}' from your list of alerts." };
                    }
                    
                }
            }

            return new
            {
                text = "Command test"
            };
        }
    }

}
