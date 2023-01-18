﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class LoginPanelStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.LoginPanel";

        /// <summary>
        /// "Do not disturb"
        /// </summary>
        public static LocalisableString DoNotDisturb => new TranslatableString(getKey(@"do_not_disturb"), @"Do not disturb");

        /// <summary>
        /// "Appear offline"
        /// </summary>
        public static LocalisableString AppearOffline => new TranslatableString(getKey(@"appear_offline"), @"Appear offline");

        /// <summary>
        /// "Signed in"
        /// </summary>
        public static LocalisableString SignedIn => new TranslatableString(getKey(@"signed_in"), @"Signed in");

        /// <summary>
        /// "ACCOUNT"
        /// </summary>
        public static LocalisableString Account => new TranslatableString(getKey(@"account"), @"ACCOUNT");

        /// <summary>
        /// "Remember username"
        /// </summary>
        public static LocalisableString RememberUsername => new TranslatableString(getKey(@"remember_username"), @"Remember username");

        /// <summary>
        /// "Stay signed in"
        /// </summary>
        public static LocalisableString StaySignedIn => new TranslatableString(getKey(@"stay_signed_in"), @"Stay signed in");

        /// <summary>
        /// "Register"
        /// </summary>
        public static LocalisableString Register => new TranslatableString(getKey(@"register"), @"Register");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}