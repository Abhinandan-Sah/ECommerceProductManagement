import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApprovalStatus } from '../../models/workflow.model';

interface TimelineState {
  status: ApprovalStatus;
  label: string;
  isCompleted: boolean;
  isCurrent: boolean;
  isPending: boolean;
}

@Component({
  selector: 'app-workflow-state-timeline',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="workflow-timeline">
      <div class="timeline-header">
        <h3 class="timeline-title">Approval Progress</h3>
      </div>
      <div class="timeline-container">
        <div 
          *ngFor="let item of timelineStates; let i = index; let last = last"
          class="timeline-item"
          [class.completed]="item.isCompleted"
          [class.current]="item.isCurrent"
          [class.pending]="item.isPending"
          [class.rejected]="currentState === 'Rejected' && item.status === 'Pending'"
        >
          <div class="timeline-marker">
            <div class="marker-circle">
              <svg 
                *ngIf="item.isCompleted" 
                class="checkmark-icon" 
                fill="none" 
                viewBox="0 0 24 24" 
                stroke="currentColor"
              >
                <path 
                  stroke-linecap="round" 
                  stroke-linejoin="round" 
                  stroke-width="2" 
                  d="M5 13l4 4L19 7"
                />
              </svg>
              <span *ngIf="item.isCurrent" class="current-dot"></span>
              <svg 
                *ngIf="currentState === 'Rejected' && item.status === 'Pending'" 
                class="error-icon" 
                fill="none" 
                viewBox="0 0 24 24" 
                stroke="currentColor"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <div *ngIf="!last" class="connector-line"></div>
          </div>
          <div class="timeline-content">
            <div class="state-label">{{ item.label }}</div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .workflow-timeline {
      background-color: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 0.5rem;
      padding: 1.5rem;
      margin: 1rem 0;
    }

    .timeline-header {
      margin-bottom: 1.5rem;
    }

    .timeline-title {
      font-size: 1.125rem;
      font-weight: 600;
      color: #111827;
      margin: 0;
    }

    .timeline-container {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .timeline-item {
      display: flex;
      gap: 1rem;
      position: relative;
    }

    .timeline-marker {
      display: flex;
      flex-direction: column;
      align-items: center;
      position: relative;
    }

    .marker-circle {
      width: 2rem;
      height: 2rem;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      z-index: 1;
      transition: all 0.3s ease;
    }

    .timeline-item.completed .marker-circle {
      background-color: #10b981;
      border: 2px solid #10b981;
    }

    .timeline-item.current .marker-circle {
      background-color: #3b82f6;
      border: 2px solid #3b82f6;
      box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.2);
    }

    .timeline-item.rejected .marker-circle {
      background-color: #ef4444;
      border: 2px solid #ef4444;
      box-shadow: 0 0 0 4px rgba(239, 68, 68, 0.2);
    }

    .timeline-item.pending .marker-circle {
      background-color: #f3f4f6;
      border: 2px solid #d1d5db;
    }

    .checkmark-icon, .error-icon {
      width: 1.25rem;
      height: 1.25rem;
      color: #ffffff;
    }

    .current-dot {
      width: 0.75rem;
      height: 0.75rem;
      background-color: #ffffff;
      border-radius: 50%;
      display: block;
    }

    .connector-line {
      width: 2px;
      flex-grow: 1;
      min-height: 1.5rem;
      margin-top: 0.25rem;
      margin-bottom: 0.25rem;
    }

    .timeline-item.completed .connector-line {
      background-color: #10b981;
    }

    .timeline-item.current .connector-line,
    .timeline-item.pending .connector-line,
    .timeline-item.rejected .connector-line {
      background-color: #d1d5db;
    }

    .timeline-content {
      display: flex;
      align-items: center;
      padding: 0.5rem 0;
      flex-grow: 1;
    }

    .state-label {
      font-size: 0.875rem;
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .timeline-item.completed .state-label {
      color: #10b981;
    }

    .timeline-item.current .state-label {
      color: #3b82f6;
      font-weight: 600;
    }

    .timeline-item.rejected .state-label {
      color: #ef4444;
      font-weight: 600;
    }

    .timeline-item.pending .state-label {
      color: #9ca3af;
    }
  `]
})
export class WorkflowStateTimelineComponent {
  @Input() currentState!: ApprovalStatus;

  // Define the workflow progression order: Pending -> Approved
  private readonly workflowOrder: ApprovalStatus[] = [
    'Pending',
    'Approved'
  ];

  get timelineStates(): TimelineState[] {
    const currentIndex = this.getCurrentStateIndex();
    
    return this.workflowOrder.map((status, index) => {
      const isCompleted = index < currentIndex;
      const isCurrent = index === currentIndex && this.currentState !== 'Rejected';
      const isPending = index > currentIndex;

      return {
        status,
        label: this.formatStateLabel(status),
        isCompleted,
        isCurrent,
        isPending
      };
    });
  }

  private getCurrentStateIndex(): number {
    if (this.currentState === 'Rejected') {
      return 0; // Show Rejected at Pending stage
    }
    
    const index = this.workflowOrder.indexOf(this.currentState);
    return index !== -1 ? index : 0;
  }

  private formatStateLabel(state: ApprovalStatus): string {
    if (state === 'Pending' && this.currentState === 'Rejected') return 'Rejected';
    return state;
  }
}
