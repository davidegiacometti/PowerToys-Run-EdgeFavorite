// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests
{
    [TestClass]
    public class FavoriteQueryTest
    {
        private readonly IFavoriteProvider _favoriteProvider;

        public FavoriteQueryTest()
        {
            _favoriteProvider = new MockFavoriteProvider();
        }

        [TestMethod]
        public void Should_Get_All_Urls()
        {
            var sut = new FavoriteQuery();
            var result = sut.GetAll(_favoriteProvider.Root);
            Assert.AreEqual(result.Count(), 10);
        }

        [DataTestMethod]
        [DataRow("", 5)]
        [DataRow("/", 0)]
        [DataRow("S", 2)]
        [DataRow("Empty", 0)]
        [DataRow("Codi", 1)]
        [DataRow("Codi/", 0)]
        [DataRow("Coding/", 4)]
        [DataRow("Coding/Tools", 1)]
        [DataRow("Coding/Tools/", 2)]
        [DataRow("coding/tools/j", 1)]
        public void Should_Get_Expected_Result(string search, int expectedResult)
        {
            var sut = new FavoriteQuery();
            var result = sut.Search(_favoriteProvider.Root, search.Split('/'), 0);
            Assert.AreEqual(result.Count(), expectedResult);
        }
    }
}
