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
        public IEnumerable<FavoriteItem> GetAll(FavoriteItem node)
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

        public IEnumerable<FavoriteItem> Search(FavoriteItem node, string[] path, int depth)
        {
            if (depth == path.Length - 1)
            {
                return node.Childrens.Where(f => f.Name != null && f.Name.StartsWith(path[depth], StringComparison.InvariantCultureIgnoreCase));
            }

            var folder = node.Childrens.SingleOrDefault(f => f.Type == FavoriteType.Folder && f.Name != null && f.Name.Equals(path[depth], StringComparison.InvariantCultureIgnoreCase));

            if (folder != null)
            {
                return Search(folder, path, ++depth);
            }

            return Enumerable.Empty<FavoriteItem>();
        }
    }
}
