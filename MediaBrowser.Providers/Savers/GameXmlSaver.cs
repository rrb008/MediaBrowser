﻿using System.Security;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Providers.Movies;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace MediaBrowser.Providers.Savers
{
    /// <summary>
    /// Saves game.xml for games
    /// </summary>
    public class GameXmlSaver : IMetadataSaver
    {
        private readonly IServerConfigurationManager _config;

        public GameXmlSaver(IServerConfigurationManager config)
        {
            _config = config;
        }

        /// <summary>
        /// Determines whether [is enabled for] [the specified item].
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <returns><c>true</c> if [is enabled for] [the specified item]; otherwise, <c>false</c>.</returns>
        public bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            var wasMetadataEdited = (updateType & ItemUpdateType.MetadataEdit) == ItemUpdateType.MetadataEdit;
            var wasMetadataDownloaded = (updateType & ItemUpdateType.MetadataDownload) == ItemUpdateType.MetadataDownload;

            // If new metadata has been downloaded and save local is on, OR metadata was manually edited, proceed
            if ((_config.Configuration.SaveLocalMeta && (wasMetadataEdited || wasMetadataDownloaded)) || wasMetadataEdited)
            {
                return item is Game;
            }

            return false;
        }

        private static readonly CultureInfo UsCulture = new CultureInfo("en-US");

        /// <summary>
        /// Saves the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        public void Save(BaseItem item, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();

            builder.Append("<Item>");

            var game = (Game)item;

            if (game.PlayersSupported.HasValue)
            {
                builder.Append("<Players>" + SecurityElement.Escape(game.PlayersSupported.Value.ToString(UsCulture)) + "</Players>");
            }

            if (!string.IsNullOrEmpty(game.GameSystem))
            {
                builder.Append("<GameSystem><![CDATA[" + game.GameSystem + "]]></GameSystem>");
            }
            
            XmlSaverHelpers.AddCommonNodes(item, builder);

            builder.Append("</Item>");

            var xmlFilePath = GetSavePath(item);

            XmlSaverHelpers.Save(builder, xmlFilePath, new[]
                {
                    "Players",
                    "GameSystem"
                });

            // Set last refreshed so that the provider doesn't trigger after the file save
            MovieProviderFromXml.Current.SetLastRefreshed(item, DateTime.UtcNow);
        }

        public string GetSavePath(BaseItem item)
        {
            if (item.ResolveArgs.IsDirectory)
            {
                var path = Directory.Exists(item.Path) ? item.Path : Path.GetDirectoryName(item.Path);

                return Path.Combine(path, "game.xml");
            }

            return Path.ChangeExtension(item.Path, ".xml");
        }
    }
}
