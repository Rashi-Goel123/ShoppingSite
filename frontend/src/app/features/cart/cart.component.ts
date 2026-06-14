import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-cart',
  imports: [RouterLink],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css'
})
export class CartComponent {
  cart = inject(CartService);

  updateQty(variantId: number, qty: number) {
    if (environment.useMockData) {
      this.cart.updateQuantityLocally(variantId, qty);
    } else {
      this.cart.updateQuantity(variantId, qty).subscribe(() => this.cart.loadCart());
    }
  }

  removeItem(variantId: number) {
    if (environment.useMockData) {
      this.cart.removeItemLocally(variantId);
    } else {
      this.cart.removeItem(variantId).subscribe(() => this.cart.loadCart());
    }
  }
}
