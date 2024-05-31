// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed class Main : IPlugin, ISettingProvider, IContextMenu, IDisposable
    {
        public static string PluginID => "D73A7EF0633F4C82A14454FFD848F447";

        private const string SearchTree = nameof(SearchTree);
        private const string ExcludedProfiles = nameof(ExcludedProfiles);
        private const bool SearchTreeDefault = false;
        private readonly ProfileManager _profileManager;
        private readonly FavoriteQuery _favoriteQuery;
        private PluginInitContext? _context;
        private bool _searchTree;
        private ReadOnlyCollection<string> _excludedProfiles = ReadOnlyCollection<string>.Empty;
        private bool _disposed;

        public string Name => "Edge Favorite";

        public string Description => "Opens Microsoft Edge favorites";

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Key = SearchTree,
                Value = SearchTreeDefault,
                DisplayLabel = "Search as tree",
                DisplayDescription = "Navigate the folder tree when searching.",
            },
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.MultilineTextbox,
                Key = ExcludedProfiles,
                DisplayLabel = "Excluded profiles",
                DisplayDescription = "Prevents favorites from the specified profiles to be loaded. Add one profile per line.",
                PlaceholderText = "Example: Personal",
                TextValue = string.Empty,
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
            _profileManager.ReloadProfiles(_excludedProfiles);
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
                var emptyQuery = string.IsNullOrWhiteSpace(query.Search);

                foreach (var favorite in _favoriteQuery.GetAll())
                {
                    var score = StringMatcher.FuzzySearch(query.Search, favorite.Name);
                    if (emptyQuery || score.Score > 0)
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
            var oldExcludedProfiles = _excludedProfiles.ToArray();

            if (settings != null && settings.AdditionalOptions != null)
            {
                _searchTree = settings.AdditionalOptions.FirstOrDefault(x => x.Key == SearchTree)?.Value ?? SearchTreeDefault;
                _excludedProfiles = settings.AdditionalOptions.FirstOrDefault(x => x.Key == ExcludedProfiles)?.TextValueAsMultilineList.AsReadOnly() ?? ReadOnlyCollection<string>.Empty;
            }
            else
            {
                _searchTree = SearchTreeDefault;
                _excludedProfiles = ReadOnlyCollection<string>.Empty;
            }

            if (!oldExcludedProfiles.SequenceEqual(_excludedProfiles, StringComparer.OrdinalIgnoreCase))
            {
                _profileManager.ReloadProfiles(_excludedProfiles);
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

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _profileManager?.Dispose();
            _disposed = true;
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
