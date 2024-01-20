// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests.Mocks
{
    public class WorkFavoriteProvider : IFavoriteProvider
    {
        private readonly ProfileInfo _profileInfo = new("Work", "Profile 1");
        private readonly FavoriteItem _root;

        public FavoriteItem Root => _root;

        public WorkFavoriteProvider()
        {
            var coding = new FavoriteItem("Coding", "Coding");
            coding.AddChildren(new FavoriteItem("AWS", "https://aws.amazon.com/", "Coding/AWS", _profileInfo));
            coding.AddChildren(new FavoriteItem("Bitbucket", "https://bitbucket.org/", "Coding/Bitbucket", _profileInfo));
            coding.AddChildren(new FavoriteItem("Microsoft Azure", "https://portal.azure.com/", "Coding/Microsoft Azure", _profileInfo));

            _root = new FavoriteItem("Favorites bar", string.Empty);
            _root.AddChildren(new FavoriteItem("Gmail", "https://mail.google.com/", "Gmail", _profileInfo));
            _root.AddChildren(coding);
        }
    }
}
