import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { ProductListItem } from '../../../core/models/models';
import { environment } from '../../../../environments/environment';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cart-drawer.component.html',
  styles: [`
    /* ── Cinematic backdrop ── */
    .drawer-backdrop {
      position: fixed; inset: 0;
      background: rgba(2, 4, 14, 0.78);
      backdrop-filter: blur(22px) saturate(0.45);
      -webkit-backdrop-filter: blur(22px) saturate(0.45);
      z-index: 1040;
      animation: fadeIn 0.3s ease forwards;
    }
    @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

    /* ── Floating glass panel ── */
    .cart-drawer {
      position: fixed;
      top: 10px; right: 10px;
isolation: isolate;
      width: 660px;
      height: calc(100vh - 20px);
      border-radius: 20px;
      right:20px;
      background: linear-gradient(160deg,
        rgba(14, 16, 42, 0.99) 0%,
        rgba(9, 11, 28, 1) 55%,
        rgba(7, 8, 22, 1) 100%);
      backdrop-filter: blur(50px) saturate(1.5);
      -webkit-backdrop-filter: blur(50px) saturate(1.5);
      border: 1px solid rgba(99, 102, 241, 0.6);
  box-shadow:
  0 0 0 1px rgba(56,189,248,.25),
  -30px 0 80px rgba(59,130,246,.45),
  30px 0 80px rgba(59,130,246,.55),
  80px 0 180px rgba(59,130,246,.40),
  0 0 220px rgba(59,130,246,.28),
  0 0 300px rgba(139,92,246,.22),
  0 40px 120px rgba(0,0,0,.75);
      z-index: 1050;
      transform: translateX(calc(100% + 12px));
      transition: transform 0.45s cubic-bezier(0.16, 1, 0.3, 1);
      display: flex; flex-direction: column;
      overflow: visible;
    }
      .cart-drawer::before {
  content: '';
  position: absolute;
  inset: -2px;
  border-radius: 22px;
  pointer-events: none;

  border: 1px solid rgba(59,130,246,.35);

  box-shadow:
    0 0 15px rgba(59,130,246,.6),
    0 0 30px rgba(59,130,246,.4),
    0 0 60px rgba(139,92,246,.25);
}
    .cart-drawer::after{
  content:'';
  position:absolute;
  top:-80px;
  right:-220px;
  width:320px;
  height:calc(100% + 160px);
  pointer-events:none;
  z-index:-1;

  background:
    radial-gradient(circle at 30% 20%,
      rgba(59,130,246,.55) 0%,
      rgba(59,130,246,.20) 30%,
      transparent 65%),

    radial-gradient(circle at 70% 50%,
      rgba(139,92,246,.45) 0%,
      rgba(139,92,246,.18) 30%,
      transparent 70%),

    radial-gradient(circle at 50% 85%,
      rgba(96,165,250,.45) 0%,
      rgba(96,165,250,.12) 35%,
      transparent 70%);

  filter: blur(45px);
  animation: floatingGlow 8s ease-in-out infinite;
}
@keyframes floatingGlow{
  0%,100%{
    transform: translateY(0px) scale(1);
    opacity:.85;
  }
  35%{
    transform: translateY(-45px) scale(1.02);
    opacity:.95;
  }
  60%{
    transform: translateY(10px) scale(.96);
    opacity:.7;
  }
}

@keyframes sparkleFloat {
  from {  
    transform: translateY(0);
  }
  to {
    transform: translateY(-80px);
  }
}
    .cart-drawer.open { transform: translateX(0); }

    /* ── Header ── */
    .drawer-header {
      padding: 20px 22px 18px;
      border-bottom: 1px solid rgba(99, 102, 241, 0.15);
      display: flex; justify-content: space-between; align-items: center;
      background: linear-gradient(180deg,
        rgba(99, 102, 241, 0.10) 0%,
        rgba(79, 70, 229, 0.04) 60%,
        transparent 100%);
      flex-shrink: 0;
    }
    .header-left { display: flex; align-items: center; gap: 12px; }
    .header-icon {
      width: 40px; height: 40px; border-radius: 12px; flex-shrink: 0;
      background: linear-gradient(135deg, rgba(124,58,237,0.35), rgba(79,70,229,0.2));
      border: 1px solid rgba(139,92,246,0.4);
      display: flex; align-items: center; justify-content: center;
      font-size: 1.05rem; color: #c4b5fd;
      box-shadow: 0 2px 12px rgba(124,58,237,0.25);
    }
    .header-title-wrap {}
    .header-title {
      display: flex; align-items: center; gap: 8px;
      font-size: 1.1rem; font-weight: 800; color: #f0eeff; letter-spacing: -0.02em;
      line-height: 1.2;
    }
    .header-count {
      background: linear-gradient(135deg, #7c3aed, #4338ca);
      color: white; font-size: 0.72rem; font-weight: 700;
      padding: 2px 9px; border-radius: 20px;
      box-shadow: 0 2px 8px rgba(124,58,237,0.45);
    }
    .header-sub {
      display: block; font-size: 0.75rem; color: #6b7280;
      margin-top: 1px; font-weight: 400;
    }
    .close-btn {
      background: rgba(255,255,255,0.05);
      border: 1px solid rgba(255,255,255,0.1);
      color: #9ca3af; width: 32px; height: 32px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      cursor: pointer; font-size: 1rem; flex-shrink: 0;
      transition: all 0.22s cubic-bezier(0.4,0,0.2,1);
    }
    .close-btn:hover {
      background: rgba(239,68,68,0.15); color: #f87171;
      border-color: rgba(239,68,68,0.3);
      transform: rotate(90deg) scale(1.1);
    }

    /* ── Scrollable body ── */
    .drawer-body {
      flex: 1; overflow-y: auto; padding: 18px 20px;
      display: flex; flex-direction: column; gap: 16px;
      scroll-behavior: smooth;
    }
    .drawer-body::-webkit-scrollbar { width: 3px; }
    .drawer-body::-webkit-scrollbar-track { background: transparent; }
    .drawer-body::-webkit-scrollbar-thumb {
      background: rgba(139,92,246,0.35); border-radius: 2px;
    }

    /* ── Empty state ── */
    .empty-state {
      flex: 1; display: flex; flex-direction: column;
      align-items: center; justify-content: center;
      text-align: center; padding: 40px 20px;
    }
    .empty-icon-wrap {
      width: 76px; height: 76px; border-radius: 22px;
      background: linear-gradient(135deg, rgba(124,58,237,0.18), rgba(79,70,229,0.08));
      border: 1px solid rgba(139,92,246,0.22);
      display: flex; align-items: center; justify-content: center;
      font-size: 2rem; color: #a78bfa; margin: 0 auto 18px;
      animation: float 3s ease-in-out infinite;
    }
    @keyframes float {
      0%, 100% { transform: translateY(0); }
      50% { transform: translateY(-7px); }
    }

    /* ── Free shipping progress ── */
    .shipping-card {
      background: rgba(99, 102, 241, 0.06);
      border: 1px solid rgba(99, 102, 241, 0.18);
      border-radius: 12px; padding: 12px 14px;
    }
    .shipping-text-row {
      display: flex; justify-content: space-between;
      align-items: center; font-size: 0.8rem;
    }
    .shipping-text-row .label { color: #b4b8d8; }
    .shipping-text-row .label strong { color: #e0ddff; }
    .shipping-text-row .amount { font-size: 0.72rem; color: #7c6fcd; font-weight: 600; }
    .shipping-unlocked {
      font-size: 0.82rem; font-weight: 600; color: #34d399;
      display: flex; align-items: center; gap: 6px;
    }
    .progress-track {
      height: 5px; background: rgba(255,255,255,0.07);
      border-radius: 3px; margin-top: 9px; overflow: hidden;
    }
    .progress-fill {
      height: 100%;
      background: linear-gradient(90deg, #6366f1 0%, #38bdf8 100%);
      border-radius: 3px;
      transition: width 0.55s cubic-bezier(0.4,0,0.2,1);
      box-shadow: 0 0 10px rgba(99,102,241,0.6);
      position: relative;
    }
    .progress-fill::after {
      content: ''; position: absolute; right: -1px; top: -2px; bottom: -2px; width: 7px;
      background: white; border-radius: 50%; opacity: 0.55;
    }

    /* ── Cart item cards ── */
    .items-list { display: flex; flex-direction: column; gap: 12px; }
    .cart-item {
      display: flex; gap: 14px; padding: 14px;
      background: rgba(255,255,255,0.03);
      border: 1px solid rgba(255,255,255,0.07);
      border-radius: 16px;
      transition: all 0.25s cubic-bezier(0.4,0,0.2,1);
      position: relative;
    }
    .cart-item:hover {
      background: rgba(99,102,241,0.07);
      border-color: rgba(99,102,241,0.35);
      box-shadow: 0 6px 24px rgba(99,102,241,0.14);
      transform: translateY(-2px);
    }
    .item-img-wrap {
      position: relative; width: 106px; height: 130px; flex-shrink: 0;
      border-radius: 12px; overflow: hidden;
      border: 1px solid rgba(255,255,255,0.1);
      background: #fff;
      box-shadow: 0 4px 14px rgba(0,0,0,0.3);
    }
    .item-img {
      width: 100%; height: 100%; object-fit: cover;
      transition: transform 0.35s ease;
    }
    .cart-item:hover .item-img { transform: scale(1.06); }
    .disc-badge {
      position: absolute; top: 7px; left: 7px;
      background: linear-gradient(135deg, #ef4444, #b91c1c);
      color: white; font-size: 0.6rem; font-weight: 800;
      padding: 2px 7px; border-radius: 5px; z-index: 2;
      letter-spacing: 0.03em;
      box-shadow: 0 2px 6px rgba(239,68,68,0.45);
    }
    .item-body {
      flex: 1; display: flex; flex-direction: column; min-width: 0;
    }
    .item-title {
      font-size: 0.9rem; font-weight: 700; color: #eeeaff;
      margin: 0 0 7px; line-height: 1.3; letter-spacing: -0.01em;
      display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
    }
    .variant-row { display: flex; flex-wrap: wrap; gap: 5px; margin-bottom: 7px; }
    .vtag {
      font-size: 0.67rem; font-weight: 500; color: #9ca3af;
      background: rgba(255,255,255,0.06);
      border: 1px solid rgba(255,255,255,0.09);
      padding: 2px 9px; border-radius: 5px;
    }
    .rating-row {
      display: flex; align-items: center; gap: 2px;
      font-size: 0.68rem; color: #fbbf24; margin-bottom: 7px;
    }
    .rating-row .rv { color: #9ca3af; margin-left: 3px; font-size: 0.66rem; }
    .price-row-item { display: flex; align-items: baseline; gap: 8px; margin-bottom: 10px; }
    .price-now { font-weight: 800; color: #e0ddff; font-size: 1.05rem; }
    .price-mrp { font-size: 0.8rem; color: #4b5563; text-decoration: line-through; }
    .item-bottom { display: flex; justify-content: space-between; align-items: center; margin-top: auto; }
    .qty-pill {
      display: flex; align-items: center; gap: 14px;
      background: rgba(255,255,255,0.05);
      border: 1px solid rgba(255,255,255,0.1);
      border-radius: 24px; padding: 5px 14px;
    }
    .qty-btn {
      background: transparent; border: none; color: #d1d5db;
      font-size: 1rem; cursor: pointer;
      display: flex; align-items: center; justify-content: center;
      width: 20px; height: 20px; border-radius: 50%;
      transition: all 0.18s ease;
    }
    .qty-btn:hover:not(:disabled) {
      color: #a78bfa; background: rgba(167,139,250,0.14); transform: scale(1.2);
    }
    .qty-btn:disabled { color: #374151; cursor: not-allowed; }
    .qty-num { font-size: 0.9rem; font-weight: 800; color: #f0eeff; min-width: 16px; text-align: center; }
    .del-btn {
      background: rgba(239,68,68,0.09);
      border: 1px solid rgba(239,68,68,0.2);
      color: #f87171; width: 32px; height: 32px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center;
      cursor: pointer; font-size: 0.82rem;
      transition: all 0.22s cubic-bezier(0.4,0,0.2,1);
    }
    .del-btn:hover {
      background: #ef4444; color: white; border-color: #ef4444;
      box-shadow: 0 0 14px rgba(239,68,68,0.4);
      transform: scale(1.1) rotate(8deg);
    }

    /* ── Recommendations — horizontal scroll ── */
    .rec-section { }
    .rec-header {
      display: flex; justify-content: space-between; align-items: center;
      margin-bottom: 12px;
    }
    .rec-label {
      font-size: 0.78rem; font-weight: 700; letter-spacing: 0.06em;
      text-transform: uppercase;
      background: linear-gradient(90deg, #a78bfa 0%, #60a5fa 100%);
      -webkit-background-clip: text; -webkit-text-fill-color: transparent;
      background-clip: text;
      display: flex; align-items: center; gap: 6px;
    }
    .view-all-btn {
      background: transparent; border: none; cursor: pointer;
      font-size: 0.75rem; color: #6366f1; font-weight: 600;
      transition: color 0.2s;
      display: flex; align-items: center; gap: 2px;
    }
    .view-all-btn:hover { color: #818cf8; }
    .rec-scroll {
      display: flex; gap: 10px; overflow-x: auto;
      padding-bottom: 4px; scrollbar-width: none;
    }
    .rec-scroll::-webkit-scrollbar { display: none; }
    .rec-card {
      min-width: 148px; max-width: 148px; flex-shrink: 0;
      background: rgba(255,255,255,0.03);
      border: 1px solid rgba(255,255,255,0.07);
      border-radius: 12px; overflow: hidden;
      cursor: pointer; transition: all 0.22s ease;
      display: flex; flex-direction: column;
    }
    .rec-card:hover {
      border-color: rgba(99,102,241,0.35);
      transform: translateY(-3px);
      box-shadow: 0 8px 24px rgba(99,102,241,0.18);
    }
    .rec-img-wrap {
      width: 100%; height: 90px; overflow: hidden;
      background: rgba(255,255,255,0.04);
    }
    .rec-img { width: 100%; height: 100%; object-fit: cover; }
    .rec-info { padding: 9px 10px 10px; flex: 1; display: flex; flex-direction: column; }
    .rec-brand {
      font-size: 0.6rem; color: #6b7280; text-transform: uppercase;
      letter-spacing: 0.07em; margin-bottom: 3px;
    }
    .rec-title {
      font-size: 0.78rem; font-weight: 600; color: #d1d5db;
      margin-bottom: 5px; line-height: 1.3;
      display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
    }
    .rec-prices { display: flex; align-items: center; gap: 5px; margin-bottom: 8px; }
    .rec-price { font-size: 0.78rem; font-weight: 700; color: #a5b4fc; }
    .rec-mrp { font-size: 0.68rem; color: #4b5563; text-decoration: line-through; }
    .rec-add-btn {
      background: rgba(99,102,241,0.12);
      border: 1px solid rgba(99,102,241,0.3);
      color: #818cf8; font-size: 0.72rem; font-weight: 700;
      padding: 5px 0; border-radius: 7px; width: 100%;
      cursor: pointer; transition: all 0.2s ease;
      display: flex; align-items: center; justify-content: center; gap: 4px;
    }
    .rec-add-btn:hover {
      background: rgba(99,102,241,0.22); color: #c7d2fe;
      border-color: rgba(99,102,241,0.5);
    }

    /* ── Delivery banner ── */
    .delivery-banner {
      display: flex; justify-content: space-between; align-items: center;
      background: rgba(52,211,153,0.05);
      border: 1px solid rgba(52,211,153,0.18);
      border-radius: 12px; padding: 12px 14px;
    }
    .delivery-left { display: flex; align-items: center; gap: 10px; }
    .delivery-left i { font-size: 1.2rem; color: #34d399; flex-shrink: 0; }
    .delivery-name { font-size: 0.82rem; font-weight: 700; color: #34d399; }
    .delivery-date { font-size: 0.72rem; color: #6b7280; margin-top: 1px; }
    .delivery-banner .gift-icon { font-size: 1.2rem; color: #6b7280; opacity: 0.6; }

    /* ── Sticky footer ── */
    .drawer-footer {
      flex-shrink: 0;
      border-top: 1px solid rgba(99,102,241,0.15);
      background: linear-gradient(180deg,
        rgba(11,13,32,0.98) 0%,
        rgba(7,9,22,1) 100%);
      backdrop-filter: blur(24px);
      box-shadow: 0 -1px 0 rgba(99,102,241,0.18), 0 -10px 30px rgba(0,0,0,0.2);
      padding: 16px 20px 20px;
    }
    .order-summary-title {
      font-size: 0.88rem; font-weight: 800; color: #e0ddff;
      letter-spacing: -0.01em; margin-bottom: 10px;
    }
    .price-row {
      display: flex; justify-content: space-between; align-items: center;
      font-size: 0.82rem; color: #8b949e; padding: 3px 0;
    }
    .price-row .val { font-weight: 500; color: #c9d1d9; }
    .price-row.savings { }
    .price-row.savings .val { color: #f87171; font-weight: 700; }
    .price-row.free-ship .val { color: #34d399; font-weight: 700; }
    .total-row {
      display: flex; justify-content: space-between; align-items: center;
      padding: 10px 0 12px;
      border-top: 1px solid rgba(255,255,255,0.07);
      margin-top: 4px;
    }
    .total-label { font-size: 1rem; font-weight: 800; color: #f0eeff; }
    .total-val {
      font-size: 1.25rem; font-weight: 900; color: #f0eeff;
      letter-spacing: -0.02em;
    }

    /* Trust strip */
    .trust-strip {
      display: flex; gap: 0;
      border: 1px solid rgba(255,255,255,0.055);
      border-radius: 10px; overflow: hidden;
      background: rgba(255,255,255,0.018);
      margin-bottom: 14px;
    }
    .trust-item {
      flex: 1; display: flex; flex-direction: column; align-items: center;
      justify-content: center; gap: 4px; padding: 10px 6px;
      text-align: center; position: relative;
    }
    .trust-item:not(:last-child)::after {
      content: ''; position: absolute; right: 0; top: 18%; bottom: 18%;
      width: 1px; background: rgba(255,255,255,0.06);
    }
    .trust-item i { font-size: 1rem; color: #818cf8; }
    .trust-name { font-size: 0.62rem; color: #c9d1d9; font-weight: 700; line-height: 1.2; }
    .trust-sub { font-size: 0.56rem; color: #6b7280; line-height: 1.2; }

    /* Checkout button */
    .checkout-btn {
      width: 100%;
      background: linear-gradient(135deg, #7c3aed 0%, #5b21b6 45%, #4338ca 100%);
      border: none; color: white;
      font-size: 1rem; font-weight: 700; letter-spacing: 0.02em;
      padding: 15px 24px; border-radius: 13px;
      cursor: pointer;
      transition: all 0.28s cubic-bezier(0.4,0,0.2,1);
      display: flex; align-items: center; justify-content: center; gap: 8px;
      box-shadow: 0 4px 20px rgba(124,58,237,0.45), 0 0 0 1px rgba(139,92,246,0.3);
      position: relative; overflow: hidden;
      margin-bottom: 10px;
    }
    .checkout-btn::after {
      content: '';
      position: absolute; top: 0; left: -100%; width: 60%; height: 100%;
      background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
      transition: left 0.5s ease;
    }
    .checkout-btn:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 30px rgba(124,58,237,0.6),
                  0 0 60px rgba(99,102,241,0.22),
                  0 0 0 1px rgba(139,92,246,0.5);
    }
    .checkout-btn:hover::after { left: 140%; }
    .checkout-btn:active { transform: scale(0.985); }
    .checkout-meta {
      display: flex; align-items: center; justify-content: center; gap: 5px;
      font-size: 0.67rem; color: #6b7280;
    }
    .checkout-meta i { color: #7c6fcd; font-size: 0.7rem; }

    /* ── Responsive ── */
    @media (max-width: 520px) {
      .cart-drawer { width: calc(100vw - 12px); top: 6px; right: 6px; height: calc(100vh - 12px); }
      .drawer-body { padding: 14px 16px; }
      .drawer-footer { padding: 14px 16px 18px; }
      .item-img-wrap { width: 88px; height: 112px; }
    }
      
  `]
})
export class CartDrawerComponent implements OnInit {
  cart = inject(CartService);
  private productService = inject(ProductService);
  private router = inject(Router);

