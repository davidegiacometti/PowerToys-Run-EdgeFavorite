// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public interface IProfileManager
    {
        ReadOnlyCollection<IFavoriteProvider> FavoriteProviders { get; }

        void ReloadProfiles(bool all);
    }
}
