﻿// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests.Mocks
{
    public class DefaultFavoriteProvider : IFavoriteProvider
    {
        private readonly ProfileInfo _profileInfo = new("Default", "Default");
        private readonly FavoriteItem _root;

        public FavoriteItem Root => _root;

        public DefaultFavoriteProvider()
        {
            var coding = new FavoriteItem("Coding", "Coding");
            coding.AddChildren(new FavoriteItem("GitHub", "https://github.com/", "Coding/GitHub", _profileInfo));
            coding.AddChildren(new FavoriteItem("Microsoft Azure", "https://portal.azure.com/", "Coding/Microsoft Azure", _profileInfo));
            coding.AddChildren(new FavoriteItem("Microsoft Developer Blogs", "https://devblogs.microsoft.com/", "Coding/Microsoft Developer Blogs", _profileInfo));

            var tools = new FavoriteItem("Tools", "Coding");
            tools.AddChildren(new FavoriteItem("JWT", "https://jwt.io/", "Coding/Tools/JWT", _profileInfo));
            tools.AddChildren(new FavoriteItem("Pigment", "https://pigment.shapefactory.co/", "Coding/Tools/Pigment", _profileInfo));
            coding.AddChildren(tools);

            var shopping = new FavoriteItem("Shopping", "Shopping");
            shopping.AddChildren(new FavoriteItem("Amazon", "https://www.amazon.com/", "Shopping/Amazon", _profileInfo));
            shopping.AddChildren(new FavoriteItem("eBay", "https://www.ebay.com/", "Shopping/eBay", _profileInfo));

            _root = new FavoriteItem("Favorites bar", string.Empty);
            _root.AddChildren(new FavoriteItem("YouTube", "https://www.youtube.com/", "YouTube", _profileInfo));
            _root.AddChildren(new FavoriteItem("Spotify", "https://open.spotify.com/", "Spotify", _profileInfo));
            _root.AddChildren(new FavoriteItem("LinkedIn", "https://www.linkedin.com/", "LinkedIn", _profileInfo));
            _root.AddChildren(coding);
            _root.AddChildren(shopping);
        }
    }
}
