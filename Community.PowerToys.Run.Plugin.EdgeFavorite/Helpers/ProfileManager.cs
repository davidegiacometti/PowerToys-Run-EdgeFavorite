// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public class ProfileManager : IProfileManager
    {
        private static readonly string _userDataPath = Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Microsoft\Edge\User Data");
        private readonly List<IFavoriteProvider> _favoriteProviders = new();

        public ReadOnlyCollection<IFavoriteProvider> FavoriteProviders => _favoriteProviders.AsReadOnly();

        public ProfileManager()
        {
        }

        public void ReloadProfiles(bool defaultOnly)
        {
            if (_favoriteProviders.Count > 0)
            {
                _favoriteProviders.Clear();
            }

            foreach (var path in Directory.GetFiles(_userDataPath, "Bookmarks", new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = 2 }))
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

                if (defaultOnly && !directory.Name.Equals("Default", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var profile = new ProfileInfo(directory.Name, directory.Name);
                TrySetProfileName(profile, directory.FullName);
                _favoriteProviders.Add(new FavoriteProvider(path, profile));
            }
        }

        private static void TrySetProfileName(ProfileInfo profileInfo, string directoryPath)
        {
            _ = Task.Run(() =>
            {
                try
                {
                    var preferencesPath = Path.Combine(directoryPath, "Preferences");
                    if (!File.Exists(preferencesPath))
                    {
                        Log.Error($"Failed to set profile name: {preferencesPath} files not found.", typeof(ProfileManager));
                        return;
                    }

                    using var fs = new FileStream(preferencesPath, FileMode.Open, FileAccess.Read);
                    using var sr = new StreamReader(fs);
                    string json = sr.ReadToEnd();
                    var parsed = JsonDocument.Parse(json);
                    parsed.RootElement.TryGetProperty("profile", out var profileElement);
                    profileElement.TryGetProperty("name", out var nameElement);
                    if (nameElement.ValueKind != JsonValueKind.String)
                    {
                        Log.Error("Failed to set profile name: name property is not a string.", typeof(ProfileManager));
                        return;
                    }

                    var name = nameElement.GetString();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Log.Error("Failed to set profile name: name property is empty.", typeof(ProfileManager));
                        return;
                    }

                    profileInfo.Name = name;
                }
                catch (Exception ex)
                {
                    Log.Exception("Failed to set profile name", ex, typeof(ProfileManager));
                }
            });
        }
    }
}
