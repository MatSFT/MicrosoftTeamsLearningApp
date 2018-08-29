//----------------------------------------------------------------------------------------------
// <copyright file="InMemoryServiceRecordStorage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Storage
{
    using System;
    using Microsoft.Teams.Learning.Models;
    using System.Collections.Generic;

    public class InMemoryServiceRecordStorage
    {
        private static Dictionary<string, ServiceRecord> ServiceRecords = new Dictionary<string, ServiceRecord> { };

        public ServiceRecord GetServiceRecordForUserId(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            ServiceRecords.TryGetValue(userId, out ServiceRecord value);
            return value;
        }

        public void SetServiceRecordForUserId(string userId, ServiceRecord record)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }
            ServiceRecords.Add(userId, record);
        }
    }
}