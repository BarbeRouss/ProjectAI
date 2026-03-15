import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { EmptyState } from '../empty-state';
import { Home } from 'lucide-react';

describe('EmptyState', () => {
  it('renders title and description', () => {
    render(
      <EmptyState
        icon={Home}
        title="No items"
        description="Start by adding your first item."
      />
    );

    expect(screen.getByText('No items')).toBeInTheDocument();
    expect(screen.getByText('Start by adding your first item.')).toBeInTheDocument();
  });

  it('renders action when provided', () => {
    render(
      <EmptyState
        icon={Home}
        title="No items"
        description="Add an item."
        action={<button>Add Item</button>}
      />
    );

    expect(screen.getByRole('button', { name: 'Add Item' })).toBeInTheDocument();
  });

  it('does not render action section when not provided', () => {
    const { container } = render(
      <EmptyState
        icon={Home}
        title="No items"
        description="Nothing here."
      />
    );

    expect(container.querySelectorAll('button')).toHaveLength(0);
  });

  it('applies custom className', () => {
    const { container } = render(
      <EmptyState
        icon={Home}
        title="Test"
        description="Test description"
        className="my-custom-class"
      />
    );

    const card = container.firstChild as HTMLElement;
    expect(card.className).toContain('my-custom-class');
  });

  it('renders the icon', () => {
    const { container } = render(
      <EmptyState
        icon={Home}
        title="Test"
        description="Test description"
      />
    );

    const svg = container.querySelector('svg');
    expect(svg).toBeInTheDocument();
  });
});
