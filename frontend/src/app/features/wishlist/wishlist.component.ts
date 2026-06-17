import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserService } from '../../core/services/user.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { ProductListItem } from '../../core/models/models';

@Component({
  selector: 'app-wishlist',
  imports: [RouterLink],
  templateUrl: './wishlist.component.html'
})
export class WishlistComponent implements OnInit {
  private userService = inject(UserService);
  private cartService = inject(CartService);
  private toastService = inject(ToastService);

  items = signal<ProductListItem[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.loadWishlist();
  }

  loadWishlist() {
    this.loading.set(true);
    this.userService.getWishlist().subscribe({
      next: (products) => {
        this.items.set(products);
        this.loading.set(false);
      },
      error: () => {
        this.items.set([]);
        this.loading.set(false);
      }
    });
  }

  removeFromWishlist(productId: number) {
    this.userService.toggleWishlist(productId).subscribe({
      next: () => {
        this.items.update(list => list.filter(p => p.id !== productId));
        this.toastService.success('Removed from wishlist');
      },
      error: () => {
        this.toastService.error('Failed to remove item');
      }
    });
  }
}
