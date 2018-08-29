//----------------------------------------------------------------------------------------------
// <copyright file="ServiceRecord.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Models
{
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Connector.Teams.Models;

    public class ServiceRecord
    {
        public TeamsChannelAccount User { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
    }
}