// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.EdgeFavorite.Properties;

namespace Community.PowerToys.Run.Plugin.EdgeFavorite.Models
{
    public static class ChannelExtensions
    {
        public static string ToLocalizedString(this Channel channel)
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
    }
}
