import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApprovalStatus } from '../../models/workflow.model';

/** Maps the integer enum values sent by the .NET backend → string names. */
const STATUS_MAP: Record<number, ApprovalStatus> = {
  0: 'Pending',
  1: 'Approved',
  2: 'Rejected'
};

@Component({
  selector: 'app-workflow-state-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span
      class="workflow-badge"
      [class.pending]="statusName === 'Pending'"
      [class.approved]="statusName === 'Approved'"
      [class.rejected]="statusName === 'Rejected'"
      [title]="tooltip"
    >
      {{ label }}
    </span>
  `,
  styles: [`
    .workflow-badge {
      display: inline-flex;
      align-items: center;
      padding: 0.25rem 0.75rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.025em;
      cursor: help;
    }

    .pending {
      background-color: #fef3c7;
      color: #92400e;
    }

    .approved {
      background-color: #d1fae5;
      color: #065f46;
    }

    .rejected {
      background-color: #fee2e2;
      color: #991b1b;
    }
  `]
})
export class WorkflowStateBadgeComponent {
  /**
   * Accepts the approval status as either:
   *  - A string name  ('Pending', 'Approved', 'Rejected')
   *  - An integer     (0, 1, 2)
   */
  @Input() set state(value: ApprovalStatus | number) {
    this.statusName = typeof value === 'number'
      ? (STATUS_MAP[value] ?? 'Pending')
      : (value as ApprovalStatus);

    this.label   = this.statusName;
    this.tooltip = this.buildTooltip(this.statusName);
  }

  statusName: ApprovalStatus = 'Pending';
  label    = 'Pending';
  tooltip  = '';

  private buildTooltip(s: ApprovalStatus): string {
    const tooltips: Record<ApprovalStatus, string> = {
      Pending:         'Product is awaiting approval',
      Approved:        'Product has been approved by admin',
      Rejected:        'Product was rejected and needs revision'
    };
    return tooltips[s] ?? '';
  }
}
