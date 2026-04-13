# Frontend API Contract

Last updated: 2026-04-13

## Runtime Topology

- Frontend static build is served by `LivePhotoFrame.WebApp` at `/app`.
- SPA deep-link fallback is handled at `/app/{*path}` to `/app/index.html`.
- `FetchData` route supports URL paging parity: `/app/fetchdata/:startDateIndex?`.

## Endpoints Used by Frontend

### `GET /api/SampleData/WeatherForecasts`

Optional query params:

- `startDateIndex` (`int`, default: `0`)

Response shape (`200 OK`):

```json
[
  {
    "dateFormatted": "14/4/2026",
    "temperatureC": 0,
    "temperatureF": 32,
    "summary": "Bracing"
  }
]
```

Notes:

- Returns 5 generated items per request.
- Endpoint is currently sample/demo data and can be replaced with real slideshow metadata in a future phase.

### `GET /healthz`

Response behavior:

- `200 OK` when app dependencies (including DB health check) are ready.
- `503 Service Unavailable` when dependency health checks fail (expected readiness signal).

## Local Development Defaults

- Web backend local URL: `http://localhost:5142`
- Vite dev proxy target: `http://localhost:5142`
