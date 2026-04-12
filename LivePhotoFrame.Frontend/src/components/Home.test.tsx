import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import Home from './Home';

describe('Home', () => {
  it('renders welcome heading', () => {
    render(<Home />);
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Hello, world!');
  });

  it('lists technology stack', () => {
    render(<Home />);
    expect(screen.getByText(/ASP\.NET Core 10/)).toBeInTheDocument();
    expect(screen.getByText(/React 19/)).toBeInTheDocument();
    expect(screen.getByText(/PostgreSQL/)).toBeInTheDocument();
  });
});
