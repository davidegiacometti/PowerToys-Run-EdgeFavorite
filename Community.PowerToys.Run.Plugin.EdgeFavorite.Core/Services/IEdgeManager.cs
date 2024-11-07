// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services
{
    public interface IEdgeManager
    {
        string UserDataPath { get; }

        bool ChannelDetected { get; }

        void Initialize(Channel channel);

        bool Open(FavoriteItem favorite, bool inPrivate, bool newWindow);

        bool Open(FavoriteItem[] favorites, bool inPrivate, bool newWindow);
    }
}
