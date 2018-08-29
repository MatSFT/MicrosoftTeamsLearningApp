//----------------------------------------------------------------------------------------------
// <copyright file="RootDialog.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Dialogs
{
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Scorables;
    using Microsoft.Bot.Connector;
    using System;
    using System.Threading.Tasks;

    [Serializable]
    public class RootDialog : DispatchDialog
    {
        [RegexPattern("hello")]
        [RegexPattern("hi")]
        [ScorableGroup(1)]
        public async Task RunHelloDialog(IDialogContext context, IActivity activity)
        {
            context.Call(new HelloDialog(), this.EndDialog);
        }

        [MethodBind]
        [ScorableGroup(2)]
        public async Task Default(IDialogContext context, IActivity activity)
        {
            // Send message
            await context.PostAsync("I'm sorry, but I didn't understand.");
            context.Done<object>(null);
        }

        public async Task EndDialog(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
    }
}