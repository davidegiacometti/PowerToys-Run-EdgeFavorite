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
                foreach (var favorite in GetAll(root))
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

                // Flatten folders with same path for each profiles
                return results.DistinctBy(f => new { f.Path, f.Type, f.Profile });
            }
        }

        private static IEnumerable<FavoriteItem> GetAll(FavoriteItem node)
        {
            if (node.Type == FavoriteType.Url)
            {
                yield return node;
            }
            else
            {
                foreach (var child in node.Childrens)
                {
                    foreach (var item in GetAll(child))
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
                return node.Childrens.Where(f => f.Name != null && f.Name.StartsWith(path[depth], StringComparison.OrdinalIgnoreCase));
            }

            var folder = node.Childrens.SingleOrDefault(f => f.Type == FavoriteType.Folder && f.Name != null && f.Name.Equals(path[depth], StringComparison.OrdinalIgnoreCase));

            if (folder != null)
            {
                return Search(folder, path, ++depth);
            }

            return Enumerable.Empty<FavoriteItem>();
        }
    }
}
