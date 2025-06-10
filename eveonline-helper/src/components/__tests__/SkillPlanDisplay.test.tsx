import React from 'react';
import { render, screen } from '@testing-library/react';
import { SkillPlanDisplay, SkillPlanDisplayProps } from '../SkillPlanDisplay';

describe('SkillPlanDisplay', () => {
  const baseProps: SkillPlanDisplayProps = {
    skillPlan: null,
    loading: false,
    error: null,
  };

  it('renders loading state', () => {
    render(<SkillPlanDisplay {...baseProps} loading={true} />);
    expect(screen.getByText(/loading skill plan/i)).toBeInTheDocument();
  });

  it('renders error state', () => {
    render(<SkillPlanDisplay {...baseProps} error="Something went wrong" />);
    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
  });

  it('renders empty state', () => {
    render(<SkillPlanDisplay {...baseProps} skillPlan={[]} />);
    expect(screen.getByText(/no missing or under-leveled skills/i)).toBeInTheDocument();
  });

  it('renders a table of skills', () => {
    const skillPlan = [
      { skill_id: 1, skill_name: 'Spaceship Command', required_level: 4, current_level: 2 },
      { skill_id: 2, skill_name: 'Engineering', required_level: 3, current_level: 0 },
    ];
    render(<SkillPlanDisplay {...baseProps} skillPlan={skillPlan} />);
    expect(screen.getByText('Spaceship Command')).toBeInTheDocument();
    expect(screen.getByText('Engineering')).toBeInTheDocument();
    expect(screen.getByText('4')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
    expect(screen.getByText('3')).toBeInTheDocument();
    expect(screen.getByText('0')).toBeInTheDocument();
  });
}); 