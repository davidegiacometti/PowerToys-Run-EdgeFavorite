// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Wox.Infrastructure;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public static class EdgeHelpers
    {
        public static void Open(FavoriteItem favorite, bool inPrivate, bool newWindow)
        {
            OpenInternal(favorite.Profile, favorite.Url!, inPrivate, newWindow);
        }

        public static void Open(FavoriteItem[] favorites, bool inPrivate, bool newWindow)
        {
            if (favorites.Length == 0)
            {
                throw new InvalidOperationException("Favorites cannot be empty.");
            }

            // If there is no need to open in a new window, starting multiple processes is preferred to avoid long command line arguments
            if (newWindow)
            {
                Open(favorites[0].Profile, string.Join(" ", favorites.Select(f => f.Url!)), inPrivate, newWindow);
            }
            else
            {
                foreach (var favorite in favorites)
                {
                    Open(favorite, inPrivate, false);
                }
            }
        }

        private static void Open(ProfileInfo profileInfo, string urls, bool inPrivate, bool newWindow)
        {
            OpenInternal(profileInfo, urls, inPrivate, newWindow);
        }

        private static void OpenInternal(ProfileInfo profileInfo, string urls, bool inPrivate, bool newWindow)
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
