using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;
using Velopack.Util;

namespace WpfApp1
{
    public class UpdateManagerMultipleVersion : UpdateManager
    {
        public UpdateManagerMultipleVersion(string urlOrPath, UpdateOptions options = null, ILogger logger = null, IVelopackLocator locator = null) : base(urlOrPath, options, logger, locator)
        {
           
        }

        public UpdateManagerMultipleVersion(IUpdateSource source, UpdateOptions options = null, ILogger logger = null, IVelopackLocator locator = null) : base(source, options, logger, locator)
        {
        }

        public async Task<UpdateInfo> CheckForUpdatesVersionAsync(SemanticVersion version)
        {
            EnsureInstalled();
            var installedVer = CurrentVersion!;
            var betaId = Locator.GetOrCreateStagedUserId();
            var latestLocalFull = Locator.GetLatestLocalFullPackage();
            
            Log.LogDebug("Retrieving latest release feed.");
            var feedObj = await Source.GetReleaseFeed(Log, Channel, betaId, latestLocalFull).ConfigureAwait(false);
            var feed = feedObj.Assets;

            //var ListlatestRemoteFull = feed.Where(r => r.Type == VelopackAssetType.Full).Where(x => x.Version == version);   //.MaxByPolyfill(x => x.Version).FirstOrDefault();
            var targetVersion = version; // versión de referencia
            var latestRemoteFull = feed
                .Where(r => r.Type == VelopackAssetType.Full)
                .Where(x => x.Version <= targetVersion) // Filtramos versiones <= a la objetivo
                .OrderByDescending(x => x.Version)      // Ordenamos de mayor a menor
                .FirstOrDefault();                      // Tomamos la más alta que cumple

            if (latestRemoteFull == null)
            {
                Log.LogInformation("No remote full releases found.");
                return null;
            }
            
            // there's a newer version available, easy.
            if (latestRemoteFull.Version > installedVer)
            {
                Log.LogInformation($"Found newer remote release available ({installedVer} -> {latestRemoteFull.Version}).");
                return CreateDeltaUpdateStrategy(feed, latestLocalFull, latestRemoteFull);
            }

            // if the remote version is < than current version and downgrade is enabled
            if (latestRemoteFull.Version < installedVer && ShouldAllowVersionDowngrade)
            {
                Log.LogInformation($"Latest remote release is older than current, and downgrade is enabled ({installedVer} -> {latestRemoteFull.Version}).");
                return new UpdateInfo(latestRemoteFull, true);
            }

            // if the remote version is the same as current version, and downgrade is enabled,
            // and we're searching for a different channel than current
            if (ShouldAllowVersionDowngrade && IsNonDefaultChannel)
            {
                if (VersionComparer.Compare(latestRemoteFull.Version, installedVer, VersionComparison.Version) == 0)
                {
                    Log.LogInformation($"Latest remote release is the same version of a different channel, and downgrade is enabled ({installedVer}: {DefaultChannel} -> {Channel}).");
                    return new UpdateInfo(latestRemoteFull, true);
                }
            }

            Log.LogInformation($"No updates, remote version ({latestRemoteFull.Version}) is not newer than current version ({installedVer}) and / or downgrade is not enabled.");
            return null;

        }

    }
}

