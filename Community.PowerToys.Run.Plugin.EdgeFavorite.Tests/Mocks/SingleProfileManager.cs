// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Tests.Mocks
{
    public class SingleProfileManager : IProfileManager
    {
        public ReadOnlyCollection<IFavoriteProvider> FavoriteProviders => (new IFavoriteProvider[] { new DefaultFavoriteProvider() }).AsReadOnly();

        public void ReloadProfiles(IEnumerable<string> excluded)
        {
        }
    }
}
