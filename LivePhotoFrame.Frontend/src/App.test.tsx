import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, it, expect, vi } from 'vitest';
import App from './App';

describe('App routing', () => {
  it('renders Home page at /', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <App />
      </MemoryRouter>,
    );
    expect(screen.getByText('Hello, world!')).toBeInTheDocument();
  });

  it('renders Counter page at /counter', () => {
    render(
      <MemoryRouter initialEntries={['/counter']}>
        <App />
      </MemoryRouter>,
    );
    expect(screen.getByRole('heading', { name: 'Counter' })).toBeInTheDocument();
  });

  it('renders FetchData page at /fetchdata', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue({
      ok: true,
      json: async () => [],
    } as Response);

    render(
      <MemoryRouter initialEntries={['/fetchdata']}>
        <App />
      </MemoryRouter>,
    );
    expect(await screen.findByRole('heading', { name: 'Weather Forecast' })).toBeInTheDocument();
    vi.restoreAllMocks();
  });
});
