using System;
using System.IO;
using System.Linq;

namespace Magma.DPDK
{
    /// <summary>
    /// Helper class for detecting and configuring huge pages on Linux.
    /// </summary>
    public static class HugePageHelper
    {
        private const string HugePagesDir = "/sys/kernel/mm/hugepages";
        private const string HugePagesMountDir = "/proc/mounts";

        /// <summary>
        /// Information about huge page configuration.
        /// </summary>
        public class HugePageInfo
        {
            public long TotalPages { get; set; }
            public long FreePages { get; set; }
            public long PageSizeKb { get; set; }
            public bool IsAvailable { get; set; }
            public string MountPoint { get; set; }

            public long TotalSizeMb => (TotalPages * PageSizeKb) / 1024;
            public long FreeSizeMb => (FreePages * PageSizeKb) / 1024;
        }

        /// <summary>
        /// Detects if huge pages are available on the system.
        /// </summary>
        /// <returns>True if huge pages are available, false otherwise</returns>
        public static bool IsHugePagesAvailable()
        {
            return Directory.Exists(HugePagesDir);
        }

        /// <summary>
        /// Gets information about the default huge page configuration (typically 2MB pages).
        /// </summary>
        /// <returns>Huge page information, or null if not available</returns>
        public static HugePageInfo GetDefaultHugePageInfo()
        {
            return GetHugePageInfo("hugepages-2048kB");
        }

        /// <summary>
        /// Gets information about a specific huge page size.
        /// </summary>
        /// <param name="hugepageDir">Huge page directory name (e.g., "hugepages-2048kB" or "hugepages-1048576kB")</param>
        /// <returns>Huge page information, or null if not available</returns>
        public static HugePageInfo GetHugePageInfo(string hugepageDir)
        {
            if (!IsHugePagesAvailable())
                return null;

            var hugepagePath = Path.Combine(HugePagesDir, hugepageDir);
            if (!Directory.Exists(hugepagePath))
                return null;

            try
            {
                var info = new HugePageInfo
                {
                    IsAvailable = true
                };

                var nrHugepagesFile = Path.Combine(hugepagePath, "nr_hugepages");
                if (File.Exists(nrHugepagesFile))
                {
                    var content = File.ReadAllText(nrHugepagesFile).Trim();
                    if (long.TryParse(content, out var totalPages))
                        info.TotalPages = totalPages;
                }

                var freeHugepagesFile = Path.Combine(hugepagePath, "free_hugepages");
                if (File.Exists(freeHugepagesFile))
                {
                    var content = File.ReadAllText(freeHugepagesFile).Trim();
                    if (long.TryParse(content, out var freePages))
                        info.FreePages = freePages;
                }

                var pageSizeKb = ExtractPageSizeFromDirName(hugepageDir);
                if (pageSizeKb.HasValue)
                    info.PageSizeKb = pageSizeKb.Value;

                info.MountPoint = GetHugePagesMountPoint();

                return info;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the mount point for hugetlbfs.
        /// </summary>
        /// <returns>Mount point, or null if not found</returns>
        public static string GetHugePagesMountPoint()
        {
            if (!File.Exists(HugePagesMountDir))
                return null;

            try
            {
                var lines = File.ReadAllLines(HugePagesMountDir);
                var hugetlbfsLine = lines.FirstOrDefault(line => line.Contains("hugetlbfs"));
                if (hugetlbfsLine is not null)
                {
                    var parts = hugetlbfsLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                        return parts[1];
                }
            }
            catch (Exception)
            {
            }

            return "/dev/hugepages";
        }

        /// <summary>
        /// Checks if the system has sufficient huge pages configured.
        /// </summary>
        /// <param name="requiredSizeMb">Required size in megabytes</param>
        /// <returns>True if sufficient huge pages are available</returns>
        public static bool HasSufficientHugePages(long requiredSizeMb)
        {
            var info = GetDefaultHugePageInfo();
            if (info is null || !info.IsAvailable)
                return false;

            return info.FreeSizeMb >= requiredSizeMb;
        }

        /// <summary>
        /// Gets a recommended huge page configuration message.
        /// </summary>
        /// <param name="requiredSizeMb">Required size in megabytes</param>
        /// <returns>Configuration message</returns>
        public static string GetConfigurationMessage(long requiredSizeMb)
        {
            var info = GetDefaultHugePageInfo();
            if (info is null || !info.IsAvailable)
            {
                return "Huge pages are not available on this system. " +
                       "Enable huge pages in the kernel or use --no-huge or --in-memory options.";
            }

            if (info.FreeSizeMb < requiredSizeMb)
            {
                var requiredPages = (requiredSizeMb * 1024) / info.PageSizeKb;
                return $"Insufficient huge pages. Required: {requiredSizeMb}MB ({requiredPages} pages), " +
                       $"Available: {info.FreeSizeMb}MB ({info.FreePages} pages). " +
                       $"Configure more huge pages with: echo {requiredPages} > /sys/kernel/mm/hugepages/hugepages-{info.PageSizeKb}kB/nr_hugepages";
            }

            return $"Huge pages are properly configured. Available: {info.FreeSizeMb}MB ({info.FreePages} pages).";
        }

        private static long? ExtractPageSizeFromDirName(string dirName)
        {
            if (string.IsNullOrEmpty(dirName))
                return null;

            var parts = dirName.Split('-');
            if (parts.Length != 2)
                return null;

            var sizeStr = parts[1].Replace("kB", "").Replace("KB", "");
            if (long.TryParse(sizeStr, out var size))
                return size;

            return null;
        }
    }
}
