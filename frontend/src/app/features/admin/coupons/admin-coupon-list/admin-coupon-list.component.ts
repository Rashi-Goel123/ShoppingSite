import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DatePipe, DecimalPipe } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-admin-coupon-list',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './admin-coupon-list.component.html'
})
export class AdminCouponListComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);

  coupons = signal<any[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.loadCoupons();
  }

  loadCoupons() {
    this.loading.set(true);
    this.http.get<any[]>(`${environment.apiUrl}/admin/coupons`).subscribe({
      next: (res) => {
        this.coupons.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load coupons');
        this.loading.set(false);
      }
    });
  }

  deleteCoupon(id: number, code: string) {
    if (confirm(`Are you sure you want to delete coupon ${code}?`)) {
      this.http.delete(`${environment.apiUrl}/admin/coupons/${id}`).subscribe({
        next: () => {
          this.toast.success('Coupon deleted');
          this.loadCoupons();
        },
        error: () => this.toast.error('Failed to delete coupon')
      });
    }
  }

  addCoupon() {
    // For now, prompt-based simple addition, normally a form
    const code = prompt('Enter Coupon Code (e.g. SUMMER50):');
    if (!code) return;
    
    const type = prompt('Type (flat/percentage):', 'percentage');
    const value = prompt('Discount Value (e.g. 50):', '50');
    
    if (code && type && value) {
      const payload = {
        code: code.toUpperCase(),
        type: type,
        value: Number(value),
        minOrderAmount: 0,
        maxDiscount: 1000,
        usageLimit: 100,
        validFrom: new Date().toISOString(),
        validUntil: new Date(Date.now() + 30*24*60*60*1000).toISOString()
      };
      
      this.http.post(`${environment.apiUrl}/admin/coupons`, payload).subscribe({
        next: () => {
          this.toast.success('Coupon created successfully!');
          this.loadCoupons();
        },
        error: () => this.toast.error('Failed to create coupon')
      });
    }
  }
}
