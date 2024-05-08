// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private static readonly string _pluginName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        private static string? _folderIcoPath;
        private static string? _urlIcoPath;
        private readonly List<FavoriteItem> _children = new();

        public string Name { get; }

        public string? Url { get; }

        public string Path { get; }

        public FavoriteType Type { get; }

        public ProfileInfo Profile { get; }

        public ReadOnlyCollection<FavoriteItem> Children => _children.AsReadOnly();

        public FavoriteItem(ProfileInfo profile)
        {
            Name = string.Empty;
            Path = string.Empty;
            Type = FavoriteType.Folder;
            Profile = profile;
        }

        public FavoriteItem(string name, string path, ProfileInfo profile)
        {
            Name = name;
            Path = path;
            Type = FavoriteType.Folder;
            Profile = profile;
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

        public Result CreateResult(IPublicAPI api, string actionKeyword, bool showProfileName, bool searchTree)
        {
            if (Type == FavoriteType.Folder)
            {
                var result = new Result
                {
                    Title = Name,
                    SubTitle = showProfileName ? $"Folder: {Path} - {Profile.Name}" : $"Folder: {Path}",
                    IcoPath = _folderIcoPath,
                    QueryTextDisplay = $"{Path}/",
                    ContextData = this,
                };

                if (searchTree)
                {
                    result.Action = _ =>
                    {
                        var newQuery = string.IsNullOrWhiteSpace(actionKeyword)
                            ? $"{Path}/"
                            : $"{actionKeyword} {Path}/";

                        api.ChangeQuery(newQuery, true);
                        return false;
                    };
                }

                return result;
            }
            else if (Type == FavoriteType.Url)
            {
                return new Result
                {
                    Title = Name,
                    SubTitle = showProfileName ? $"Favorite: {Path} - {Profile.Name}" : $"Favorite: {Path}",
                    IcoPath = _urlIcoPath,
                    QueryTextDisplay = searchTree ? Path : Name,
                    Action = _ =>
                    {
                        EdgeHelpers.OpenInEdge(this, false, false);
                        return true;
                    },
                    ToolTipData = new ToolTipData(Name, Url),
                    ContextData = this,
                };
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public List<ContextMenuResult> CreateContextMenuResult()
        {
            if (Type == FavoriteType.Folder)
            {
                var childFavorites = Children.Where(c => c.Type == FavoriteType.Url).ToArray();
                var childFavoritesCount = childFavorites.Length;

                if (childFavoritesCount > 0)
                {
                    return new()
                    {
                        new()
                        {
                            Title = $"Open all ({childFavoritesCount}) (Ctrl+O)",
                            Glyph = "\xE737",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            AcceleratorKey = Key.O,
                            AcceleratorModifiers = ModifierKeys.Control,
                            PluginName = _pluginName,
                            Action = _ => OpenAll(childFavorites, false, false),
                        },
                        new()
                        {
                            Title = $"Open all ({childFavoritesCount}) in new window (Ctrl+N)",
                            Glyph = "\xE8A7",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            AcceleratorKey = Key.N,
                            AcceleratorModifiers = ModifierKeys.Control,
                            PluginName = _pluginName,
                            Action = _ => OpenAll(childFavorites, false, true),
                        },
                        new()
                        {
                            Title = $"Open all ({childFavoritesCount}) in InPrivate window (Ctrl+P)",
                            Glyph = "\xE727",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            AcceleratorKey = Key.P,
                            AcceleratorModifiers = ModifierKeys.Control,
                            PluginName = _pluginName,
                            Action = _ => OpenAll(childFavorites, true, false),
                        },
                    };
                }
            }
            else if (Type == FavoriteType.Url)
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
                        Title = "Open in new window (Ctrl+N)",
                        Glyph = "\xE8A7",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.N,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            EdgeHelpers.OpenInEdge(this, false, true);
                            return true;
                        },
                    },
                    new()
                    {
                        Title = "Open in InPrivate window (Ctrl+P)",
                        Glyph = "\xE727",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.P,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            EdgeHelpers.OpenInEdge(this, true, false);
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

        private static bool OpenAll(FavoriteItem[] favorites, bool inPrivate, bool newWindow)
        {
            if (favorites.Length == 0)
            {
                throw new InvalidOperationException("Favorites cannot be empty.");
            }

            // If there is no need to open in a new window, starting multiple processes is preferred to avoid long command line arguments
            if (newWindow)
            {
                EdgeHelpers.OpenInEdge(favorites[0].Profile, string.Join(" ", favorites.Select(f => f.Url!)), inPrivate, newWindow);
            }
            else
            {
                foreach (var favorite in favorites)
                {
                    EdgeHelpers.OpenInEdge(favorite, inPrivate, false);
                }
            }

            return true;
        }
    }
}
