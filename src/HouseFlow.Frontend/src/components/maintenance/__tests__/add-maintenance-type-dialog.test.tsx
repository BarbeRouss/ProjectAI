import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, fireEvent } from '@testing-library/react';
import { renderWithProviders } from '@/__tests__/test-utils';
import { AddMaintenanceTypeDialog } from '../add-maintenance-type-dialog';

const mockMutate = vi.fn();

vi.mock('@/lib/api/hooks', () => ({
  useCreateMaintenanceType: () => ({
    mutate: mockMutate,
    isPending: false,
    isError: false,
  }),
}));

describe('AddMaintenanceTypeDialog', () => {
  const defaultProps = {
    deviceId: 'd1',
    open: true,
    onClose: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders nothing when closed', () => {
    const { container } = renderWithProviders(
      <AddMaintenanceTypeDialog {...defaultProps} open={false} />
    );
    expect(container.innerHTML).toBe('');
  });

  it('renders form with name and periodicity fields', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);

    expect(screen.getByLabelText('maintenance.typeName')).toBeInTheDocument();
    expect(screen.getByLabelText('maintenance.periodicity')).toBeInTheDocument();
  });

  it('shows title and description', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);
    expect(screen.getByText('maintenance.addMaintenanceType')).toBeInTheDocument();
    expect(screen.getByText('maintenance.addMaintenanceTypeDescription')).toBeInTheDocument();
  });

  it('shows custom days field when Custom periodicity is selected', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);

    expect(screen.queryByLabelText('maintenance.customDays')).not.toBeInTheDocument();

    fireEvent.change(screen.getByLabelText('maintenance.periodicity'), {
      target: { value: 'Custom' },
    });

    expect(screen.getByLabelText('maintenance.customDays')).toBeInTheDocument();
  });

  it('calls onClose when cancel is clicked', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);
    fireEvent.click(screen.getByText('common.cancel'));
    expect(defaultProps.onClose).toHaveBeenCalled();
  });

  it('submits with correct data', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);

    fireEvent.change(screen.getByLabelText('maintenance.typeName'), {
      target: { value: 'Révision annuelle' },
    });

    fireEvent.click(screen.getByText('common.add'));

    expect(mockMutate).toHaveBeenCalledWith({
      name: 'Révision annuelle',
      periodicity: 'Annual',
      customDays: null,
    });
  });

  it('submits with custom days when Custom is selected', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);

    fireEvent.change(screen.getByLabelText('maintenance.typeName'), {
      target: { value: 'Custom check' },
    });
    fireEvent.change(screen.getByLabelText('maintenance.periodicity'), {
      target: { value: 'Custom' },
    });
    fireEvent.change(screen.getByLabelText('maintenance.customDays'), {
      target: { value: '90' },
    });

    fireEvent.click(screen.getByText('common.add'));

    expect(mockMutate).toHaveBeenCalledWith({
      name: 'Custom check',
      periodicity: 'Custom',
      customDays: 90,
    });
  });

  it('closes backdrop on click', () => {
    renderWithProviders(<AddMaintenanceTypeDialog {...defaultProps} />);

    const backdrop = screen.getByText('maintenance.addMaintenanceType').closest('.fixed');
    fireEvent.click(backdrop!);
    expect(defaultProps.onClose).toHaveBeenCalled();
  });
});
