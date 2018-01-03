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

        public ManageController (IBusClient _client) {
            client = _client;
        }

        [HttpPost]
        [Route("/ShareFile")]
        public async Task<int> ShareFile([FromBody] ShareFile sf) {
            ShareFileNoRev shareNoRev = new ShareFileNoRev(sf);

            // check if toUser exist
            var response = await CouchDBConnect.PostToDB(shareNoRev, "shares");
            
            // create the same file with diffrenet id
            await client.PublishAsync(shareNoRev);

            Console.WriteLine(response);
            return 1;
        }
    }
}