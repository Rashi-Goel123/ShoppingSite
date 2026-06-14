import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { OrderService } from '../../../core/services/order.service';
import { OrderListItem } from '../../../core/models/models';

@Component({
  selector: 'app-order-history',
  imports: [RouterLink, DatePipe],
  templateUrl: './order-history.component.html',
  styleUrl: './order-history.component.css'
})
export class OrderHistoryComponent implements OnInit {
  private orderService = inject(OrderService);
  orders = signal<OrderListItem[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.orderService.getOrders().subscribe({
      next: (o) => { this.orders.set(o); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
