using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace SafetyIndex
{
    public class DocumentDBRepository<T> where T :class
    {
       
        private static DocumentClient client;
        private static string databaseName;
        private static string collectionName;


        public DocumentDBRepository()
        {
 
            InitializeAsync();
        }

        public List<T> GetItems(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName))
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(query.ExecuteNextAsync<T>().ConfigureAwait(false).GetAwaiter().GetResult());
            }

            return results;
        }

        public void DeleteDatabase()
        {
            try
            {
                client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName)).ConfigureAwait(false);
                
            }
            catch (DocumentClientException de)
            {
               

                throw;
            }
        }

        public void DeleteDocument(string documentName)
        {
            try
            {
                client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentName)).ConfigureAwait(false);
               
            }
            catch (DocumentClientException de)
            {
               

                throw;
            }
        }
        public void ReplaceDocument(string documentName, T updatedObj)
        {
            try
            {
                client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentName), updatedObj).ConfigureAwait(false);
               
            }
            catch (DocumentClientException de)
            {
               

                throw;
            }
        }

        public IQueryable<T> ExecuteSimpleQuery(string query)
        {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Now execute the same query via direct SQL
            IQueryable<T> TQueryInSql = client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    query,
                    queryOptions);
            return TQueryInSql;
        }

        public void CreateDocumentIfNotExists(T obj, string documentId)
        {
            try
            {
                var task = Task.Run(async () => { await client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentId)); });
                task.Wait();
                //client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentId)).GetAwaiter().GetResult();
                // _logger.Log(LogLevel.Info, string.Format("Found {0}", documentId));
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    var task = Task.Run(async () => { await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), obj); });
                    task.Wait();
                    //client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), obj).GetAwaiter().GetResult();
                    // _logger.Log(LogLevel.Info, string.Format("Created Object {0}", documentId));
                }
                else
                {
                   

                    throw;
                }
            }
        }


        public void CreateDocument(T obj)
        {
            var task = Task.Run(async () => { await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), obj); });
            task.Wait();
        }

        private void CreateDatabaseIfNotExists()
        {
            // Check to verify a database with the id=FamilyDB does not exist
            try
            {
                client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName)).ConfigureAwait(false).GetAwaiter().GetResult();
                //_logger.Log(new LoggedInfo
                //{
                //    Type = LogType.Information,
                //    Message = string.Format("Found {0}", databaseName),
                //    MethodName = "CreateDatabaseIfNotExists"
                //});

            }
            catch (DocumentClientException de)
            {
                // If the database does not exist, create a new database
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    client.CreateDatabaseAsync(new Database { Id = databaseName }).ConfigureAwait(false).GetAwaiter().GetResult();
                   
                }
                else
                {
                   
                    throw;
                }
            }
        }

        private void CreateDocumentCollectionIfNotExists()
        {
            try
            {
               
                client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName)).ConfigureAwait(false).GetAwaiter().GetResult();

                //_logger.Log(new LoggedInfo
                //{
                //    Type = LogType.Information,
                //    Message = string.Format("Found {0}", collectionName),
                //    MethodName = "CreateDocumentCollectionIfNotExists"
                //});
            }
            catch (DocumentClientException de)
            {
                // If the document collection does not exist, create a new collection
                if (de.StatusCode == HttpStatusCode.NotFound)
                {
                    DocumentCollection collectionInfo = new DocumentCollection();
                    collectionInfo.Id = collectionName;

                    // Configure collections for maximum query flexibility including string range queries.
                    collectionInfo.IndexingPolicy = new IndexingPolicy(new RangeIndex(DataType.String) { Precision = -1 });

                    // Here we create a collection with 400 RU/s.


                    client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(databaseName),
                        collectionInfo,
                        new RequestOptions()).ConfigureAwait(false).GetAwaiter().GetResult();

                   
                }
                else
                {


                    throw;
                }
            }
            catch (Exception ex)
            {
                string uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName).ToString();
               
                //throw;
            }
        }

        private void InitializeAsync()
        {
            collectionName = "index";
            databaseName = "safetyIndex";
            try
            {
                if (client == null)
                {

                    string endpoint = "https://safetyindex.documents.azure.com:443/";
                    string authKey = "aL48fmGkxVFnHqOLvPFdditd113wTiga1ArnKY1hmXccFscEFWbOzpvqm0Kf9oq1tEZdFdlPpX2SjAVfl1jr0A==";

                    Uri endpointUri = new Uri(endpoint);
                    try
                    {
                        client = new DocumentClient(endpointUri, authKey);
                    }
                    catch (DocumentClientException de)
                    {
                        Exception baseException = de.GetBaseException();
                       
                    }
                    catch (Exception e)
                    {
                        Exception baseException = e.GetBaseException();

                       
                    }
                }
                CreateDatabaseIfNotExists();
                CreateDocumentCollectionIfNotExists();
            }
            catch (Exception ex)
            {
               
                throw;

              

            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
