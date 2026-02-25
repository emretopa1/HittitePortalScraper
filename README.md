# 🏛️ Hittite Portal Scraper

A high-performance .NET application for scraping and downloading images from the Hittite Portal archaeological database at the University of Würzburg.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 📋 Overview

The Hittite Portal Scraper automates the extraction and download of archaeological images from the [Hethitologie Portal Mainz](https://www.hethport.uni-wuerzburg.de/CTH/). It uses Playwright for browser automation and implements optimized batch processing with concurrent downloads.

### Key Features

- **Automated Scraping**: Systematically crawls the Hittite Portal catalog
- **Batch Processing**: Handles large datasets with configurable batch sizes
- **Concurrent Downloads**: Parallel processing with rate limiting to prevent overload
- **Memory Optimized**: Includes garbage collection strategies for long-running operations
- **Retry Logic**: Automatic retry mechanism for failed downloads
- **Docker Support**: Containerized deployment for consistent environments

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- [Docker](https://www.docker.com/get-started) (optional, for containerized deployment)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/HittitePortalScraper.git
   cd HittitePortalScraper
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project Test/HittitePortalScraper.csproj
   ```

### Docker Deployment

#### Using Docker Compose (Recommended)

```bash
docker-compose up --build
```

#### Manual Docker Build

```bash
# Build the image
docker build -t hittite-scraper .

# Run the container
docker run -v $(pwd)/images:/app/images hittite-scraper
```

The downloaded images will be saved to the `./images` directory on your host machine.

## 🛠️ Configuration

### Performance Tuning

You can adjust the following parameters in `Program.cs`:

```csharp
const int batchSize = 50;        // Number of items per batch
const int maxConcurrent = 5;     // Maximum concurrent downloads
```

### Browser Options

Playwright browser settings can be modified for your environment:

```csharp
await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = true,
    Args = new[] {
        "--disable-dev-shm-usage",
        "--disable-gpu",
        "--no-sandbox"
    }
});
```

## 📦 Dependencies

- **[Microsoft.Playwright](https://playwright.dev/dotnet/)** - Browser automation
- **[RestSharp](https://restsharp.dev/)** - HTTP client library
- **.NET 8.0** - Runtime framework

## 🏗️ Project Structure

```
HittitePortalScraper/
├── HittitePortalScraper/
│   ├── HittitePortalScraper.csproj
│   ├── Program.cs              # Main application logic
│   └── ImageDownloader.cs      # Image scraping utilities
├── images/                     # Downloaded images (created at runtime)
├── Dockerfile
├── docker-compose.yml
├── .dockerignore
├── .gitignore
└── README.md
```

## 🔄 How It Works

1. **Initial Crawl**: Starts from the main CTH portal page
2. **Link Extraction**: Recursively extracts catalog links using XPath selectors
3. **Image Discovery**: Identifies `touchpic.php` endpoints with image parameters
4. **Batch Processing**: Groups downloads into manageable batches
5. **Concurrent Execution**: Uses semaphores to control parallel downloads
6. **Response Interception**: Captures `pixl3.php` image responses via Playwright
7. **File Storage**: Saves images with `bildnr` as filename

## 📊 Performance

- Processes **50 items per batch** by default
- Supports **5 concurrent downloads** simultaneously
- Includes automatic garbage collection between batches
- Built-in retry logic (2 attempts per image)
- Timeout protection (20s page load, 6s image wait)

## 🐳 Docker Details

The Docker setup includes:

- **Multi-stage build** for optimized image size
- **Playwright browser dependencies** pre-installed
- **Volume mounting** for persistent image storage
- **Non-root user** for security
- **Health checks** for container monitoring

## ⚠️ Important Notes

- **Respect Rate Limits**: The tool includes rate limiting, but always use responsibly
- **Data Usage**: Downloading large image collections will consume significant bandwidth
- **Legal Compliance**: Ensure you have permission to scrape and store these images
- **Attribution**: Images belong to the Hethitologie Portal Mainz at the University of Würzburg

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Hethitologie Portal Mainz](https://www.hethport.uni-wuerzburg.de/) - Source of the archaeological data
- University of Würzburg - Maintaining the Hittite Portal
- Playwright Team - For the excellent browser automation framework

## 📧 Contact

For questions or suggestions, please open an issue on GitHub.

---

**Note**: This tool is for academic and research purposes. Always respect the terms of service of the target website and applicable copyright laws.
