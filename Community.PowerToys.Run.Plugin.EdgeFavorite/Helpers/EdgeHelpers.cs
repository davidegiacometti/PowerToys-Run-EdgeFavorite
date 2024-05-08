// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Wox.Infrastructure;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public static class EdgeHelpers
    {
        public static void OpenInEdge(FavoriteItem favorite, bool inPrivate, bool newWindow)
        {
            OpenInEdgeInternal(favorite.Profile, favorite.Url!, inPrivate, newWindow);
        }

        public static void OpenInEdge(ProfileInfo profileInfo, string urls, bool inPrivate, bool newWindow)
        {
            OpenInEdgeInternal(profileInfo, urls, inPrivate, newWindow);
        }

        private static void OpenInEdgeInternal(ProfileInfo profileInfo, string urls, bool inPrivate, bool newWindow)
        {
            var args = urls;

            if (inPrivate)
            {
                args += " -inprivate";
            }

            if (newWindow)
            {
                args += " -new-window";
            }

            args += $" -profile-directory=\"{profileInfo.Directory}\"";

            try
            {
                Helper.OpenInShell(@"shell:AppsFolder\Microsoft.MicrosoftEdge.Stable_8wekyb3d8bbwe!App", args);
            }
            catch (Exception ex)
            {
                Log.Exception("Failed to launch Microsoft Edge", ex, typeof(EdgeHelpers));
            }
        }
    }
}
