// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services;
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
        public void Should_Get_All_Favorites()
        {
            var result = _singleFavoriteQuery.GetAll();
            Assert.AreEqual(14, result.Count());
        }

        [TestMethod]
        public void Should_Get_All_Favorites_From_All_Profiles()
        {
            var result = _multiFavoriteQuery.GetAll();
            Assert.AreEqual(19, result.Count());
        }

        [DataTestMethod]
        [DataRow("", 5)]
        [DataRow("/", 0)]
        [DataRow("S", 2)]
        [DataRow("Empty", 0)]
        [DataRow("Codi", 1)]
        [DataRow("Codi*", 1)]
        [DataRow("Codi/", 0)]
        [DataRow("Coding/", 5)]
        [DataRow("Coding/*", 5)]
        [DataRow("Coding/Tools", 1)]
        [DataRow("Coding/Tools/", 2)]
        [DataRow("coding/tools/j", 1)]
        [DataRow(@"coding\tools\j", 1)]
        [DataRow("coding/*blo", 2)]
        [DataRow("coding/*blo*", 2)]
        [DataRow("coding/blo*", 0)]
        [DataRow("*ing", 2)]
        [DataRow("*ing*", 2)]
        [DataRow("ing*", 0)]
        public void Should_Get_Expected_Result(string search, int expectedResult)
        {
            var result = _singleFavoriteQuery.Search(search);
            Assert.AreEqual(expectedResult, result.Count());
        }

        [TestMethod]
        public void Should_Get_Folder_Twice()
        {
            var result = _multiFavoriteQuery.Search("Codi");
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void Should_Merge_Folders_Content()
        {
            var result = _multiFavoriteQuery.Search("Coding/");
            Assert.AreEqual(8, result.Count());
        }
    }
}
