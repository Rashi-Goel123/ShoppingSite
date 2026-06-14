import { Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-toast',
  standalone: true,
  templateUrl: './toast.component.html',
  styles: [`
    .toast-container {
      position: fixed;
      top: 40px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 12px;
      pointer-events: none;
    }
    .toast-item {
      pointer-events: auto;
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px 24px;
      background: rgba(19, 27, 47, 0.95);
      backdrop-filter: blur(12px);
      border: 1px solid var(--border-color);
      border-radius: var(--radius-lg);
      box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5);
      min-width: 320px;
      max-width: 450px;
      color: white;
      position: relative;
      overflow: hidden;
    }
    .toast-item::before {
      content: '';
      position: absolute;
      left: 0; top: 0; bottom: 0;
      width: 5px;
    }
    .toast-item.success::before { background: var(--success); }
    .toast-item.error::before { background: var(--danger); }
    .toast-item.info::before { background: var(--primary); }
    
    .toast-icon {
      font-size: 1.4rem;
      font-weight: bold;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 28px;
      height: 28px;
      border-radius: 50%;
    }
    .success .toast-icon { color: var(--success); background: rgba(46, 160, 67, 0.15); }
    .error .toast-icon { color: var(--danger); background: rgba(248, 81, 73, 0.15); }
    .info .toast-icon { color: var(--primary); background: rgba(43, 132, 234, 0.15); }
    
    .toast-message {
      flex: 1;
      font-size: 0.98rem;
      font-weight: 500;
      letter-spacing: 0.3px;
    }
    .toast-close {
      background: transparent;
      border: none;
      color: var(--text-muted);
      font-size: 1.6rem;
      cursor: pointer;
      padding: 0 4px;
      line-height: 1;
      transition: var(--transition);
    }
    .toast-close:hover { color: white; transform: scale(1.1); }
  `],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateY(-40px) scale(0.95)', opacity: 0 }),
        animate('300ms cubic-bezier(0.175, 0.885, 0.32, 1.275)', style({ transform: 'translateY(0) scale(1)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('250ms ease-in', style({ transform: 'translateY(-40px) scale(0.95)', opacity: 0 }))
      ])
    ])
  ]
})
export class ToastComponent {
  toastService = inject(ToastService);
}
