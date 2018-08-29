//----------------------------------------------------------------------------------------------
// <copyright file="GameMatchDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;
    using Microsoft.Teams.Learning.Cards;
    using Microsoft.Teams.Learning.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [Serializable]
    public class GameMatchDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Create the connector client which will be used to make requests for roster and conversation data.
            var connectorClient = new ConnectorClient(new Uri(context.Activity.ServiceUrl));

            // Fetch members of this conversation and data from the current conversation.
            var members = await connectorClient.Conversations.GetConversationMembersAsync(context.Activity.Conversation.Id);
            var channelData = context.Activity.GetChannelData<TeamsChannelData>();
            var bot = context.Activity.Recipient;

            // Create the match and initial results.
            var matchResults = members.Select(member => new MatchResult { User = member, Choice = Choices.None });
            var match = new Match
            {
                SessionId = Guid.NewGuid(),
                Results = matchResults.ToArray()
            };

            // Build the result card message
            var resultCard = new ResultCard(match.Results);
            var resultMessage = context.MakeMessage();
            resultMessage.Attachments.Add(resultCard.ToAttachment());

            // Keep track of this message so that we can update it later.
            var resource = await connectorClient.Conversations.ReplyToActivityAsync((Activity)resultMessage);
            match.MessageId = resource.Id;
            context.ConversationData.SetValue(match.SessionId.ToString(), match);

            // For each member, create the conversation and send the greetings message
            foreach (var member in members)
            {
                // Build the 1:1 conversation parameters
                var parameters = new ConversationParameters
                {
                    Bot = bot,
                    Members = new ChannelAccount[] { member },
                    ChannelData = new TeamsChannelData
                    {
                        Tenant = channelData.Tenant
                    }
                };

                // Create the conversation. If the bot has never talked to the user before this conversation will not exist; this ensures that the conversation exists.
                var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);

                // Create and send the response message.
                var message = Activity.CreateMessageActivity();
                message.From = bot;
                message.Conversation = new ConversationAccount(id: conversationResource.Id);

                // Ensure that the match card has the conversation Id and the session Id to be able to pull up and update the appropriate data later.
                var matchCard = new MatchCard(context.Activity.Conversation.Id, match.SessionId);
                message.Attachments.Add(matchCard.ToAttachment());

                await connectorClient.Conversations.SendToConversationAsync((Activity)message);
            }

            context.Done<object>(null);
        }
    }
}