// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public sealed partial class ProfileManager : IProfileManager, IDisposable
    {
        private readonly EdgeManager _edgeManager;
        private readonly List<IFavoriteProvider> _favoriteProviders = new();
        private bool _disposed;

        public ReadOnlyCollection<IFavoriteProvider> FavoriteProviders => _favoriteProviders.AsReadOnly();

        public ProfileManager(EdgeManager edgeManager)
        {
            _edgeManager = edgeManager;
        }

        public void ReloadProfiles(IEnumerable<string> excluded)
        {
            var userDataPath = _edgeManager.UserDataPath;

            if (!Path.Exists(userDataPath))
            {
                Log.Error($"User data {userDataPath} is not a valid path.", typeof(ProfileManager));
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
                _favoriteProviders.Add(new FavoriteProvider(path, profile));
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

        private static string? GetProfileName(string directoryPath)
        {
            try
            {
                var preferencesPath = Path.Combine(directoryPath, "Preferences");
                if (!File.Exists(preferencesPath))
                {
                    Log.Error($"Failed to read profile name: {preferencesPath} files not found.", typeof(ProfileManager));
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
                    Log.Error("Failed to read profile name: name property is not a string.", typeof(ProfileManager));
                    return null;
                }

                var name = nameElement.GetString();
                if (string.IsNullOrWhiteSpace(name))
                {
                    Log.Error("Failed to read profile name: name property is empty.", typeof(ProfileManager));
                    return null;
                }

                return name;
            }
            catch (Exception ex)
            {
                Log.Exception("Failed to read profile name", ex, typeof(ProfileManager));
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
