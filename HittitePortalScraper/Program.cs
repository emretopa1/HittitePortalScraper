using Microsoft.Playwright;
using System;
using System.Web;
using Test;

namespace HittitePortalScraper
{
    public class Program
    {
        private static string OutputDirectory = "hittite_images";
        private const int BatchSize = 50;
        private const int MaxConcurrent = 5;

        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("🏺 Hittite Portal Image Scraper");
            Console.WriteLine("================================\n");

            // Parse command line args or ask user
            List<int> cthNumbers;

            if (args.Length > 0)
            {
                // Parse from command line
                var config = ParseArguments(args);
                OutputDirectory = config.OutputDirectory;
                cthNumbers = config.CthNumbers;
            }
            else
            {
                // Interactive mode - ask user
                cthNumbers = AskUserForCthNumbers();
                OutputDirectory = AskUserForOutputDirectory();
            }

            if (cthNumbers.Count == 0)
            {
                Console.WriteLine("❌ No CTH entries specified!");
                ShowHelp();
                return 1;
            }

            Directory.CreateDirectory(OutputDirectory);

            Console.WriteLine($"\n📋 Will download {cthNumbers.Count} CTH entries:");
            Console.WriteLine($"   {string.Join(", ", cthNumbers.Take(10))}{(cthNumbers.Count > 10 ? "..." : "")}");
            Console.WriteLine($"📁 Output: {Path.GetFullPath(OutputDirectory)}\n");

            await RunScraperAsync(cthNumbers);
            return 0;
        }

        static List<int> AskUserForCthNumbers()
        {
            Console.WriteLine("Which CTH entries do you want to download?");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  1) Single number:    1");
            Console.WriteLine("  2) Range:            1-20");
            Console.WriteLine("  3) Multiple:         1,20,50");
            Console.WriteLine("  4) Mixed:            1-20,50,60-80");
            Console.WriteLine("  5) All:              1-835, 999, 1001");
            Console.WriteLine();
            Console.Write("Enter CTH numbers: ");

