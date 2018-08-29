//----------------------------------------------------------------------------------------------
// <copyright file="Choices.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace Microsoft.Teams.Learning.Models
{
    public enum Choices
    {
        None = 0,
        Rock = 1,
        Paper = 2,
        Scissors = 3
    }

    public static class ChoicesExtensions
    {
        public static bool Beats(this Choices choice, Choices other)
        {
            if (choice == Choices.None || other == Choices.None)
            {
                return false;
            } 

            return (choice == Choices.Paper && other == Choices.Rock) ||
                 (choice == Choices.Rock && other == Choices.Scissors) ||
                  (choice == Choices.Scissors && other == Choices.Paper);
        }
    }
}