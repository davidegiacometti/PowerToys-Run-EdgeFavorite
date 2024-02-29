// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using ManagedCommon;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Models
{
    public class FavoriteItem
    {
        private static readonly ProfileInfo _emptyProfile = new(string.Empty, string.Empty);
        private static readonly string _pluginName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        private static string? _folderIcoPath;
        private static string? _urlIcoPath;
        private readonly List<FavoriteItem> _childrens = new();

        public string Name { get; }

        public string? Url { get; }

        public string Path { get; }

        public FavoriteType Type { get; }

        public ProfileInfo Profile { get; }

        public ReadOnlyCollection<FavoriteItem> Childrens => _childrens.AsReadOnly();

        public FavoriteItem()
        {
            Name = string.Empty;
            Path = string.Empty;
            Type = FavoriteType.Folder;
            Profile = _emptyProfile; // Folders are profile agnostic
        }

        public FavoriteItem(string name, string path)
        {
            Name = name;
            Path = path;
            Type = FavoriteType.Folder;
            Profile = _emptyProfile; // Folders are profile agnostic
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
            _childrens.Add(item);
        }

        public Result CreateResult(IPublicAPI api, string actionKeyword, bool showProfileName, bool searchTree)
        {
            return Type switch
            {
                FavoriteType.Folder => new Result
                {
                    Title = Name,
                    SubTitle = $"Folder: {Path}",
                    IcoPath = _folderIcoPath,
                    QueryTextDisplay = $"{Path}/",
                    ContextData = this,
                    Action = _ =>
                    {
                        var newQuery = string.IsNullOrWhiteSpace(actionKeyword)
                            ? $"{Path}/"
                            : $"{actionKeyword} {Path}/";

                        api.ChangeQuery(newQuery, true);
                        return false;
                    },
                },
                FavoriteType.Url => new Result
                {
                    Title = Name,
                    SubTitle = showProfileName ? $"Favorite: {Path} - {Profile.Name}" : $"Favorite: {Path}",
                    IcoPath = _urlIcoPath,
                    QueryTextDisplay = searchTree ? Path : Name,
                    Action = _ =>
                    {
                        EdgeHelpers.OpenInEdge(this, false);
                        return true;
                    },
                    ToolTipData = new ToolTipData(Name, Url),
                    ContextData = this,
                },
                _ => throw new ArgumentException(),
            };
        }

        public List<ContextMenuResult> CreateContextMenuResult()
        {
            if (Type == FavoriteType.Url)
            {
                return new()
                {
                    new()
                    {
                        Title = "Copy URL (Ctrl+C)",
                        Glyph = "\xE8C8",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            try
                            {
                                Clipboard.SetText(Url);
                            }
                            catch (Exception ex)
                            {
                                Log.Exception("Failed to copy URL to clipboard", ex, typeof(FavoriteItem));
                            }

                            return true;
                        },
                    },
                    new()
                    {
                        Title = "Open InPrivate (Ctrl+P)",
                        Glyph = "\xE727",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.P,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            EdgeHelpers.OpenInEdge(this, true);
                            return true;
                        },
                    },
                };
            }

            return new();
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
