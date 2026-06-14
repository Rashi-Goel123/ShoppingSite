import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts = signal<Toast[]>([]);
  toasts = this._toasts.asReadonly();
  private idCounter = 0;

  show(message: string, type: 'success' | 'error' | 'info' = 'info') {
    const id = ++this.idCounter;
    this._toasts.update(t => [...t, { id, message, type }]);

    setTimeout(() => {
      this.remove(id);
    }, 3000);
  }

  success(message: string) { this.show(message, 'success'); }
  error(message: string) { this.show(message, 'error'); }
  info(message: string) { this.show(message, 'info'); }

  remove(id: number) {
    this._toasts.update(t => t.filter(toast => toast.id !== id));
  }
}
