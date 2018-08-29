//----------------------------------------------------------------------------------------------
// <copyright file="MessagesController.cs" company="Microsoft">
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

    /// <summary>
    /// The <see cref="MessagesController"/> exposes a POST endpoint on api/messages that receives and handles
    /// incoming activities from Bot Framework.
    /// Take careful note of the <see cref="BotAuthentication"/> attribute applied to this class.
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        // A global place to store the service records.
        private static InMemoryServiceRecordStorage ServiceRecordStorage = new InMemoryServiceRecordStorage();

        /// <summary>
        /// Receive a message from a Bot Framework and process it.
        /// </summary>
        /// <param name="activity">The incoming activity from Bot Framework.</param>
        /// <returns>A <see cref="Task"/> that resolves to a <see cref="IHttpActionResult"/> with the response from the API.</returns>
        [HttpPost]
        [Route("api/messages")]
        public async Task<IHttpActionResult> PostAsync([FromBody]Activity activity)
        {
            // Confirmation check - if activity is null - do nothing
            if (activity == null)
            {
                return Ok();
            }

            // Message activities are generally text messages sent from a user.
            // An action taken from an AdaptiveCard is also a Message activity with an empty Text property but a populated Value property.
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Text == null && activity.Value != null)
                {
                    return await HandleAdaptiveCardActionAsync(activity);
                }
                else
                {
                    return await HandleTextMessageAsync(activity);
                }
            }
            else if (activity.Type == ActivityTypes.Invoke)
            {
                return await HandleInvokeMessageAsync(activity);
            }
            else
            {
                // This is used to handle many other (some unsupported) types of messages
                return await HandleSystemMessageAsync(activity);
            }
        }

        /// <summary>
        /// Processes an <see cref="Activity"/> of <see cref="ActivityTypes.Message"/> type.
        /// </summary>
        /// <param name="activity">The incoming activity from Bot Framework.</param>
        /// <returns>A <see cref="Task"/> that resolves to a <see cref="IHttpActionResult"/> with the response from the API.</returns>
        private async Task<IHttpActionResult> HandleTextMessageAsync(Activity activity)
        {
            // This is used for removing the '@botName' from the incoming message so it can
            // be parsed correctly
            var messageActivity = StripBotAtMentions.StripAtMentionText(activity);
            try
            {
                // This sends all messages to the RootDialog for processing.
                await Conversation.SendAsync(messageActivity, () => new RootDialog());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return Ok();
        }

        /// <summary>
        /// Processes an <see cref="Activity"/> of <see cref="ActivityTypes.Invoke"/> type.
        /// </summary>
        /// <param name="activity">The incoming activity from Bot Framework.</param>
        /// <returns>A <see cref="Task"/> that resolves to a <see cref="IHttpActionResult"/> with the response from the API.</returns>
        private async Task<IHttpActionResult> HandleInvokeMessageAsync(Activity activity)
        {
            if (activity.IsComposeExtensionQuery())
            {
                // Parse out the search parameters to fetch the name of the user.
                var queryData = activity.GetComposeExtensionQueryData();
                var searchParam = "";
                if (queryData != null && queryData.Parameters != null && queryData.Parameters.Any(p => p.Name != "initialRun"))
                {
                    searchParam = queryData.Parameters.First().Value as string;
                }

                // Attempt to get all the members of the current conversation.
                var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));
                var members = new ChannelAccount[] { activity.From };
                try
                {
                    members = await connectorClient.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
                }
                catch (UnauthorizedAccessException)
                {
                    // Swallow this exception. It happens when the messaging extension is queried in a conversation that it has no access to.
                    // We can just pretend that there are no members and return an empty result set.
                }

                // Find the service record in the global service record storage or return an empty service record.
                var records = members.Select(member =>
                {
                    var record = ServiceRecordStorage.GetServiceRecordForUserId(member.Id);
                    return record ?? new ServiceRecord { User = member, Losses = 0, Wins = 0, Ties = 0 };
                });


                // Filter records to the search parameters
                if (!string.IsNullOrWhiteSpace(searchParam))
                {
                    records = records.Where(record => record.User.Name.ToLowerInvariant().Contains(searchParam.ToLowerInvariant()));
                }

                // Create cards out of the service records.
                var cards = records.Select(record =>
                {
                    // This is the preview that shows up in the list.
                    var preview = new ThumbnailCard
                    {
                        Text = record.User.Name + " wins: " + record.Wins + " losses: " + record.Losses + " ties: " + record.Ties
                    };

                    // This is the actual card that is pasted into the chat window.
                    var attachment = new HeroCard
                    {
                        Text = record.User.Name + " wins: " + record.Wins + " losses: " + record.Losses + " ties: " + record.Ties

                    }.ToAttachment()
                    .ToComposeExtensionAttachment(preview.ToAttachment());

                    return attachment;
                }).ToList();

                // This is the response format we must return.
                var response = new ComposeExtensionResponse
                {
                    ComposeExtension = new ComposeExtensionResult
                    {
                        AttachmentLayout = AttachmentLayoutTypes.List,
                        Type = "result",
                        Attachments = cards
                    }
                };
                return Ok(response);
            }
            return Ok();
        }

        /// <summary>
        /// Processes an <see cref="Activity"/> of <see cref="ActivityTypes.Message"/> type that is an adaptive card action.
        /// </summary>
        /// <param name="activity">The incoming activity from Bot Framework.</param>
        /// <returns>A <see cref="Task"/> that resolves to a <see cref="IHttpActionResult"/> with the response from the API.</returns>
        private async Task<IHttpActionResult> HandleAdaptiveCardActionAsync(Activity activity)
        {
            // Parse out the payload.
            var payload = activity.Value as JObject;
            var sessionId = payload.GetValue("sessionId").Value<string>();
            var conversationId = payload.GetValue("conversationId").Value<string>();
            var choice = (Choices)Enum.Parse(typeof(Choices), payload.GetValue("GameChoice").Value<string>());

            Match match = null;
            // Load the correct conversation data record and update it with the user's choice.
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, activity))
            {
                var botDataStore = scope.Resolve<IBotDataStore<BotData>>();
                var address = new Address(activity.Recipient.Id, activity.ChannelId, activity.From.Id, conversationId, activity.ServiceUrl);
                var data = await botDataStore.LoadAsync(address, BotStoreType.BotConversationData, CancellationToken.None);
                match = data.GetProperty<Match>(sessionId);
                foreach(var result in match.Results)
                {
                    if (result.User.Id == activity.From.Id)
                    {
                        result.Choice = choice;
                    }
                }
                data.SetProperty(sessionId, match);
                await botDataStore.SaveAsync(address, BotStoreType.BotConversationData, data, CancellationToken.None);
                await botDataStore.FlushAsync(address, CancellationToken.None);
            }

            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl));

            // Remove the response card.
            var thankYouResponse = Activity.CreateMessageActivity();
            var thanksCard = new ThanksForPlayingCard(choice);
            thankYouResponse.Attachments.Add(thanksCard.ToAttachment());
            await connectorClient.Conversations.UpdateActivityAsync(activity.Conversation.Id, activity.ReplyToId, (Activity)thankYouResponse);

            // Update the original card.
            var updateActivity = Activity.CreateMessageActivity();
            var resultCard = new ResultCard(match.Results);
            updateActivity.Attachments.Add(resultCard.ToAttachment());
            await connectorClient.Conversations.UpdateActivityAsync(conversationId, match.MessageId, (Activity)updateActivity);

            // Update service records if the match is done.
            if (!match.Results.Any(result => result.Choice == Choices.None))
            {
                foreach(var result in match.Results)
                {
                    var wins = match.Results.Count(otherResult => result.Choice.Beats(otherResult.Choice));
                    var losses = match.Results.Count(otherResult => otherResult.Choice.Beats(result.Choice));
                    var ties = match.Results.Count(otherResult => otherResult.User.Id != result.User.Id && !result.Choice.Beats(otherResult.Choice) && !otherResult.Choice.Beats(result.Choice));

                    var serviceRecord = ServiceRecordStorage.GetServiceRecordForUserId(result.User.Id);
                    if (serviceRecord == null)
                    {
                        serviceRecord = new ServiceRecord() { User = result.User, Losses = 0, Wins = 0, Ties = 0 };
                    }
                    serviceRecord.Wins += wins;
                    serviceRecord.Losses += losses;
                    serviceRecord.Ties += ties;

                    ServiceRecordStorage.SetServiceRecordForUserId(result.User.Id, serviceRecord);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Processes an <see cref="Activity"/> with all other <see cref="ActivityTypes"/> not specifically handled.
        /// </summary>
        /// <param name="activity">The incoming activity from Bot Framework.</param>
        /// <returns>A <see cref="Task"/> that resolves to a <see cref="IHttpActionResult"/> with the response from the API.</returns>
        private async Task<IHttpActionResult> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.ConversationUpdate)
            {
            }
            else if (message.Type == ActivityTypes.MessageReaction)
            {
            }

            return Ok();
        }
    }
}