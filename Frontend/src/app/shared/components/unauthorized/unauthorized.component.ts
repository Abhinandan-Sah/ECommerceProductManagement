import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';

/**
 * UnauthorizedComponent displays an access denied message.
 * 
 * Shown when users attempt to access routes they don't have permission for.
 * 
 * Requirements: 8.5
 */
@Component({
    selector: 'app-unauthorized',
    imports: [CommonModule, RouterModule],
    template: `
    <div class="unauthorized-container">
      <div class="unauthorized-card">
        <div class="icon">🔒</div>
        <h1>Access Denied</h1>
        <p>You don't have permission to access this page.</p>
        <p class="hint">Please contact your administrator if you believe this is an error.</p>
        <button (click)="goHome()" class="btn btn-primary">
          Go to Home
        </button>
      </div>
    </div>
  `,
    styles: [`
    .unauthorized-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      padding: 1rem;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    }

    .unauthorized-card {
      background: white;
      border-radius: 8px;
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
      padding: 3rem 2rem;
      text-align: center;
      max-width: 500px;
    }

    .icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    h1 {
      font-size: 2rem;
      font-weight: 700;
      color: #1a202c;
      margin: 0 0 1rem 0;
    }

    p {
      color: #718096;
      font-size: 1rem;
      margin: 0 0 0.5rem 0;
      line-height: 1.5;

      &.hint {
        font-size: 0.875rem;
        margin-bottom: 2rem;
      }
    }

    .btn {
      padding: 0.875rem 2rem;
      font-size: 1rem;
      font-weight: 600;
      border: none;
      border-radius: 6px;
      cursor: pointer;
      transition: all 0.2s;

      &.btn-primary {
        background: #667eea;
        color: white;

        &:hover {
          background: #5a67d8;
          transform: translateY(-1px);
          box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
        }
      }
    }
  `]
})
export class UnauthorizedComponent {
  constructor(private router: Router) {}

  goHome(): void {
    this.router.navigate(['/']);
  }
}
