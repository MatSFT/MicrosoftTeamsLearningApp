//----------------------------------------------------------------------------------------------
// <copyright file="MatchResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Models
{
    using Microsoft.Bot.Connector;

    public class MatchResult
    {
        public ChannelAccount User { get; set; }
        public Choices Choice { get; set; }
    }
}