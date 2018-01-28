using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using FoodaBot.Entites;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace FoodaBot.Database
{
    public class DataAccess : IDisposable
    {
        private readonly CloudTableClient _tableClient;
        public DataAccess()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
            _tableClient = account.CreateCloudTableClient();
        }

        #region Authorizations Table
        public bool InsertAuthorization(AuthorizationEntity auth)
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.Authorizaiton);
                var insertOperation = TableOperation.Insert(auth);
                table.Execute(insertOperation);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<AuthorizationEntity> GetAllAuthorizations()
        {                        
            var table = _tableClient.GetTableReference(Tables.Authorizaiton);
            TableContinuationToken token = null;
            var entities = new List<AuthorizationEntity>();
            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<AuthorizationEntity>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        public bool DeleteAllAuthorizations()
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.Authorizaiton);
                var entites = GetAllAuthorizations();
                foreach (var e in entites)
                {
                    var deleteOperation = TableOperation.Delete(e);
                    table.Execute(deleteOperation);
                }
                
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void Dispose()
        {            
        }

        public AuthorizationEntity GetAuthorizationForTeam(string teamId)
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.Authorizaiton);
                var rangeQuery = new TableQuery<AuthorizationEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, teamId));

                return table.ExecuteQuery(rangeQuery).FirstOrDefault();                
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void UpdateAuthorization(AuthorizationEntity teamAuth)
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.Authorizaiton);
                var replaceOperation = TableOperation.Replace(teamAuth);
                table.Execute(replaceOperation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        #endregion

        #region UserAlerts Table
        public List<UserAlertsEntity> GetAllUsersAlerts(string teamId, string userId)
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.UserAlerts);
                var rangeQuery = new TableQuery<UserAlertsEntity>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

                return table.ExecuteQuery(rangeQuery).ToList().Where(x=>x.TeamId == teamId).ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<UserAlertsEntity> GetAllUsersAlerts()
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.UserAlerts);
                TableContinuationToken token = null;
                var entities = new List<UserAlertsEntity>();
                do
                {
                    var queryResult = table.ExecuteQuerySegmented(new TableQuery<UserAlertsEntity>(), token);
                    entities.AddRange(queryResult.Results);
                    token = queryResult.ContinuationToken;
                } while (token != null);

                return entities;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public bool InsertUserAlert(UserAlertsEntity userAlert)
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.UserAlerts);
                var insertOperation = TableOperation.Insert(userAlert);
                table.Execute(insertOperation);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }        

        public bool RemoveUserAlert(UserAlertsEntity userAlert)
        {
            try
            {
                var table = _tableClient.GetTableReference(Tables.UserAlerts);
                var deleteOperation = TableOperation.Delete(userAlert);
                table.Execute(deleteOperation);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion
    }

    internal class Tables
    {
        public static string Authorizaiton => "authorization";
        public static string UserAlerts => "userAlerts";
    }

}