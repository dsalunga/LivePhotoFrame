# LivePhotoFrame

LivePhotoFrame is a multi-project photo slideshow app that helps you turn your archived images into a full-screen digital photo frame experience.

## Features

- Full-screen slideshow playback for desktop-style photo frame use.
- Supports image sources from:
  - local file system folders
  - FTP servers
- Configurable slideshow interval and max idle timeout.
- Optional portrait skipping.
- Image display modes: `Uniform`, `UniformToFill`, and `BestFit`.
- Local caching for remote FTP images.
- Quick controls while viewing: left/right navigation and exit shortcuts.

## Solution Structure

- `LivePhotoFrame.sln`: full solution container.
- `LivePhotoFrame/LivePhotoFrame`: shared `.NET Standard 2.0` core library.
- `LivePhotoFrame/LivePhotoFrame.Android`: Xamarin Android app.
- `LivePhotoFrame/LivePhotoFrame.iOS`: Xamarin iOS app.
- `LivePhotoFrame/LivePhotoFrame.UWP`: original UWP project.
- `LivePhotoFrame.UWPv2`: newer UWP variant.
- `LivePhotoFrame.WebApp`: ASP.NET Core MVC web app (`net10.0`) serving the frontend from `/app`.
- `LivePhotoFrame.Frontend`: React + TypeScript + Vite frontend (active web UI).
- `LivePhotoFrame.ReactJs`: legacy ASP.NET Core + React + webpack project (kept for historical reference, not part of active build pipeline).

## Prerequisites

- `.NET SDK 10+` (tested with SDK `10.0.103`).
- `Node.js + npm` (required for `LivePhotoFrame.Frontend`).
- PostgreSQL (default) or SQL Server (fallback) for `LivePhotoFrame.WebApp` identity database.
- For native app targets:
  - Windows + Visual Studio (UWP tooling) for `LivePhotoFrame.UWPv2` / `LivePhotoFrame.UWP`.
  - Xamarin workloads for `LivePhotoFrame.Android` and `LivePhotoFrame.iOS`.

## Quick Start

### 1) Clone

```bash
git clone https://github.com/dsalunga/LivePhotoFrame.git
cd LivePhotoFrame
```

### 2) Build and run the ASP.NET MVC web app

```bash
cd LivePhotoFrame.WebApp
dotnet restore
dotnet build
dotnet run
```

The launch profile defaults to `http://localhost:5142` in development.

### 3) Build the active frontend (Vite)

```bash
cd LivePhotoFrame.Frontend
npm ci
npm run build
```

This writes static assets to `LivePhotoFrame.WebApp/wwwroot/app`.

### 4) Optional frontend dev server

```bash
cd LivePhotoFrame.Frontend
npm ci
npm run dev
```

The dev server proxies `/api` requests to `http://localhost:5142`.

## Configuration Notes

### UWP slideshow settings

From the `Settings` page you can configure:

- FTP host/path/username/password
- local file system image folder
- active source (`FTP` or `FileSystem`)
- interval (minutes)
- max idle time (minutes)
- auto-start behavior
- skip portraits
- image display mode

Default placeholders include:

- FTP host: `ftp.yourserver.com`
- FTP path: `/path/to/your/photos/`
- File system path: `D:\Pictures\LivePhotoFrame\Albums\Current\`

### Web app database

`LivePhotoFrame.WebApp/appsettings.json` defaults to PostgreSQL:

```json
"DatabaseProvider": "postgres",
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=livephotoframe;Username=postgres;Password=postgres",
  "SqlServerConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-LivePhotoFrame.WebApp-...;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

Set `DatabaseProvider=sqlserver` to use the SQL Server fallback connection.

## Troubleshooting

- Frontend assets missing under `/app`  
  Run `npm ci && npm run build` inside `LivePhotoFrame.Frontend`.
- `Xamarin.Android.CSharp.targets` or `Xamarin.iOS.CSharp.targets` not found  
  Install Xamarin workloads and build through Visual Studio with Xamarin support.
- `Microsoft.Windows.UI.Xaml.CSharp.targets` not found  
  Build UWP projects on Windows with UWP tooling installed.

## License

Licensed under the MIT License. See [LICENSE](LICENSE).
