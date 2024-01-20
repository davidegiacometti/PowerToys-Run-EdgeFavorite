// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests
{
    [TestClass]
    public class FavoriteQueryTest
    {
        private readonly IProfileManager _singleProfileManager;
        private readonly FavoriteQuery _singleFavoriteQuery;

        private readonly IProfileManager _multiProfileManager;
        private readonly FavoriteQuery _multiFavoriteQuery;

        public FavoriteQueryTest()
        {
            _singleProfileManager = new SingleProfileManager();
            _singleFavoriteQuery = new FavoriteQuery(_singleProfileManager);

            _multiProfileManager = new MultiProfileManager();
            _multiFavoriteQuery = new FavoriteQuery(_multiProfileManager);
        }

        [TestMethod]
        public void Should_Get_All_Urls()
        {
            var result = _singleFavoriteQuery.GetAll();
            Assert.AreEqual(result.Count(), 10);
        }

        [TestMethod]
        public void Should_Get_All_Urls_From_All_Profiles()
        {
            var result = _multiFavoriteQuery.GetAll();
            Assert.AreEqual(result.Count(), 14);
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
        [DataRow("coding\\tools\\j", 1)]
        public void Should_Get_Expected_Result(string search, int expectedResult)
        {
            var result = _singleFavoriteQuery.Search(search);
            Assert.AreEqual(result.Count(), expectedResult);
        }

        [TestMethod]
        public void Should_Get_Single_Folder()
        {
            var result = _multiFavoriteQuery.Search("Codi");
            Assert.AreEqual(result.Count(), 1);
        }

        [TestMethod]
        public void Should_Merge_Folders_Content()
        {
            var result = _multiFavoriteQuery.Search("Coding/");
            Assert.AreEqual(result.Count(), 7);
        }
    }
}
