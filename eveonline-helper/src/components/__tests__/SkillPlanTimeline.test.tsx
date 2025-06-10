import React from 'react';
import { render, screen } from '@testing-library/react';
import { SkillPlanTimeline } from '../SkillPlanTimeline';

describe('SkillPlanTimeline', () => {
  it('renders empty state', () => {
    render(<SkillPlanTimeline skillPlan={[]} />);
    expect(screen.getByText(/no skill plan to visualize/i)).toBeInTheDocument();
  });

  it('renders a single skill node', () => {
    const skillPlan = [
      { skill_id: 1, skill_name: 'Spaceship Command', required_level: 4, current_level: 2 },
    ];
    render(<SkillPlanTimeline skillPlan={skillPlan} />);
    expect(screen.getByText('Spaceship Command')).toBeInTheDocument();
    expect(screen.getByText(/Required: Level 4/)).toBeInTheDocument();
    expect(screen.getByText(/Current: Level 2/)).toBeInTheDocument();
  });

  it('renders multiple skill nodes and progress bars', () => {
    const skillPlan = [
      { skill_id: 1, skill_name: 'Spaceship Command', required_level: 4, current_level: 4 },
      { skill_id: 2, skill_name: 'Engineering', required_level: 3, current_level: 1 },
    ];
    render(<SkillPlanTimeline skillPlan={skillPlan} />);
    expect(screen.getByText('Spaceship Command')).toBeInTheDocument();
    expect(screen.getByText('Engineering')).toBeInTheDocument();
    // Check for completion color (bg-green-500) and in-progress color (bg-blue-400)
    const greenBars = document.querySelectorAll('.bg-green-500');
    const blueBars = document.querySelectorAll('.bg-blue-400');
    expect(greenBars.length).toBeGreaterThan(0);
    expect(blueBars.length).toBeGreaterThan(0);
  });
}); 