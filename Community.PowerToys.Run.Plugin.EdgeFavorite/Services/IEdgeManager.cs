// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Services
{
    public interface IEdgeManager
    {
        string UserDataPath { get; }

        bool ChannelDetected { get; }

        void Initialize(Channel channel);

        void Open(FavoriteItem favorite, bool inPrivate, bool newWindow);

        void Open(FavoriteItem[] favorites, bool inPrivate, bool newWindow);
    }
}
