//----------------------------------------------------------------------------------------------
// <copyright file="TabController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams;
    using System.Diagnostics;
    using Microsoft.Teams.Learning.BotMiddleware;
    using Microsoft.Teams.Learning.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Autofac;
    using Newtonsoft.Json.Linq;
    using System.Threading;
    using Microsoft.Teams.Learning.Models;
    using Microsoft.Teams.Learning.Cards;
    using System.Linq;
    using Microsoft.Bot.Connector.Teams.Models;
    using System.Collections.Generic;
    using Microsoft.Teams.Learning.Storage;
    using System.Net.Http;
    using System.Net;
    using System.Net.Http.Headers;

    /// <summary>
    /// The <see cref="TabController"/> returns the tabs provided by this app.
    /// </summary>
    public class TabController : ApiController
    {
        // A global place to store the service records.
        private static InMemoryServiceRecordStorage ServiceRecordStorage = new InMemoryServiceRecordStorage();

        /// <summary>
        /// Returns the My Service Record static tab.
        /// </summary>
        /// <returns>A <see cref="Task"/> that resolves to a <see cref="IHttpActionResult"/> with the response from the API.</returns>
        [HttpGet]
        [Route("myservicerecord")]
        public HttpResponseMessage Get(string userObjectId)
        {
            if (userObjectId == null)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            var record = ServiceRecordStorage.GetServiceRecordForAADObjectId(userObjectId);
            if (record == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            var response = new HttpResponseMessage();
            response.Content = new StringContent(record.User.Name + " wins: " + record.Wins + " losses: " + record.Losses + " ties: " + record.Ties);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}