            var input = Console.ReadLine() ?? "";
            return ParseCthInput(input);
        }

        static string AskUserForOutputDirectory()
        {
            Console.Write("\nOutput directory (press Enter for './hittite_images'): ");
            var input = Console.ReadLine()?.Trim();
            return string.IsNullOrEmpty(input) ? "hittite_images" : input;
        }

        static List<int> ParseCthInput(string input)
        {
            var numbers = new HashSet<int>();

            // Split by comma
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var trimmed = part.Trim();

                // Check if it's a range (e.g., "305-320")
                if (trimmed.Contains('-'))
                {
                    var rangeParts = trimmed.Split('-');
                    if (rangeParts.Length == 2 &&
                        int.TryParse(rangeParts[0].Trim(), out int start) &&
                        int.TryParse(rangeParts[1].Trim(), out int end))
                    {
                        for (int i = start; i <= end; i++)
                        {
                            numbers.Add(i);
                        }
                    }
                }
                else
                {
                    // Single number
                    if (int.TryParse(trimmed, out int num))
                    {
                        numbers.Add(num);
                    }
                }
            }

            return numbers.OrderBy(n => n).ToList();
        }

        static Config ParseArguments(string[] args)
        {
            var config = new Config();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--cth":
                    case "-c":
                        if (i + 1 < args.Length)
                        {
                            config.CthNumbers = ParseCthInput(args[++i]);
                        }
                        break;

                    case "--output":
                    case "-o":
                        if (i + 1 < args.Length)
                        {
                            config.OutputDirectory = args[++i];
                        }
                        break;

                    case "--help":
                    case "-h":
                    case "/?":
                        ShowHelp();
                        Environment.Exit(0);
                        break;
                }
            }

            return config;
        }

        static void ShowHelp()
        {
            Console.WriteLine("\nUsage: HittitePortalScraper [options]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  --cth, -c <numbers>     CTH numbers to download");
            Console.WriteLine("                          Examples:");
            Console.WriteLine("                            1           (single)");
            Console.WriteLine("                            1-20       (range)");
            Console.WriteLine("                            1,20,50   (multiple)");
            Console.WriteLine("                            1-20,50   (mixed)");
            Console.WriteLine("                            1-835,999,1001   (all)");
            Console.WriteLine();
            Console.WriteLine("  --output, -o <path>     Output directory (default: ./hittite_images)");
            Console.WriteLine("  --help, -h              Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  HittitePortalScraper --cth 305-320");
            Console.WriteLine("  HittitePortalScraper -c 305,310,315 -o ./images");
            Console.WriteLine("  HittitePortalScraper                  (interactive mode)");
        }

        static async Task RunScraperAsync(List<int> cthNumbers)
        {
            const string baseUrl = "https://www.hethport.uni-wuerzburg.de/hetkonk/hetkonk_abfrage.php";

            try
            {
                var downloader = new RestSharpLinks();
                var allImageParams = new List<(string bildnr, string fundnr, string cthName)>();

                // Process each CTH entry directly
                int processed = 0;
                foreach (var cthNum in cthNumbers)
                {
                    processed++;
                    var cthName = $"CTH{cthNum}";
                    Console.WriteLine($"[{processed}/{cthNumbers.Count}] Processing {cthName}...");

                    // Build URL directly
                    var cthUrl = $"{baseUrl}?c={cthNum}";

                    try
                    {
                        // Get image links from both x2 and y2 sections
                        var imageLinks = new List<string>();
                        imageLinks.AddRange(await downloader.GetAllLinksAsync(cthUrl, "//*[@id='y2']/a[@href][1]"));
                        imageLinks.AddRange(await downloader.GetAllLinksAsync(cthUrl, "//*[@id='x2']/a[@href][1]"));

                        if (imageLinks.Count == 0)
                        {
                            Console.WriteLine($"  ⚠️  No images found for {cthName}");
                            continue;
                        }

                        // Get bildpraep links
                        var bildpraepLinks = await downloader.GetAllLinksAsync(imageLinks);

                        // Get touchpic links
                        var touchpicLinks = new List<string>();
                        foreach (var link in bildpraepLinks)
                        {
                            touchpicLinks.AddRange(
                                await downloader.GetAllLinksAsyncd(link, "//a[contains(@href,'touchpic.php')]")
                            );
                        }

                        // Extract bildnr and fundnr parameters
                        foreach (var touchLink in touchpicLinks)
                        {
                            var (bildnr, fundnr) = ExtractImageParams(touchLink);
                            if (!string.IsNullOrEmpty(bildnr) && !string.IsNullOrEmpty(fundnr))
                            {
                                allImageParams.Add((bildnr, fundnr, cthName));
                            }
                        }

                        Console.WriteLine($"  ✓ Found {touchpicLinks.Count} images");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  ✗ Error processing {cthName}: {ex.Message}");
                    }
                }

                if (allImageParams.Count == 0)
                {
                    Console.WriteLine("\n⚠️  No images found to download!");
                    return;
                }

                Console.WriteLine($"\n📸 Total images to download: {allImageParams.Count}");
                Console.WriteLine("🚀 Starting batch download...\n");

                // Download all images in batches
                await ProcessBatchesAsync(allImageParams);

                Console.WriteLine("\n✅ Scraping completed successfully!");
                Console.WriteLine($"📁 Images saved to: {Path.GetFullPath(OutputDirectory)}");

                // Summary
                var downloadedCth = allImageParams.Select(x => x.cthName).Distinct().Count();
                Console.WriteLine($"\n📊 Summary:");
                Console.WriteLine($"   CTH entries: {downloadedCth}/{cthNumbers.Count}");
                Console.WriteLine($"   Total images: {allImageParams.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        static (string bildnr, string fundnr) ExtractImageParams(string link)
        {
            var uri = new Uri("https://dummy.com/" + link);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            return (queryParams["bildnr"] ?? "", queryParams["fundnr"] ?? "");
        }

        static async Task ProcessBatchesAsync(List<(string bildnr, string fundnr, string cthName)> items)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--disable-dev-shm-usage", "--disable-gpu", "--no-sandbox" }
            });

            for (int i = 0; i < items.Count; i += BatchSize)
            {
                var batch = items.Skip(i).Take(BatchSize).ToList();
                var batchNum = i / BatchSize + 1;
                var totalBatches = (items.Count + BatchSize - 1) / BatchSize;

                Console.WriteLine($"📦 Batch {batchNum}/{totalBatches} ({batch.Count} images)");

                var semaphore = new SemaphoreSlim(MaxConcurrent);
                var tasks = batch.Select(async item =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await DownloadImageAsync(browser, item.bildnr, item.fundnr, item.cthName);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        static async Task DownloadImageAsync(IBrowser browser, string bildnr, string fundnr, string cthName)
        {
            const int maxAttempts = 2;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                IPage? page = null;
                IBrowserContext? context = null;

                try
                {
                    context = await browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        IgnoreHTTPSErrors = true,
                        JavaScriptEnabled = true
                    });

                    page = await context.NewPageAsync();

                    var imageDownloaded = false;
                    var tcs = new TaskCompletionSource<bool>();

                    page.Response += async (_, response) =>
                    {
                        if (response.Url.Contains("pixl3.php") &&
                            response.Headers.ContainsKey("content-type") &&
                            response.Headers["content-type"].StartsWith("image"))
                        {
                            try
                            {
                                var bytes = await response.BodyAsync();

                                var cthDir = Path.Combine(OutputDirectory, cthName);
                                Directory.CreateDirectory(cthDir);

                                var filename = $"{bildnr}.jpg";
                                var filepath = Path.Combine(cthDir, filename);

                                await File.WriteAllBytesAsync(filepath, bytes);
                                Console.WriteLine($"  ✓ {cthName}/{filename} ({bytes.Length / 1024:N0} KB)");

                                imageDownloaded = true;
                                tcs.TrySetResult(true);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"  ✗ Error saving {bildnr}: {ex.Message}");
                                tcs.TrySetResult(false);
                            }
                        }
                    };

                    var url = $"https://www.hethport.adwmainz.de/fotarch/touchpic.php?bildnr={bildnr}&fundnr={Uri.EscapeDataString(fundnr)}";
                    await page.GotoAsync(url, new PageGotoOptions { Timeout = 20000 });

                    await Task.WhenAny(tcs.Task, Task.Delay(6000));

                    if (imageDownloaded)
                        return;
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                        Console.WriteLine($"  ✗ Failed: {bildnr} - {ex.Message}");
                }
                finally
                {
                    if (page != null) try { await page.CloseAsync(); } catch { }
                    if (context != null) try { await context.CloseAsync(); } catch { }
                }
            }
        }

        class Config
        {
            public List<int> CthNumbers { get; set; } = new List<int>();
            public string OutputDirectory { get; set; } = "hittite_images";
        }
    }
}

