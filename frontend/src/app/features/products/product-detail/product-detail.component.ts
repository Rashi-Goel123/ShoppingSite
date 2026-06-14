import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProductDetail, ProductVariant } from '../../../core/models/models';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-product-detail',
  imports: [RouterLink],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'
})
export class ProductDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private productService = inject(ProductService);
  private cartService = inject(CartService);
  toastService = inject(ToastService);

  product = signal<ProductDetail | null>(null);
  loading = signal(true);
  selectedImage = signal('');
  selectedColor = signal('');
  selectedSize = signal('');
  selectedVariant = signal<ProductVariant | null>(null);
  addedToCart = signal(false);
  colors = signal<string[]>([]);
  sizes = signal<string[]>([]);

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const slug = params.get('slug');
      if (slug) {
        this.productService.getBySlug(slug).subscribe({
          next: (p) => {
            this.product.set(p);
            this.selectedImage.set(p.images[0]?.url || '');
            const colorList = [...new Set(p.variants.map(v => v.color).filter(Boolean) as string[])];
            this.colors.set(colorList);
            if (colorList.length > 0) this.selectColor(colorList[0]);
            this.loading.set(false);
          },
          error: () => { this.loading.set(false); }
        });
      }
    });
  }

  selectColor(color: string) {
    this.selectedColor.set(color);
    const sizeList = [...new Set(
      this.product()!.variants.filter(v => v.color === color).map(v => v.size).filter(Boolean) as string[]
    )];
    this.sizes.set(sizeList);
    if (sizeList.length > 0) this.selectSize(sizeList[0]);
  }

  selectSize(size: string) {
    this.selectedSize.set(size);
    this.selectedVariant.set(this.product()!.variants.find(v => v.color === this.selectedColor() && v.size === size) || null);
  }

  getColorHex(color: string) {
    return this.product()?.variants.find(v => v.color === color)?.colorHex || '#888';
  }

  isLightColor(hex: string) {
    if (!hex) return false;
    const c = hex.replace('#', '');
    const r = parseInt(c.substring(0, 2), 16);
    const g = parseInt(c.substring(2, 4), 16);
    const b = parseInt(c.substring(4, 6), 16);
    return (r * 299 + g * 587 + b * 114) / 1000 > 200;
  }

  getStock(size: string) {
    return this.product()?.variants.find(v => v.color === this.selectedColor() && v.size === size)?.stock || 0;
  }

  getDiscount() {
    if (!this.selectedVariant()) return 0;
    return Math.round((1 - this.selectedVariant()!.price / this.selectedVariant()!.mrp) * 100);
  }

  addToCart() {
    const v = this.selectedVariant();
    const p = this.product();
    if (!v || !p) return;

    if (environment.useMockData) {
      this.cartService.addItemLocally({
        id: Date.now(), productId: p.id, variantId: v.id,
        title: p.title, image: p.images[0]?.url,
        color: v.color, size: v.size,
        price: v.price, mrp: v.mrp,
        quantity: 1, stock: v.stock
      });
      this.toastService.success('Product added to cart successfully!');
      this.cartService.openDrawer();
    } else {
      this.cartService.addToCart(p.id, v.id).subscribe({
        next: () => {
          this.cartService.loadCart();
          this.toastService.success('Product added to cart successfully!');
          this.cartService.openDrawer();
        },
        error: (err) => {
          if (err.status === 401) {
            this.toastService.error('Please login to continue');
            this.router.navigate(['/login']);
          } else {
            this.toastService.error('Failed to add to cart: ' + (err.error?.message || 'Unknown error'));
          }
        }
      });
    }
  }
}
