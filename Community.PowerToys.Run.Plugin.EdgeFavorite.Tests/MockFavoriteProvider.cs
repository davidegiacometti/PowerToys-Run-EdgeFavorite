// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests
{
    public class MockFavoriteProvider : IFavoriteProvider
    {
        private readonly FavoriteItem _root;

        public FavoriteItem Root => _root;

        public MockFavoriteProvider()
        {
            var coding = new FavoriteItem("Coding", null, "Coding", FavoriteType.Folder);
            coding.AddChildren(new FavoriteItem("GitHub", "https://github.com/", "Coding/GitHub", FavoriteType.Url));
            coding.AddChildren(new FavoriteItem("Microsoft Azure", "https://portal.azure.com/", "Coding/Microsoft Azure", FavoriteType.Url));
            coding.AddChildren(new FavoriteItem("Microsoft Developer Blogs", "https://devblogs.microsoft.com/", "Coding/Microsoft Developer Blogs", FavoriteType.Url));

            var tools = new FavoriteItem("Tools", null, "Coding", FavoriteType.Folder);
            tools.AddChildren(new FavoriteItem("JWT", "https://jwt.io/", "Coding/Tools/JWT", FavoriteType.Url));
            tools.AddChildren(new FavoriteItem("Pigment", "https://pigment.shapefactory.co/", "Coding/Tools/Pigment", FavoriteType.Url));
            coding.AddChildren(tools);

            var shopping = new FavoriteItem("Shopping", null, "Shopping", FavoriteType.Folder);
            shopping.AddChildren(new FavoriteItem("Amazon", "https://www.amazon.com/", "Shopping/Amazon", FavoriteType.Url));
            shopping.AddChildren(new FavoriteItem("eBay", "https://www.ebay.com/", "Shopping/eBay", FavoriteType.Url));

            _root = new FavoriteItem("Favorites bar", null, string.Empty, FavoriteType.Folder);
            _root.AddChildren(new FavoriteItem("YouTube", "https://www.youtube.com/", "YouTube", FavoriteType.Url));
            _root.AddChildren(new FavoriteItem("Spotify", "https://open.spotify.com/", "Spotify", FavoriteType.Url));
            _root.AddChildren(new FavoriteItem("LinkedIn", "https://www.linkedin.com/", "LinkedIn", FavoriteType.Url));
            _root.AddChildren(coding);
            _root.AddChildren(shopping);
        }
    }
}
