// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public interface IFavoriteProvider
    {
        FavoriteItem Root { get; }
    }
}
