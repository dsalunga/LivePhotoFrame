import { useEffect, useState } from 'react';

interface WeatherForecast {
  dateFormatted: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

function FetchData() {
  const [forecasts, setForecasts] = useState<WeatherForecast[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch('/api/SampleData/WeatherForecasts')
      .then(res => {
        if (!res.ok) throw new Error('Network response was not ok');
        return res.json();
      })
      .then((data: WeatherForecast[]) => {
        setForecasts(data);
        setLoading(false);
      })
      .catch(() => {
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <p>Loading...</p>;
  }

  return (
    <div>
      <h1>Weather Forecast</h1>
      <p>This component demonstrates fetching data from the server.</p>
      {forecasts.length === 0 ? (
        <p>No data available. Ensure the API endpoint is running.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Date</th>
              <th>Temp. (C)</th>
              <th>Temp. (F)</th>
              <th>Summary</th>
            </tr>
          </thead>
          <tbody>
            {forecasts.map((f, i) => (
              <tr key={i}>
                <td>{f.dateFormatted}</td>
                <td>{f.temperatureC}</td>
                <td>{f.temperatureF}</td>
                <td>{f.summary}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

export default FetchData;
