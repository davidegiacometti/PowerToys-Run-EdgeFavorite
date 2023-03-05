// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ManagedCommon;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Models
{
    public class FavoriteItem
    {
        private static string? _folderIcoPath;
        private static string? _urlIcoPath;
        private readonly List<FavoriteItem> _childrens = new();

        public string Name { get; }

        public string? Url { get; }

        public string Path { get; }

        public FavoriteType Type { get; }

        public ReadOnlyCollection<FavoriteItem> Childrens => _childrens.AsReadOnly();

        public FavoriteItem()
        {
            Name = string.Empty;
            Path = string.Empty;
            Type = FavoriteType.Folder;
        }

        public FavoriteItem(string name, string? url, string path, FavoriteType type)
        {
            Name = name;
            Url = url;
            Path = path;
            Type = type;
        }

        public void AddChildren(FavoriteItem item)
        {
            _childrens.Add(item);
        }

        public Result Create()
        {
            return Type switch
            {
                FavoriteType.Folder => new Result
                {
                    Title = Name,
                    SubTitle = $"Folder: {Path}",
                    IcoPath = _folderIcoPath,
                    QueryTextDisplay = Path,
                },
                FavoriteType.Url => new Result
                {
                    Title = Name,
                    SubTitle = $"Favorite: {Path}",
                    IcoPath = _urlIcoPath,
                    QueryTextDisplay = Path,
                    Action = _ =>
                    {
                        Wox.Infrastructure.Helper.OpenInShell($"microsoft-edge:{Url}");
                        return true;
                    },
                    ToolTipData = new ToolTipData(Name, Url),
                },
                _ => throw new ArgumentException(),
            };
        }

        public static void SetIcons(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _folderIcoPath = "Images/Folder.light.png";
                _urlIcoPath = "Images/Url.light.png";
            }
            else
            {
                _folderIcoPath = "Images/Folder.dark.png";
                _urlIcoPath = "Images/Url.dark.png";
            }
        }
    }
}
