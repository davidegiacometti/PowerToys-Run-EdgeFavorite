// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models
{
    public class FavoriteItem
    {
        private readonly List<FavoriteItem> _children = new();
        private readonly bool _special;

        public string Name { get; }

        public string? Url { get; }

        public string Path { get; }

        public FavoriteType Type { get; }

        public ProfileInfo Profile { get; }

        public ReadOnlyCollection<FavoriteItem> Children => _children.AsReadOnly();

        public bool IsEmptySpecialFolder => _special && Children.Count == 0;

        public FavoriteItem(ProfileInfo profile)
        {
            Name = string.Empty;
            Path = string.Empty;
            Type = FavoriteType.Folder;
            Profile = profile;
        }

        public FavoriteItem(string name, string path, ProfileInfo profile, bool special)
        {
            Name = name;
            Path = path;
            Type = FavoriteType.Folder;
            Profile = profile;
            _special = special;
        }

        public FavoriteItem(string name, string url, string path, ProfileInfo profile)
        {
            Name = name;
            Url = url;
            Path = path;
            Type = FavoriteType.Url;
            Profile = profile;
        }

        public void AddChildren(FavoriteItem item)
        {
            _children.Add(item);
        }
    }
}
