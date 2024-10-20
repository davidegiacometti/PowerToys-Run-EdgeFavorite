// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Services
{
    public sealed class FavoriteProvider : IFavoriteProvider, IDisposable
    {
        private readonly string _path;
        private readonly FileSystemWatcher _watcher;
        private FavoriteItem _root;
        private bool _disposed;

        public FavoriteItem Root => _root;

        public ProfileInfo ProfileInfo { get; }

        public FavoriteProvider(string path, ProfileInfo profileInfo)
        {
            _path = path;
            ProfileInfo = profileInfo;
            _root = new FavoriteItem(profileInfo);
            InitFavorites();

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_path)!,
                Filter = Path.GetFileName(_path),
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
            };

            _watcher.Changed += (s, e) => InitFavorites();
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _watcher?.Dispose();
            _disposed = true;
        }

        private void InitFavorites()
        {
            try
            {
                if (!Path.Exists(_path))
                {
                    Log.Warn($"Failed to find Bookmarks file: {_path}", typeof(FavoriteProvider));
                    return;
                }

                using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);
                string json = sr.ReadToEnd();
                var parsed = JsonDocument.Parse(json);
                parsed.RootElement.TryGetProperty("roots", out var rootElement);
                if (rootElement.ValueKind != JsonValueKind.Object)
                {
                    return;
                }

                var root = new FavoriteItem(ProfileInfo);
                rootElement.TryGetProperty("bookmark_bar", out var bookmarkBarElement);
                if (bookmarkBarElement.ValueKind == JsonValueKind.Object)
                {
                    ProcessFavorites(bookmarkBarElement, root, string.Empty, true, false);
                }

                rootElement.TryGetProperty("other", out var otherElement);
                if (otherElement.ValueKind == JsonValueKind.Object)
                {
                    ProcessFavorites(otherElement, root, string.Empty, root.Children.Count == 0, true);
                }

                rootElement.TryGetProperty("synced", out var syncedElement);
                if (syncedElement.ValueKind == JsonValueKind.Object)
                {
                    ProcessFavorites(syncedElement, root, string.Empty, root.Children.Count == 0, true);
                }

                rootElement.TryGetProperty("workspaces", out var workspacesElement);
                if (workspacesElement.ValueKind == JsonValueKind.Object)
                {
                    ProcessFavorites(workspacesElement, root, string.Empty, root.Children.Count == 0, true);
                }

                _root = root;
            }
            catch (Exception ex)
            {
                Log.Exception($"Failed to read favorites: {_path}", ex, typeof(FavoriteProvider));
            }
        }

        private void ProcessFavorites(JsonElement element, FavoriteItem parent, string path, bool root, bool specialFolder)
        {
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("children", out var children))
            {
                var name = element.GetProperty("name").GetString();
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (!root)
                    {
                        path += $"{(string.IsNullOrWhiteSpace(path) ? string.Empty : "/")}{name}";
                    }

                    var folder = new FavoriteItem(name, path, ProfileInfo, specialFolder);

                    if (root)
                    {
                        folder = parent;
                    }
                    else
                    {
                        parent.AddChildren(folder);
                    }

                    if (children.ValueKind == JsonValueKind.Array)
                    {
                        using var childEnumerator = children.EnumerateArray();
                        foreach (var child in childEnumerator)
                        {
                            ProcessFavorites(child, folder, path, false, false);
                        }
                    }
                }
            }
            else if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("url", out var urlProperty))
            {
                var name = element.GetProperty("name").GetString();
                var url = urlProperty.GetString();
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(url))
                {
                    path += $"{(string.IsNullOrWhiteSpace(path) ? string.Empty : "/")}{name}";
                    var favorite = new FavoriteItem(name, url, path, ProfileInfo);
                    parent.AddChildren(favorite);
                }
            }
        }
    }
}
