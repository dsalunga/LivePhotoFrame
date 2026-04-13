import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import FetchData from './FetchData';

function renderAt(path: string) {
  render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/fetchdata/:startDateIndex?" element={<FetchData />} />
      </Routes>
    </MemoryRouter>,
  );
}

describe('FetchData', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('shows loading state initially', () => {
    vi.spyOn(globalThis, 'fetch').mockReturnValue(new Promise(() => {}));
    renderAt('/fetchdata/10');
    expect(globalThis.fetch).toHaveBeenCalledWith('/api/SampleData/WeatherForecasts?startDateIndex=10');
    expect(screen.getByText('Loading...')).toBeInTheDocument();
  });

  it('renders forecast table on success', async () => {
    const data = [
      { dateFormatted: '2026-04-15', temperatureC: 20, temperatureF: 68, summary: 'Mild' },
    ];
    vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: true,
      json: async () => data,
    } as Response);

    renderAt('/fetchdata/5');
    await waitFor(() => expect(screen.getByText('Mild')).toBeInTheDocument());
    expect(screen.getByText('20')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Previous' })).toHaveAttribute('href', '/fetchdata/0');
    expect(screen.getByRole('link', { name: 'Next' })).toHaveAttribute('href', '/fetchdata/10');
  });

  it('shows empty message on fetch error', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: false,
      status: 500,
    } as Response);

    renderAt('/fetchdata');
    await waitFor(() =>
      expect(screen.getByText(/No data available/)).toBeInTheDocument(),
    );
  });
});
