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
    using System.Diagnostics;
    using Microsoft.Teams.Learning.BotMiddleware;
    using Microsoft.Teams.Learning.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Autofac;
    using Newtonsoft.Json.Linq;
    using System.Threading;
    using Microsoft.Teams.Learning.Models;
    using Microsoft.Teams.Learning.Cards;

    /// <summary>
    /// The <see cref="MessagesController"/> exposes a POST endpoint on api/messages that receives and handles
    /// incoming activities from Bot Framework.
    /// Take careful note of the <see cref="BotAuthentication"/> attribute applied to this class.
    /// </summary>
    [BotAuthentication]
    public class MessagesController : ApiController
    {
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