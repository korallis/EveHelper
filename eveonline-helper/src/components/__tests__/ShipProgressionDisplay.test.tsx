import '@testing-library/jest-dom';
import { render, screen } from '@testing-library/react';
import { ShipProgressionDisplay, ShipProgressionDisplayProps } from '../ShipProgressionDisplay';

describe('ShipProgressionDisplay', () => {
  const baseProps: ShipProgressionDisplayProps = {
    progression: null,
    loading: false,
    error: null,
  };

  it('renders loading state', () => {
    render(<ShipProgressionDisplay {...baseProps} loading={true} />);
    expect(screen.getByText(/loading progression suggestion/i)).toBeInTheDocument();
  });

  it('renders error state', () => {
    render(<ShipProgressionDisplay {...baseProps} error="Something went wrong" />);
    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
  });

  it('renders empty state', () => {
    render(<ShipProgressionDisplay {...baseProps} progression={null} />);
    expect(screen.getByText(/no progression suggestion available/i)).toBeInTheDocument();
  });

  it('renders a progression suggestion', () => {
    const progression = {
      tier: 'Cruiser',
      ship: { ship_id: 2, ship_name: 'Omen Cruiser' },
      required_skills: [
        [333, 'Spaceship Command', 5],
        [444, 'Engineering', 4],
      ] as [number, string, number][],
    };
    render(<ShipProgressionDisplay {...baseProps} progression={progression} />);
    expect(screen.getByText('Next Ship Progression')).toBeInTheDocument();
    expect(screen.getByText('Cruiser')).toBeInTheDocument();
    expect(screen.getByText('Omen Cruiser')).toBeInTheDocument();
    expect(screen.getByText(/Spaceship Command/)).toBeInTheDocument();
    expect(screen.getByText(/Engineering/)).toBeInTheDocument();
    expect(screen.getByText(/Level 5/)).toBeInTheDocument();
    expect(screen.getByText(/Level 4/)).toBeInTheDocument();
  });
}); 