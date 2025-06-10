import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { SkillPlanExportButton } from '../SkillPlanExportButton';
import * as skillPlanUtils from '../../hooks/useSkillPlan';

describe('SkillPlanExportButton', () => {
  const skillPlan = [
    { skill_id: 1, skill_name: 'Spaceship Command', required_level: 4, current_level: 2 },
    { skill_id: 2, skill_name: 'Engineering', required_level: 3, current_level: 0 },
  ];

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders and triggers export on click', async () => {
    const mockExport = jest.spyOn(skillPlanUtils, 'exportSkillPlanEvemon').mockResolvedValue('Spaceship Command Level 4\nEngineering Level 3');
    render(<SkillPlanExportButton skillPlan={skillPlan} />);
    const button = screen.getByText(/export to evemon/i);
    fireEvent.click(button);
    expect(button).toHaveTextContent(/exporting/i);
    await waitFor(() => expect(mockExport).toHaveBeenCalled());
    // Download is triggered (cannot assert file save in jsdom, but no error shown)
    expect(screen.queryByText(/failed/i)).not.toBeInTheDocument();
  });

  it('shows error if export fails', async () => {
    jest.spyOn(skillPlanUtils, 'exportSkillPlanEvemon').mockRejectedValue(new Error('Export failed'));
    render(<SkillPlanExportButton skillPlan={skillPlan} />);
    const button = screen.getByText(/export to evemon/i);
    fireEvent.click(button);
    await waitFor(() => expect(screen.getByText(/export failed/i)).toBeInTheDocument());
  });

  it('disables button if skill plan is empty', () => {
    render(<SkillPlanExportButton skillPlan={[]} />);
    const button = screen.getByText(/export to evemon/i);
    expect(button).toBeDisabled();
  });
}); 