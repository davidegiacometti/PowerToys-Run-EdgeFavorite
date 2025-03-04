﻿// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests.Mocks
{
    public class MockEdgeManager : IEdgeManager
    {
        public string UserDataPath => string.Empty;

        public bool ChannelDetected => false;

        public void Initialize(Channel channel)
        {
        }

        public bool Open(FavoriteItem favorite, bool inPrivate, bool newWindow)
        {
            return true;
        }

        public bool Open(FavoriteItem[] favorites, bool inPrivate, bool newWindow)
        {
            return true;
        }
    }
}
