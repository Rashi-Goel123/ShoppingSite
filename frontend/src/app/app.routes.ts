import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'admin/login', loadComponent: () => import('./features/auth/admin-login/admin-login.component').then(m => m.AdminLoginComponent) },
  { path: 'admin/register', loadComponent: () => import('./features/auth/admin-register/admin-register.component').then(m => m.AdminRegisterComponent) },
  { path: 'products', loadComponent: () => import('./features/products/product-list/product-list.component').then(m => m.ProductListComponent) },
  { path: 'products/:slug', loadComponent: () => import('./features/products/product-detail/product-detail.component').then(m => m.ProductDetailComponent) },
  { path: 'cart', loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent), canActivate: [authGuard] },
  { path: 'checkout', loadComponent: () => import('./features/checkout/checkout.component').then(m => m.CheckoutComponent), canActivate: [authGuard] },
  { path: 'orders', loadComponent: () => import('./features/orders/order-history/order-history.component').then(m => m.OrderHistoryComponent), canActivate: [authGuard] },
  { path: 'orders/:id', loadComponent: () => import('./features/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent), canActivate: [authGuard] },
  { path: 'order-success/:id', loadComponent: () => import('./features/orders/order-success/order-success.component').then(m => m.OrderSuccessComponent), canActivate: [authGuard] },
  { path: 'profile', loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent), canActivate: [authGuard] },
  { path: 'wishlist', loadComponent: () => import('./features/wishlist/wishlist.component').then(m => m.WishlistComponent), canActivate: [authGuard] },
  { path: 'admin', loadComponent: () => import('./features/admin/dashboard/dashboard.component').then(m => m.DashboardComponent), canActivate: [adminGuard] },
  { path: '**', redirectTo: '' }
];
