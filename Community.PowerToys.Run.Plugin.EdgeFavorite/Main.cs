// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite
{
    public class Main : IPlugin, ISettingProvider, IContextMenu
    {
        public static string PluginID => "D73A7EF0633F4C82A14454FFD848F447";

        private const string SearchTree = nameof(SearchTree);
        private const string DefaultOnly = nameof(DefaultOnly);
        private const bool SearchTreeDefault = false;
        private const bool DefaultOnlyDefault = false;
        private readonly ProfileManager _profileManager;
        private readonly FavoriteQuery _favoriteQuery;
        private PluginInitContext? _context;
        private bool _searchTree;
        private bool _defaultOnly;

        public string Name => "Edge Favorite";

        public string Description => "Open Microsoft Edge favorites.";

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new()
            {
                Key = SearchTree,
                Value = SearchTreeDefault,
                DisplayLabel = "Search as tree",
                DisplayDescription = "Navigate the folder tree when searching.",
            },
            new()
            {
                Key = DefaultOnly,
                Value = DefaultOnlyDefault,
                DisplayLabel = "Default profile only",
                DisplayDescription = "Show favorites only from the default Microsoft Edge profile.",
            },
        };

        public Main()
        {
            _profileManager = new ProfileManager();
            _favoriteQuery = new FavoriteQuery(_profileManager);
        }

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconsPath(_context.API.GetCurrentTheme());
            _profileManager.ReloadProfiles(_searchTree);
        }

        public List<Result> Query(Query query)
        {
            var showProfileName = _profileManager.FavoriteProviders.Count > 1;

            if (_searchTree)
            {
                return _favoriteQuery
                    .Search(query.Search)
                    .OrderBy(f => f.Type)
                    .ThenBy(f => f.Name)
                    .Select(f => f.CreateResult(_context!.API, query.ActionKeyword, showProfileName, _searchTree))
                    .ToList();
            }
            else
            {
                var results = new List<Result>();

                foreach (var favorite in _favoriteQuery.GetAll())
                {
                    var score = StringMatcher.FuzzySearch(query.Search, favorite.Name);
                    if (string.IsNullOrWhiteSpace(query.Search) || score.Score > 0)
                    {
                        var result = favorite.CreateResult(_context!.API, query.ActionKeyword, showProfileName, _searchTree);
                        result.Score = score.Score;
                        result.TitleHighlightData = score.MatchData;
                        results.Add(result);
                    }
                }

                return results.OrderBy(r => r.Title).ToList();
            }
        }

        public Control CreateSettingPanel()
        {
            throw new NotImplementedException();
        }

        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            var oldDefaultOnly = _defaultOnly;

            if (settings != null && settings.AdditionalOptions != null)
            {
                _searchTree = settings.AdditionalOptions.FirstOrDefault(x => x.Key == SearchTree)?.Value ?? SearchTreeDefault;
                _defaultOnly = settings.AdditionalOptions.FirstOrDefault(x => x.Key == DefaultOnly)?.Value ?? DefaultOnlyDefault;
            }
            else
            {
                _searchTree = SearchTreeDefault;
                _defaultOnly = DefaultOnlyDefault;
            }

            if (oldDefaultOnly != _defaultOnly)
            {
                _profileManager.ReloadProfiles(_defaultOnly);
            }
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is not FavoriteItem favorite)
            {
                return new();
            }

            return favorite.CreateContextMenuResult();
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconsPath(newTheme);
        }

        private static void UpdateIconsPath(Theme theme)
        {
            FavoriteItem.SetIcons(theme);
        }
    }
}
