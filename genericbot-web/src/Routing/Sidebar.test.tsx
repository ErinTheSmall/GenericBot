import React from 'react';
import { render } from '@testing-library/react';
import Sidebar from './Sidebar';

test('renders sidebar title', () => {
  const { getByText } = render(<Sidebar />);
  const header = getByText(/GenericBot/);
  expect(header).toBeInTheDocument();
});

test('renders link properly', () => {
    const { getByText } = render(<Sidebar />);
    const link = getByText(/Link One/i);
    expect(link).toBeInTheDocument();
  });