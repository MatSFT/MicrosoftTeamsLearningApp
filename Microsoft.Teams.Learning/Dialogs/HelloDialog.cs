//----------------------------------------------------------------------------------------------
// <copyright file="HelloDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using System;
    using System.Threading.Tasks;

    [Serializable]
    public class HelloDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            
            // Send the message 'Hello!'
            await context.PostAsync("Hello!");

            // Return back to the RootDialog
            context.Done<object>(null);
        }
    }
}