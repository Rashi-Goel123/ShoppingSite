import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { CartItem } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private _items = signal<CartItem[]>([]);

  items = this._items.asReadonly();
  itemCount = computed(() => this._items().reduce((sum, i) => sum + i.quantity, 0));
  subtotal = computed(() => this._items().reduce((sum, i) => sum + i.price * i.quantity, 0));
  totalMrp = computed(() => this._items().reduce((sum, i) => sum + i.mrp * i.quantity, 0));
  savings = computed(() => this.totalMrp() - this.subtotal());

  isDrawerOpen = signal(false);
  appliedCoupon = signal<any | null>(null);
  couponDiscount = signal<number>(0);

  constructor(private http: HttpClient) { }

  openDrawer() { this.isDrawerOpen.set(true); }
  closeDrawer() { this.isDrawerOpen.set(false); }
  toggleDrawer() { this.isDrawerOpen.update(v => !v); }

  loadCart() {
    this.http.get<CartItem[]>(`${environment.apiUrl}/cart`).subscribe({
      next: items => this._items.set(items),
      error: () => this._items.set([])
    });
  }

  addToCart(productId: number, variantId: number, quantity = 1) {
    return this.http.post<{ message: string }>(
      `${environment.apiUrl}/cart/add`,
      { productId, variantId, quantity }
    );
  }

  updateQuantity(cartItemId: number, quantity: number) {
    return this.http.put<{ message: string }>(
      `${environment.apiUrl}/cart/update`,
      { cartItemId, quantity }
    );
  }

  removeItem(id: number) {
    return this.http.delete<{ message: string }>(`${environment.apiUrl}/cart/remove/${id}`);
  }

  clearCart() {
    return this.http.delete<{ message: string }>(`${environment.apiUrl}/cart/clear`);
  }
  setItems(items: CartItem[]) {
    this._items.set(items);
  }

  addItemLocally(item: CartItem) {
    const existing = this._items().find(i => i.variantId === item.variantId);
    if (existing) {
      this._items.update(items =>
        items.map(i => i.variantId === item.variantId
          ? { ...i, quantity: Math.min(i.quantity + item.quantity, i.stock) }
          : i
        )
      );
    } else {
      this._items.update(items => [...items, item]);
    }
  }

  updateQuantityLocally(variantId: number, quantity: number) {
    if (quantity <= 0) {
      this._items.update(items => items.filter(i => i.variantId !== variantId));
    } else {
      this._items.update(items =>
        items.map(i => i.variantId === variantId ? { ...i, quantity } : i)
      );
    }
  }

  removeItemLocally(variantId: number) {
    this._items.update(items => items.filter(i => i.variantId !== variantId));
  }

  clearLocally() {
    this._items.set([]);
    this.appliedCoupon.set(null);
    this.couponDiscount.set(0);
  }
}
