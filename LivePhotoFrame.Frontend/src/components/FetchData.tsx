import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';

interface WeatherForecast {
  dateFormatted: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

function FetchData() {
  const { startDateIndex: startDateIndexParam } = useParams<{ startDateIndex?: string }>();
  const [forecasts, setForecasts] = useState<WeatherForecast[]>([]);
  const [loading, setLoading] = useState(true);

  const startDateIndex = useMemo(() => {
    const parsed = Number.parseInt(startDateIndexParam ?? '0', 10);
    return Number.isNaN(parsed) ? 0 : parsed;
  }, [startDateIndexParam]);

  useEffect(() => {
    setLoading(true);
    fetch(`/api/SampleData/WeatherForecasts?startDateIndex=${startDateIndex}`)
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
  }, [startDateIndex]);

  if (loading) {
    return <p>Loading...</p>;
  }

  const prevStartDateIndex = startDateIndex - 5;
  const nextStartDateIndex = startDateIndex + 5;

  return (
    <div>
      <h1>Weather forecast</h1>
      <p>This component demonstrates fetching data from the server and working with URL parameters.</p>
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
      <p className="pagination">
        <Link to={`/fetchdata/${prevStartDateIndex}`}>Previous</Link>
        <Link to={`/fetchdata/${nextStartDateIndex}`}>Next</Link>
      </p>
    </div>
  );
}

export default FetchData;
