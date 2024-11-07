// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Properties;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite
{
    public static class FavoriteItemExtensions
    {
        private static readonly string _pluginName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;

        public static Result ToResult(this FavoriteItem item, IPublicAPI api, IEdgeManager edgeManager, string actionKeyword, bool showProfileName, bool searchTree)
        {
            if (item.Type == FavoriteType.Folder)
            {
                var result = new Result
                {
                    Title = item.Name,
                    SubTitle = showProfileName
                        ? string.Format(Resources.FolderResult_Profile_Subtitle, item.Path, item.Profile.Name)
                        : string.Format(Resources.FolderResult_Subtitle, item.Path),
                    IcoPath = Main.FolderIcoPath,
                    QueryTextDisplay = $"{item.Path}/",
                    ContextData = item,
                };

                if (searchTree)
                {
                    result.Action = _ =>
                    {
                        var newQuery = string.IsNullOrWhiteSpace(actionKeyword)
                            ? $"{item.Path}/"
                            : $"{actionKeyword} {item.Path}/";

                        api.ChangeQuery(newQuery, true);
                        return false;
                    };
                }

                return result;
            }
            else if (item.Type == FavoriteType.Url)
            {
                return new Result
                {
                    Title = item.Name,
                    SubTitle = showProfileName
                        ? string.Format(Resources.FavoriteResult_Profile_Subtitle, item.Path, item.Profile.Name)
                        : string.Format(Resources.FavoriteResult_Subtitle, item.Path),
                    IcoPath = Main.UrlIcoPath,
                    QueryTextDisplay = searchTree ? item.Path : item.Name,
                    Action = _ =>
                    {
                        edgeManager.Open(item, false, false);
                        return true;
                    },
                    ToolTipData = new ToolTipData(item.Name, item.Url),
                    ContextData = item,
                };
            }

            throw new ArgumentException(null, nameof(item));
        }

        public static List<ContextMenuResult> ToContextMenuResults(this FavoriteItem item, ILogger logger, IEdgeManager edgeManager)
        {
            if (item.Type == FavoriteType.Folder)
            {
                var childFavorites = item.Children.Where(c => c.Type == FavoriteType.Url).ToArray();
                var childFavoritesCount = childFavorites.Length;

                if (childFavoritesCount > 0)
                {
                    return new()
                    {
                        new()
                        {
                            Title = string.Format(Resources.Action_OpenAll, childFavoritesCount),
                            Glyph = "\xE737",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            AcceleratorKey = Key.O,
                            AcceleratorModifiers = ModifierKeys.Control,
                            PluginName = _pluginName,
                            Action = _ =>
                            {
                                edgeManager.Open(childFavorites, false, false);
                                return true;
                            },
                        },
                        new()
                        {
                            Title = string.Format(Resources.Action_OpenAllWindow, childFavoritesCount),
                            Glyph = "\xE8A7",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            AcceleratorKey = Key.N,
                            AcceleratorModifiers = ModifierKeys.Control,
                            PluginName = _pluginName,
                            Action = _ =>
                            {
                                edgeManager.Open(childFavorites, false, true);
                                return true;
                            },
                        },
                        new()
                        {
                            Title = string.Format(Resources.Action_OpenAllPrivate, childFavoritesCount),
                            Glyph = "\xE727",
                            FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                            AcceleratorKey = Key.P,
                            AcceleratorModifiers = ModifierKeys.Control,
                            PluginName = _pluginName,
                            Action = _ =>
                            {
                                edgeManager.Open(childFavorites, true, false);
                                return true;
                            },
                        },
                    };
                }
            }
            else if (item.Type == FavoriteType.Url)
            {
                return new()
                {
                    new()
                    {
                        Title = Resources.Action_CopyUrl,
                        Glyph = "\xE8C8",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            try
                            {
                                Clipboard.SetText(item.Url);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, "Failed to copy URL to clipboard", typeof(FavoriteItemExtensions));
                            }

                            return true;
                        },
                    },
                    new()
                    {
                        Title = Resources.Action_OpenWindow,
                        Glyph = "\xE8A7",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.N,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            edgeManager.Open(item, false, true);
                            return true;
                        },
                    },
                    new()
                    {
                        Title = Resources.Action_OpenPrivate,
                        Glyph = "\xE727",
                        FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                        AcceleratorKey = Key.P,
                        AcceleratorModifiers = ModifierKeys.Control,
                        PluginName = _pluginName,
                        Action = _ =>
                        {
                            edgeManager.Open(item, true, false);
                            return true;
                        },
                    },
                };
            }

            throw new ArgumentException(null, nameof(item));
        }
    }
}
