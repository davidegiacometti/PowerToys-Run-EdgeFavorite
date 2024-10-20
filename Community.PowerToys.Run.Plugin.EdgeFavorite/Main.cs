// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Properties;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Services;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite
{
    public sealed partial class Main : IPlugin, IPluginI18n, ISettingProvider, IContextMenu, IDisposable
    {
        public static string PluginID => "D73A7EF0633F4C82A14454FFD848F447";

        private const string SearchTree = nameof(SearchTree);
        private const string ExcludedProfiles = nameof(ExcludedProfiles);
        private const string EdgeChannel = nameof(EdgeChannel);
        private const bool SearchTreeDefault = false;
        private const int DefaultChannel = 0;

        private readonly EdgeManager _edgeManager;
        private readonly ProfileManager _profileManager;
        private readonly FavoriteQuery _favoriteQuery;
        private PluginInitContext? _context;

        private bool _searchTree;
        private ReadOnlyCollection<string> _excludedProfiles = ReadOnlyCollection<string>.Empty;
        private Channel _channel = 0;

        private bool _disposed;

        private static readonly List<KeyValuePair<string, string>> _channels = new()
        {
            new KeyValuePair<string, string>(Channel.Stable.ToLocalizedString(), Channel.Stable.ToString("D")),
            new KeyValuePair<string, string>(Channel.Beta.ToLocalizedString(), Channel.Beta.ToString("D")),
            new KeyValuePair<string, string>(Channel.Dev.ToLocalizedString(), Channel.Dev.ToString("D")),
            new KeyValuePair<string, string>(Channel.Canary.ToLocalizedString(), Channel.Canary.ToString("D")),
        };

        public string Name => Resources.PluginName;

        public string Description => Resources.PluginDescription;

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Key = SearchTree,
                Value = SearchTreeDefault,
                DisplayLabel = Resources.Option_SearchTree_Label,
                DisplayDescription = Resources.Option_SearchTree_Description,
            },
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.MultilineTextbox,
                Key = ExcludedProfiles,
                DisplayLabel = Resources.Option_ExcludedProfiles_Label,
                DisplayDescription = Resources.Option_ExcludedProfiles_Description,
                PlaceholderText = Resources.Option_ExcludedProfiles_Placeholder,
                TextValue = string.Empty,
            },
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Combobox,
                Key = EdgeChannel,
                DisplayLabel = Resources.Option_Channel_Label,
                DisplayDescription = Resources.Option_Channel_Description,
                ComboBoxItems = _channels,
                ComboBoxValue = DefaultChannel,
            },
        };

        public Main()
        {
            _edgeManager = new EdgeManager();
            _profileManager = new ProfileManager(_edgeManager);
            _favoriteQuery = new FavoriteQuery(_profileManager);
        }

        public string GetTranslatedPluginTitle() => Resources.PluginName;

        public string GetTranslatedPluginDescription() => Resources.PluginDescription;

        public void Init(PluginInitContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconsPath(_context.API.GetCurrentTheme());

            _edgeManager.Initialize(_channel);
            _profileManager.ReloadProfiles(_excludedProfiles);
        }

        public List<Result> Query(Query query)
        {
            if (!string.IsNullOrWhiteSpace(query.ActionKeyword) && !_edgeManager.ChannelDetected)
            {
                var theme = _context!.API.GetCurrentTheme();

                return new List<Result>
                {
                    new Result
                    {
                        Title = string.Format(Resources.MissingChannel_Title, _channel.ToLocalizedString()),
                        SubTitle = Resources.MissingChannel_Subtitle,
                        IcoPath = theme == Theme.Light || theme == Theme.HighContrastWhite
                            ? "Images/Warning.light.png"
                            : "Images/Warning.dark.png",
                    },
                };
            }

            var showProfileName = _profileManager.FavoriteProviders.Count > 1;

            if (_searchTree)
            {
                return _favoriteQuery
                    .Search(query.Search)
                    .OrderBy(f => f.Type)
                    .ThenBy(f => f.Name)
                    .Where(f => !f.IsEmptySpecialFolder)
                    .Select(f => f.CreateResult(_context!.API, _edgeManager, query.ActionKeyword, showProfileName, _searchTree))
                    .ToList();
            }
            else
            {
                var results = new List<Result>();
                var emptyQuery = string.IsNullOrWhiteSpace(query.Search);

                foreach (var favorite in _favoriteQuery.GetAll().Where(f => !f.IsEmptySpecialFolder))
                {
                    var score = StringMatcher.FuzzySearch(query.Search, favorite.Name);
                    if (emptyQuery || score.Score > 0)
                    {
                        var result = favorite.CreateResult(_context!.API, _edgeManager, query.ActionKeyword, showProfileName, _searchTree);
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
            var oldChannel = _channel;

            if (settings != null && settings.AdditionalOptions != null)
            {
                _searchTree = settings.AdditionalOptions.FirstOrDefault(x => x.Key == SearchTree)?.Value ?? SearchTreeDefault;
                _excludedProfiles = settings.AdditionalOptions.FirstOrDefault(x => x.Key == ExcludedProfiles)?.TextValueAsMultilineList.AsReadOnly() ?? ReadOnlyCollection<string>.Empty;
                _channel = (Channel)(settings.AdditionalOptions.FirstOrDefault(x => x.Key == EdgeChannel)?.ComboBoxValue ?? DefaultChannel);
            }
            else
            {
                _searchTree = SearchTreeDefault;
                _excludedProfiles = ReadOnlyCollection<string>.Empty;
            }

            if (oldChannel != _channel)
            {
                _edgeManager.Initialize(_channel);
            }

            if (oldChannel != _channel || !oldExcludedProfiles.SequenceEqual(_excludedProfiles, StringComparer.OrdinalIgnoreCase))
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

            return favorite.CreateContextMenuResult(_edgeManager);
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
