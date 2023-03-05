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
    public class Main : IPlugin, ISettingProvider
    {
        private const string SearchTree = nameof(SearchTree);
        private readonly IFavoriteProvider _favoriteProvider;
        private readonly IFavoriteQuery _favoriteQuery;
        private PluginInitContext? _context;
        private bool _searchTree;

        public string Name => "Edge Favorite";

        public string Description => "Open Microsoft Edge favorites.";

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new PluginAdditionalOption
            {
                Key = SearchTree,
                Value = true,
                DisplayLabel = "Search as tree",
                DisplayDescription = "Navigate the original directory tree when searching.",
            },
        };

        public Main()
        {
            _favoriteProvider = new FavoriteProvider();
            _favoriteQuery = new FavoriteQuery();
        }

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconsPath(_context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query)
        {
            var search = query.Search.Replace('\\', '/').Split('/');

            if (_searchTree)
            {
                return _favoriteQuery
                    .Search(_favoriteProvider.Root, search, 0)
                    .OrderBy(f => f.Type)
                    .ThenBy(f => f.Name)
                    .Select(f => f.Create())
                    .ToList();
            }
            else
            {
                var results = new List<Result>();

                foreach (var favorite in _favoriteQuery.GetAll(_favoriteProvider.Root))
                {
                    var score = StringMatcher.FuzzySearch(query.Search, favorite.Name);
                    if (string.IsNullOrWhiteSpace(query.Search) || score.Score > 0)
                    {
                        var result = favorite.Create();
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
            if (settings != null && settings.AdditionalOptions != null)
            {
                _searchTree = settings.AdditionalOptions.FirstOrDefault(x => x.Key == SearchTree)?.Value ?? true;
            }
            else
            {
                _searchTree = true;
            }
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
