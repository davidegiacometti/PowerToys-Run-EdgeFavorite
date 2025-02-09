// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Properties;
using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;
using Wox.Infrastructure;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite
{
    public sealed partial class Main : IPlugin, IPluginI18n, ISettingProvider, IContextMenu, IDisposable
    {
        public static string PluginID => "D73A7EF0633F4C82A14454FFD848F447";

        public static string FolderIcoPath { get; private set; } = string.Empty;

        public static string UrlIcoPath { get; private set; } = string.Empty;

        private const string ExcludedProfiles = nameof(ExcludedProfiles);
        private const string EdgeChannel = nameof(EdgeChannel);
        private const string Mode = nameof(Mode);

        private readonly WoxLogger _logger;
        private readonly EdgeManager _edgeManager;
        private readonly ProfileManager _profileManager;
        private readonly FavoriteQuery _favoriteQuery;
        private PluginInitContext? _context;

        private ReadOnlyCollection<string> _excludedProfiles = ReadOnlyCollection<string>.Empty;
        private Channel _channel = Channel.Stable;
        private SearchMode _searchMode = SearchMode.Flat;

        private bool _disposed;

        private static readonly List<KeyValuePair<string, string>> _searchModes = new()
        {
            new KeyValuePair<string, string>(GetLocalizedSearchMode(SearchMode.Flat), SearchMode.Flat.ToString("D")),
            new KeyValuePair<string, string>(GetLocalizedSearchMode(SearchMode.FlatFavorites), SearchMode.FlatFavorites.ToString("D")),
            new KeyValuePair<string, string>(GetLocalizedSearchMode(SearchMode.Tree), SearchMode.Tree.ToString("D")),
        };

        private static readonly List<KeyValuePair<string, string>> _channels = new()
        {
            new KeyValuePair<string, string>(GetLocalizedChannel(Channel.Stable), Channel.Stable.ToString("D")),
            new KeyValuePair<string, string>(GetLocalizedChannel(Channel.Beta), Channel.Beta.ToString("D")),
            new KeyValuePair<string, string>(GetLocalizedChannel(Channel.Dev), Channel.Dev.ToString("D")),
            new KeyValuePair<string, string>(GetLocalizedChannel(Channel.Canary), Channel.Canary.ToString("D")),
        };

        public string Name => Resources.PluginName;

        public string Description => Resources.PluginDescription;

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => new List<PluginAdditionalOption>
        {
            new()
            {
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Combobox,
                Key = Mode,
                DisplayLabel = Resources.Option_SearchMode_Label,
                DisplayDescription = Resources.Option_SearchMode_Description,
                ComboBoxItems = _searchModes,
                ComboBoxValue = (int)SearchMode.Flat,
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
                ComboBoxValue = (int)Channel.Stable,
            },
        };

        public Main()
        {
            _logger = new WoxLogger();
            _edgeManager = new EdgeManager(_logger);
            _profileManager = new ProfileManager(_logger, _edgeManager);
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
                        Title = string.Format(Resources.MissingChannel_Title, GetLocalizedChannel(_channel)),
                        SubTitle = Resources.MissingChannel_Subtitle,
                        IcoPath = theme == Theme.Light || theme == Theme.HighContrastWhite
                            ? "Images/Warning.light.png"
                            : "Images/Warning.dark.png",
                    },
                };
            }

            var showProfileName = _profileManager.FavoriteProviders.Count > 1;

            if (_searchMode == SearchMode.Tree)
            {
                return _favoriteQuery
                    .Search(query.Search)
                    .OrderBy(f => f.Type)
                    .ThenBy(f => f.Name)
                    .Where(f => !f.IsEmptySpecialFolder)
                    .Select(f => f.ToResult(_context!.API, _edgeManager, query.ActionKeyword, showProfileName, true))
                    .ToList();
            }
            else
            {
                var results = new List<Result>();
                var emptyQuery = string.IsNullOrWhiteSpace(query.Search);

                foreach (var favorite in _favoriteQuery.GetAll().Where(f => !f.IsEmptySpecialFolder))
                {
                    if (favorite.Type == FavoriteType.Folder && _searchMode == SearchMode.FlatFavorites)
                    {
                        continue;
                    }

                    var score = StringMatcher.FuzzySearch(query.Search, favorite.Name);
                    if (emptyQuery || score.Score > 0)
                    {
                        var result = favorite.ToResult(_context!.API, _edgeManager, query.ActionKeyword, showProfileName, false);
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
                _searchMode = (SearchMode)(settings.AdditionalOptions.FirstOrDefault(x => x.Key == Mode)?.ComboBoxValue ?? (int)SearchMode.Flat);
                _excludedProfiles = settings.AdditionalOptions.FirstOrDefault(x => x.Key == ExcludedProfiles)?.TextValueAsMultilineList.AsReadOnly() ?? ReadOnlyCollection<string>.Empty;
                _channel = (Channel)(settings.AdditionalOptions.FirstOrDefault(x => x.Key == EdgeChannel)?.ComboBoxValue ?? (int)Channel.Stable);
            }
            else
            {
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
            return selectedResult.ContextData is FavoriteItem favorite
                ? favorite.ToContextMenuResults(_logger, _edgeManager)
                : new();
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
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                FolderIcoPath = "Images/Folder.light.png";
                UrlIcoPath = "Images/Url.light.png";
            }
            else
            {
                FolderIcoPath = "Images/Folder.dark.png";
                UrlIcoPath = "Images/Url.dark.png";
            }
        }

        private static string GetLocalizedChannel(Channel channel)
        {
            return channel switch
            {
                Channel.Stable => Resources.Channel_Stable,
                Channel.Beta => Resources.Channel_Beta,
                Channel.Dev => Resources.Channel_Dev,
                Channel.Canary => Resources.Channel_Canary,
                _ => string.Empty,
            };
        }

        private static string GetLocalizedSearchMode(SearchMode searchMode)
        {
            return searchMode switch
            {
                SearchMode.Flat => Resources.SearchMode_Flat,
                SearchMode.FlatFavorites => Resources.SearchMode_FlatFavorites,
                SearchMode.Tree => Resources.SearchMode_Tree,
                _ => string.Empty,
            };
        }
    }
}
