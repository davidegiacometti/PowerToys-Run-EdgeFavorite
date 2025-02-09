// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services
{
    public interface IFavoriteProvider
    {
        FavoriteItem Root { get; }
    }
}
