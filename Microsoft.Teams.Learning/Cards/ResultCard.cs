//----------------------------------------------------------------------------------------------
// <copyright file="ResultCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Cards
{
    using System;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using AdaptiveCards;
    using System.Collections.Generic;
    using Microsoft.Teams.Learning.Models;
    using System.Linq;

    public class ResultCard
    {
        private IEnumerable<MatchResult> results;

        public ResultCard(IEnumerable<MatchResult> results)
        {
            this.results = results ?? throw new ArgumentNullException(nameof(results));
        }

        public Attachment ToAttachment()
        {
            var everyoneResponded = !this.results.Any(result => result.Choice == Choices.None);
            var responses = this.results.Select(result =>
            {
                var howManyBeat = this.results.Count(otherResult => result.Choice.Beats(otherResult.Choice));

                var respondedText = (result.Choice != Choices.None) ? "has responded." : "has not responded.";
                return new AdaptiveTextBlock
                {
                    Text = result.User.Name + " " + (everyoneResponded ? "chose " + result.Choice.ToString() + " and beat " + howManyBeat + " others." : respondedText)
                };
            }).ToList<AdaptiveElement>();

            var card = new AdaptiveCard
            {
                Body = responses
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }
}