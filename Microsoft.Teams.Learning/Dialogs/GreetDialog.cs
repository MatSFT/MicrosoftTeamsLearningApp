//----------------------------------------------------------------------------------------------
// <copyright file="GreetDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;
    using System;
    using System.Threading.Tasks;

    [Serializable]
    public class GreetDialog : IDialog<object>
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

            // For each member, create the conversation and send the greetings message
            foreach (var member in members)
            {
                // Do not greet the sender.
                if (member.Id == context.Activity.From.Id)
                {
                    continue;
                }

                // Fetch and increment the number of times that the user has been greeted.
                if (!context.ConversationData.TryGetValue(member.Id, out int timesGreeted))
                {
                    timesGreeted = 0;
                }
                timesGreeted++;
                context.ConversationData.SetValue(member.Id, timesGreeted);

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
                message.Text = "Greetings! (I have greeted you " + timesGreeted + " time" + (timesGreeted != 1 ? "s" : "") + ")";

                await connectorClient.Conversations.SendToConversationAsync((Activity)message);
            }

            context.Done<object>(null);
        }
    }
}