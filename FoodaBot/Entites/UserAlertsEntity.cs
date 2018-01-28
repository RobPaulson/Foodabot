using Microsoft.WindowsAzure.Storage.Table;

namespace FoodaBot.Entites
{
    public class UserAlertsEntity : TableEntity
    {        
        public UserAlertsEntity SetKeys()
        {
            RowKey = AlertText;
            PartitionKey = UserId;            

            return this;
        }

        public string UserId { get; set; }
        public string TeamId { get; set; }
        public string AlertText { get; set; }
    }
}