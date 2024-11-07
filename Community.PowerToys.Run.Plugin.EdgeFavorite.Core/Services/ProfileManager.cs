// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Core.Services
{
    public sealed partial class ProfileManager : IProfileManager, IDisposable
    {
        private readonly ILogger _logger;
        private readonly EdgeManager _edgeManager;
        private readonly List<IFavoriteProvider> _favoriteProviders = new();
        private bool _disposed;

        public ReadOnlyCollection<IFavoriteProvider> FavoriteProviders => _favoriteProviders.AsReadOnly();

        public ProfileManager(ILogger logger, EdgeManager edgeManager)
        {
            _logger = logger;
            _edgeManager = edgeManager;
        }

        public void ReloadProfiles(IEnumerable<string> excluded)
        {
            var userDataPath = _edgeManager.UserDataPath;

            if (!Path.Exists(userDataPath))
            {
                _logger.LogError($"User data {userDataPath} is not a valid path.", typeof(ProfileManager));
                return;
            }

            if (_favoriteProviders.Count > 0)
            {
                DisposeFavoriteProviders();
                _favoriteProviders.Clear();
            }

            foreach (var path in Directory.GetFiles(userDataPath, "Bookmarks", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 2 }))
            {
                var directory = Directory.GetParent(path);

                if (directory == null)
                {
                    continue;
                }

                // Guest profile doesn't allow favorites
                if (directory.Name.Equals("Guest Profile", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = GetProfileName(directory.FullName) ?? directory.Name;

                if (excluded.Any(e => e.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var profile = new ProfileInfo(name, directory.Name);
                _favoriteProviders.Add(new FavoriteProvider(_logger, path, profile));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            DisposeFavoriteProviders();
            _disposed = true;
        }

        private string? GetProfileName(string directoryPath)
        {
            try
            {
                var preferencesPath = Path.Combine(directoryPath, "Preferences");
                if (!File.Exists(preferencesPath))
                {
                    _logger.LogError($"Failed to read profile name: {preferencesPath} files not found.", typeof(ProfileManager));
                    return null;
                }

                using var fs = new FileStream(preferencesPath, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                string json = sr.ReadToEnd();
                var parsed = JsonDocument.Parse(json);
                parsed.RootElement.TryGetProperty("profile", out var profileElement);
                profileElement.TryGetProperty("name", out var nameElement);
                if (nameElement.ValueKind != JsonValueKind.String)
                {
                    _logger.LogError("Failed to read profile name: name property is not a string.", typeof(ProfileManager));
                    return null;
                }

                var name = nameElement.GetString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogError("Failed to read profile name: name property is empty.", typeof(ProfileManager));
                    return null;
                }

                return name;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read profile name", typeof(ProfileManager));
                return null;
            }
        }

        private void DisposeFavoriteProviders()
        {
            foreach (var provider in _favoriteProviders)
            {
                if (provider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                }
            }
        }
    }
}
