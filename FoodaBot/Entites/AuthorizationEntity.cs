using Microsoft.WindowsAzure.Storage.Table;

namespace FoodaBot.Entites
{
    public class AuthorizationEntity : TableEntity
    {        
        public AuthorizationEntity SetKeys()
        {
            RowKey = AccessToken;
            PartitionKey = TeamId;
            FoodaId = "NotSet";

            return this;
        }

        public string AccessToken { get; set; }
        public string TeamName { get; set; }
        public string TeamId { get; set; }
        public string Channel { get; set; }
        public string ChannelId { get; set; }        
        public string Url { get; set; }
        public string FoodaId { get; set; }
    }
}