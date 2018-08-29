//----------------------------------------------------------------------------------------------
// <copyright file="MatchResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Models
{
    using System;

    public class Match
    {
        public Guid SessionId { get; set; }
        public MatchResult[] Results { get; set; }
        public string MessageId { get; set; }
    }
}