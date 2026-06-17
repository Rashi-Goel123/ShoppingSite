import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cart',
  imports: [RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent implements OnInit {
  cart = inject(CartService);

  ngOnInit() {
    if (!environment.useMockData) {
      this.cart.loadCart();
    }
  }

  updateQty(item: any, qty: number) {
    if (environment.useMockData) {
      this.cart.updateQuantityLocally(item.variantId, qty);
    } else {
      this.cart.updateQuantity(item.id, qty).subscribe(() => this.cart.loadCart());
    }
  }

  removeItem(item: any) {
    if (environment.useMockData) {
      this.cart.removeItemLocally(item.variantId);
    } else {
      this.cart.removeItem(item.id).subscribe(() => this.cart.loadCart());
    }
  }
}
