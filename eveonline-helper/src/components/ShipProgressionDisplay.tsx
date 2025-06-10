import React from 'react';
import { ShipProgressionSuggestion } from '../hooks/useShipProgression';

/**
 * Props for ShipProgressionDisplay
 */
export interface ShipProgressionDisplayProps {
  progression: ShipProgressionSuggestion | null;
  loading: boolean;
  error: string | null;
}

/**
 * Displays the next ship progression suggestion for the user
 * @param progression The progression suggestion
 * @param loading Loading state
 * @param error Error message
 */
export const ShipProgressionDisplay: React.FC<ShipProgressionDisplayProps> = ({ progression, loading, error }) => {
  if (loading) return <div>Loading progression suggestion...</div>;
  if (error) return <div style={{ color: 'red' }}>Error: {error}</div>;
  if (!progression) return <div>No progression suggestion available.</div>;

  return (
    <div className="border border-blue-300 rounded p-3 mt-2">
      <h3 className="font-bold text-lg mb-2">Next Ship Progression</h3>
      <div><strong>Tier:</strong> {progression.tier}</div>
      <div><strong>Suggested Ship:</strong> {progression.ship.ship_name}</div>
      <div className="mt-2">
        <strong>Required Skills:</strong>
        <ul className="list-disc ml-6">
          {progression.required_skills.map(([id, name, level]) => (
            <li key={id}>{name} (Level {level})</li>
          ))}
        </ul>
      </div>
    </div>
  );
}; 