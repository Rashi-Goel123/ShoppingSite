import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderDetail } from '../../../core/models/models';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-order-detail',
  imports: [RouterLink, DatePipe],
  templateUrl: './order-detail.component.html'
})
export class OrderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);
  order = signal<OrderDetail | null>(null);
  loading = signal(true);

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.orderService.getOrder(id).subscribe({
      next: (o) => { this.order.set(o); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  cancelOrder() {
    const o = this.order();
    if (!o || !confirm('Are you sure you want to cancel this order?')) return;
    this.orderService.cancelOrder(o.id).subscribe(() => {
      this.order.set({ ...o, status: 'cancelled' });
    });
  }
}
