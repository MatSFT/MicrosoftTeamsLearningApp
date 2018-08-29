//----------------------------------------------------------------------------------------------
// <copyright file="ThanksForPlayingCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Cards
{
    using Microsoft.Bot.Connector;
    using AdaptiveCards;
    using System.Collections.Generic;
    using Microsoft.Teams.Learning.Models;

    public class ThanksForPlayingCard
    {
        private Choices choice;

        public ThanksForPlayingCard(Choices choice)
        {
            this.choice = choice;
        }

        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Thanks for playing!"
                    },
                    new AdaptiveTextBlock
                    {
                        Text = "Your choice has been recorded as " + choice.ToString(),
                        Size = AdaptiveTextSize.Small
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