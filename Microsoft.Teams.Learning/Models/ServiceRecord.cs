//----------------------------------------------------------------------------------------------
// <copyright file="ServiceRecord.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Models
{
    using Microsoft.Bot.Connector;

    public class ServiceRecord
    {
        public ChannelAccount User { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }
    }
}