//----------------------------------------------------------------------------------------------
// <copyright file="MatchCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Cards
{
    using System;
    using Microsoft.Bot.Connector;
    using AdaptiveCards;
    using System.Collections.Generic;

    public class MatchCard
    {
        private string conversationId;
        private Guid sessionId;

        public MatchCard(string conversationId, Guid sessionId)
        {
            this.conversationId = conversationId;
            this.sessionId = sessionId;
        }

        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Choose your action:"
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = "GameChoice",
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice
                            {
                                Title = "Rock",
                                Value = "Rock"
                            },
                            new AdaptiveChoice
                            {
                                Title = "Paper",
                                Value = "Paper"
                            },
                            new AdaptiveChoice
                            {
                                Title = "Scissors",
                                Value = "Scissors"
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Data = new Dictionary<string, object> { {"sessionId", sessionId.ToString() }, { "conversationId", conversationId } },
                        Title = "Submit"
                    }
                }
                
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }
}