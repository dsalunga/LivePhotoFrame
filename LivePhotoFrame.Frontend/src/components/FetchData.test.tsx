import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import FetchData from './FetchData';

describe('FetchData', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('shows loading state initially', () => {
    vi.spyOn(globalThis, 'fetch').mockReturnValue(new Promise(() => {}));
    render(<FetchData />);
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

    render(<FetchData />);
    await waitFor(() => expect(screen.getByText('Mild')).toBeInTheDocument());
    expect(screen.getByText('20')).toBeInTheDocument();
  });

  it('shows empty message on fetch error', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: false,
      status: 500,
    } as Response);

    render(<FetchData />);
    await waitFor(() =>
      expect(screen.getByText(/No data available/)).toBeInTheDocument(),
    );
  });
});
