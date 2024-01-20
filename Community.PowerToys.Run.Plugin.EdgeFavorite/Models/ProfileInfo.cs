// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Models
{
    public class ProfileInfo
    {
        public string Name { get; set; }

        public string Directory { get; private set; }

        public ProfileInfo(string name, string directory)
        {
            Name = name;
            Directory = directory;
        }
    }
}
