using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ManageService.Models;
using ManageService.Helpers;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using StackExchange.Redis;
using RawRabbit.Enrichers.MessageContext.Context;
using RawRabbit.Operations.MessageSequence;
using RawRabbit;

namespace ManageService.Controllers
{

    [Route("api/[controller]")]
    public class ManageController : Controller {
        IBusClient client;
        IDatabase cachingDB;

        public ManageController (IBusClient _client, IRedisConnectionFactory caching) {
            client = _client;
            cachingDB = caching.Connection().GetDatabase();
        }

        [HttpGet]
        [Route("/ReadFromCache/{id}")]
        public Data ReadFromCache(string id) {
            Data d = Newtonsoft.Json.JsonConvert.DeserializeObject<Data>(cachingDB.StringGet(id.ToString()));
            return d;
        }

        private bool VerifyTheToken(string id)
        {
            int index = id.LastIndexOf(':');
            string newUserId = id.Substring(index + 1, id.Length - index - 1);
            Data getData = ReadFromCache("id:" + newUserId);

             if ((DateTime.Now - getData.create.AddSeconds(getData.ttl)) < TimeSpan.FromHours(1))
             {
                 return true;
             }
             else
             {
                 return false;
             }
        }

        [HttpGet]
        [Route("/init")]
        public void init() { }

        [HttpPost]
        [Route("/ShareFile")]
        public async Task<string> ShareFile([FromBody] ShareFile sf) {
            if(VerifyTheToken(sf._id)){
                ShareFileNoRev shareNoRev = new ShareFileNoRev(sf);

                // check if toUser exist
                var response = await CouchDBConnect.PostToDB(shareNoRev, "shares");
                
                // create the same file with diffrenet id
                string json = JsonConvert.SerializeObject(shareNoRev);
                await client.PublishAsync(json);

                Console.WriteLine(response);
                return "Image shared to " + sf.toUser + " successfully";
            }
            else
            {
                return "Please login first!";
            }
        }
    }
}