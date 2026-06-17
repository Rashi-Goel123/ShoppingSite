import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-admin-product-list',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './admin-product-list.component.html'
})
export class AdminProductListComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);

  products = signal<any[]>([]);
  loading = signal(true);
  totalCount = signal(0);
  page = signal(1);

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.loading.set(true);
    this.http.get<any>(`${environment.apiUrl}/admin/products?page=${this.page()}&pageSize=20`).subscribe({
      next: (res) => {
        this.products.set(res.items);
        this.totalCount.set(res.total);
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load products');
        this.loading.set(false);
      }
    });
  }

  deleteProduct(id: number, title: string) {
    if (confirm(`Are you sure you want to delete ${title}?`)) {
      this.http.delete(`${environment.apiUrl}/admin/products/${id}`).subscribe({
        next: () => {
          this.toast.success('Product deleted');
          this.loadProducts();
        },
        error: () => this.toast.error('Failed to delete product')
      });
    }
  }

  nextPage() {
    this.page.update(p => p + 1);
    this.loadProducts();
  }

  prevPage() {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.loadProducts();
    }
  }
}