  featuredProducts = signal<ProductListItem[]>([]);

  recommendations = computed(() => {
    const inCart = new Set(this.cart.items().map(i => i.productId));
    return this.featuredProducts().filter(p => !inCart.has(p.id)).slice(0, 5);
  });

  ngOnInit() {
    this.productService.getFeatured().subscribe({
      next: p => this.featuredProducts.set(p),
      error: () => { }
    });
  }

  navigate(path: string) { this.router.navigate([path]); }

  navigateToProduct(slug: string) {
    this.cart.closeDrawer();
    this.router.navigate(['/products', slug]);
  }

  updateQty(variantId: number, qty: number) {
    if (environment.useMockData) {
      this.cart.updateQuantityLocally(variantId, qty);
    } else {
      const item = this.cart.items().find(i => i.variantId === variantId);
      if (item) this.cart.updateQuantity(item.id, qty).subscribe(() => this.cart.loadCart());
    }
  }

  removeItem(variantId: number) {
    if (environment.useMockData) {
      this.cart.removeItemLocally(variantId);
    } else {
      const item = this.cart.items().find(i => i.variantId === variantId);
      if (item) this.cart.removeItem(item.id).subscribe(() => this.cart.loadCart());
    }
  }

  getDiscountPct(price: number, mrp: number): number {
    if (!mrp || mrp <= price) return 0;
    return Math.round(((mrp - price) / mrp) * 100);
  }

  getRating(productId: number): number {
    return 4.0 + (productId % 11) * 0.1;
  }

  getStars(rating: number): number[] {
    return new Array(5).fill(0).map((_, i) => {
      if (i < Math.floor(rating)) return 1;
      if (i === Math.floor(rating) && rating % 1 >= 0.5) return 0.5;
      return 0;
    });
  }

  getShippingProgress(): number { return Math.min(100, (this.cart.subtotal() / 499) * 100); }
  getShippingShortage(): number { return Math.max(0, 499 - this.cart.subtotal()); }
  getDeliveryCharge(): number { return this.cart.subtotal() >= 499 ? 0 : 49; }
  getFinalTotal(): number { return this.cart.subtotal() + this.getDeliveryCharge(); }

  getDeliveryDate(): string {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return d.toLocaleDateString('en-IN', { weekday: 'long', day: 'numeric', month: 'long' });
  }
}
