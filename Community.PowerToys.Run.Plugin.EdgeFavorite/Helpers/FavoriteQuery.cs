// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Community.PowerToys.Run.Plugin.EdgeFavorite.Models;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Helpers
{
    public class FavoriteQuery : IFavoriteQuery
    {
        private readonly IProfileManager _profileManager;

        public FavoriteQuery(IProfileManager profileManager)
        {
            _profileManager = profileManager;
        }

        public IEnumerable<FavoriteItem> GetAll()
        {
            foreach (var root in _profileManager.FavoriteProviders.Select(p => p.Root))
            {
                foreach (var favorite in GetAll(root, false))
                {
                    yield return favorite;
                }
            }
        }

        public IEnumerable<FavoriteItem> Search(string query)
        {
            var path = query.Replace('\\', '/').Split('/');

            if (_profileManager.FavoriteProviders.Count == 1)
            {
                return Search(_profileManager.FavoriteProviders[0].Root, path, 0);
            }
            else
            {
                var results = new List<FavoriteItem>();

                foreach (var root in _profileManager.FavoriteProviders.Select(p => p.Root))
                {
                    results.AddRange(Search(root, path, 0));
                }

                return results;
            }
        }

        private static IEnumerable<FavoriteItem> GetAll(FavoriteItem node, bool isChild)
        {
            if (node.Type == FavoriteType.Url)
            {
                yield return node;
            }
            else
            {
                // Skip the root node
                if (isChild)
                {
                    yield return node;
                }

                foreach (var child in node.Children)
                {
                    foreach (var item in GetAll(child, true))
                    {
                        yield return item;
                    }
                }
            }
        }

        private static IEnumerable<FavoriteItem> Search(FavoriteItem node, string[] path, int depth)
        {
            if (depth == path.Length - 1)
            {
                var token = path[depth].TrimEnd('*');

                if (token.Length > 1 && token[0] == '*')
                {
                    return node.Children.Where(f => f.Name != null && f.Name.Contains(token.TrimStart('*'), StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    return node.Children.Where(f => f.Name != null && f.Name.StartsWith(token, StringComparison.OrdinalIgnoreCase));
                }
            }

            var folder = node.Children.SingleOrDefault(f => f.Type == FavoriteType.Folder && f.Name != null && f.Name.Equals(path[depth], StringComparison.OrdinalIgnoreCase));

            if (folder != null)
            {
                return Search(folder, path, ++depth);
            }

            return Enumerable.Empty<FavoriteItem>();
        }
    }
}
