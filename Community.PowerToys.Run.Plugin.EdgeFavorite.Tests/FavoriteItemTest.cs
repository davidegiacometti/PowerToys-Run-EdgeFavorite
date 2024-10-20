// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Globalization;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests
{
    [TestClass]
    public class FavoriteItemTest
    {
        private readonly PluginInitContext _context;
        private readonly MockEdgeManager _edgeManager;
        private readonly ProfileInfo _profileInfo;

        public FavoriteItemTest()
        {
            _context = new PluginInitContext();
            _edgeManager = new MockEdgeManager();
            _profileInfo = new ProfileInfo("Default", "Default");
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
        }

        [DataTestMethod]
        [DataRow("DevOps", "Coding/DevOps", true, "Coding/DevOps/")]
        [DataRow("DevOps", "Coding/DevOps", false, "Coding/DevOps/")]
        public void Assert_Folder_Item_Result_QueryTextDisplay(string name, string path, bool searchTree, string expectedQueryTextDisplay)
        {
            var item = new FavoriteItem(name, path, _profileInfo, false);
            var result = item.CreateResult(_context.API, _edgeManager, string.Empty, false, searchTree);
            Assert.AreEqual(result.QueryTextDisplay, expectedQueryTextDisplay);
        }

        [DataTestMethod]
        [DataRow("GitHub", "https://github.com/", "Coding/GitHub", true, "Coding/GitHub")]
        [DataRow("GitHub", "https://github.com/", "Coding/GitHub", false, "GitHub")]
        public void Assert_Url_Item_Result_QueryTextDisplay(string name, string url, string path, bool searchTree, string expectedQueryTextDisplay)
        {
            var item = new FavoriteItem(name, url, path, _profileInfo);
            var result = item.CreateResult(_context.API, _edgeManager, string.Empty, false, searchTree);
            Assert.AreEqual(result.QueryTextDisplay, expectedQueryTextDisplay);
        }

        [DataTestMethod]
        [DataRow("GitHub", "https://github.com/", "Coding/GitHub", true, "Favorite: Coding/GitHub - Default")]
        [DataRow("GitHub", "https://github.com/", "Coding/GitHub", false, "Favorite: Coding/GitHub")]
        public void Assert_Url_Item_Result_SubTitle(string name, string url, string path, bool showProfileName, string expectedSubTitle)
        {
            var item = new FavoriteItem(name, url, path, _profileInfo);
            var result = item.CreateResult(_context.API, _edgeManager, string.Empty, showProfileName, false);
            Assert.AreEqual(result.SubTitle, expectedSubTitle);
        }

        [DataTestMethod]
        [DataRow("GitHub", "Coding/GitHub", true, "Folder: Coding/GitHub - Default")]
        [DataRow("GitHub", "Coding/GitHub", false, "Folder: Coding/GitHub")]
        public void Assert_Folder_Item_Result_SubTitle(string name, string path, bool showProfileName, string expectedSubTitle)
        {
            var item = new FavoriteItem(name, path, _profileInfo, false);
            var result = item.CreateResult(_context.API, _edgeManager, string.Empty, showProfileName, false);
            Assert.AreEqual(result.SubTitle, expectedSubTitle);
        }
    }
}
