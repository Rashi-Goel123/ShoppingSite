import { HttpInterceptorFn, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { of } from 'rxjs';
import { catchError, delay } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { MOCK_PRODUCTS, MOCK_PRODUCTS_DETAIL } from '../../mock-data/mock-products';
import { MOCK_CATEGORIES } from '../../mock-data/mock-categories';
import { MOCK_ORDERS } from '../../mock-data/mock-orders';
import { MOCK_COUPONS } from '../../mock-data/mock-coupons';

/**
 * Mock/Fallback interceptor.
 * - When useMockData is true: always returns mock data (skip real API).
 * - When useMockData is false: tries real API first, falls back to mock data on error.
 */
export const mockInterceptor: HttpInterceptorFn = (req, next) => {
  // If force-mock mode, return mock data directly
  if (environment.useMockData) {
    const mockResponse = getMockResponse(req);
    if (mockResponse) return mockResponse;
    return next(req);
  }

  // Normal mode: try real API first, fallback to mock on error
  // IMPORTANT: Do NOT catch 401/403 — those must propagate for proper auth handling
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 || error.status === 403) {
        // Auth errors must propagate — don't mask with mock data
        throw error;
      }
      console.warn(`[API Fallback] ${req.method} ${req.url} failed with ${error.status || 'network error'}, trying mock data...`);
      const mockResponse = getMockResponse(req);
      if (mockResponse) return mockResponse;
      // No mock available for this route — rethrow the original error
      throw error;
    })
  );
};

function getMockResponse(req: any) {
  const url = req.url.replace(environment.apiUrl, '');
  const queryStr = req.params.keys().length > 0 ? '?' + req.params.toString() : '';
  const fullUrl = url + queryStr;
  const method = req.method;

  // Payments always go to real API, no mock fallback
  if (url.startsWith('/payments/')) {
    return null;
  }

  const respond = (body: any, status = 200) =>
    of(new HttpResponse({ status, body })).pipe(delay(300));

  // === AUTH ===
  if (url === '/auth/send-otp' && method === 'POST')
    return respond({ message: 'OTP sent successfully', dev_otp: '123456' });
  if (url === '/auth/verify-otp' && method === 'POST')
    return respond({
      token: 'mock-jwt-token-xyz',
      user: { id: 1, phone: '9876543210', name: 'Fashion User', email: 'user@example.com', avatar: null, role: 'user' }
    });

  if (url === '/auth/admin-login' && method === 'POST')
    return respond({
      token: 'mock-admin-jwt-token',
      user: { id: 99, phone: '9999999999', name: 'Admin', email: 'admin@fashionstore.com', avatar: null, role: 'admin' }
    });

  if (url === '/auth/admin-register' && method === 'POST')
    return respond({
      token: 'mock-admin-jwt-token',
      user: { id: 99, phone: '9999999999', name: 'Admin', email: 'admin@fashionstore.com', avatar: null, role: 'admin' }
    });

  if (url === '/auth/me' && method === 'GET')
    return respond({ id: 1, phone: '9876543210', name: 'Fashion User', email: 'user@example.com', avatar: null, role: 'user' });

  // === PRODUCTS ===
  if (url === '/products/categories' && method === 'GET')
    return respond(MOCK_CATEGORIES);
  if (url === '/products/featured' && method === 'GET')
    return respond(MOCK_PRODUCTS.filter(p => p.isFeatured));
  if (url === '/products/new-arrivals' && method === 'GET')
    return respond(MOCK_PRODUCTS.slice(0, 8));

  if (url === '/products/brands' && method === 'GET')
    return respond([...new Set(MOCK_PRODUCTS.map(p => p.brand))]);

  if (url.match(/^\/products\/[a-z0-9-]+$/) && method === 'GET') {
    const slug = url.split('/').pop()!;
    let product = MOCK_PRODUCTS_DETAIL.find(p => p.slug === slug);

    if (!product && slug.includes('-ed-')) {
      const baseSlug = slug.split('-ed-')[0];
      const baseProduct = MOCK_PRODUCTS_DETAIL.find(p => p.slug === baseSlug);
      if (baseProduct) {
        product = JSON.parse(JSON.stringify(baseProduct));
        const listProduct = MOCK_PRODUCTS.find(p => p.slug === slug);
        if (listProduct && product) {
          product.id = listProduct.id;
          product.title = listProduct.title;
          product.slug = listProduct.slug;
          if (listProduct.firstImage && product.images && product.images.length > 0) {
            product.images[0].url = listProduct.firstImage;
          }
        }
      }
    }

    return product ? respond(product) : respond({ message: 'Not found' }, 404);
  }

  if (url.startsWith('/products') && !url.match(/^\/products\/[a-z0-9-]+$/) && method === 'GET' && !url.includes('/categories') && !url.includes('/featured') && !url.includes('/new-arrivals') && !url.includes('/brands')) {
    const urlObj = new URL('http://localhost' + fullUrl);
    const category = urlObj.searchParams.get('category');
    const gender = urlObj.searchParams.get('gender');
    const search = urlObj.searchParams.get('search');
    const sortBy = urlObj.searchParams.get('sortBy');
    const minPrice = urlObj.searchParams.get('minPrice');
    const maxPrice = urlObj.searchParams.get('maxPrice');

    let filtered = [...MOCK_PRODUCTS];

    if (category) {
      const targetSlug = category.toLowerCase();
      const parentCat = MOCK_CATEGORIES.find(c => c.slug === targetSlug);
      
      const getProductSlug = (catName: string) => catName.toLowerCase().replace(/ & /g, '-').replace(/ /g, '-');
      
      if (parentCat && parentCat.children) {
        const childSlugs = parentCat.children.map(c => c.slug);
        filtered = filtered.filter(p => {
          const productCatSlug = getProductSlug(p.categoryName);
          return productCatSlug === targetSlug || childSlugs.includes(productCatSlug);
        });
      } else {
        filtered = filtered.filter(p => getProductSlug(p.categoryName) === targetSlug);
      }
    }

    if (gender && gender !== 'all') {
      filtered = filtered.filter(p => p.gender.toLowerCase() === gender.toLowerCase());
    }

    if (search) {
      const q = search.toLowerCase();
      filtered = filtered.filter(p => p.title.toLowerCase().includes(q) || p.brand.toLowerCase().includes(q));
    }

    if (minPrice) {
      filtered = filtered.filter(p => (p.discountedPrice || p.basePrice) >= Number(minPrice));
    }

    if (maxPrice) {
      filtered = filtered.filter(p => (p.discountedPrice || p.basePrice) <= Number(maxPrice));
    }

    if (sortBy === 'price_asc') {
      filtered.sort((a, b) => (a.discountedPrice || a.basePrice) - (b.discountedPrice || b.basePrice));
    } else if (sortBy === 'price_desc') {
      filtered.sort((a, b) => (b.discountedPrice || b.basePrice) - (a.discountedPrice || a.basePrice));
    } else if (sortBy === 'rating') {
      filtered.sort((a, b) => b.ratingAverage - a.ratingAverage);
    } else {
      filtered.sort((a, b) => b.id - a.id);
    }

    const page = Number(urlObj.searchParams.get('page')) || 1;
    const pageSize = Number(urlObj.searchParams.get('pageSize')) || 12;
    const totalCount = filtered.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const paginatedItems = filtered.slice((page - 1) * pageSize, page * pageSize);

    return respond({ items: paginatedItems, totalCount, page, pageSize, totalPages });
  }

  // === CART ===
  if (url === '/cart' && method === 'GET')
    return respond([]);
  if (url === '/cart/add' && method === 'POST')
    return respond({ message: 'Added to cart' });
  if (url === '/cart/count' && method === 'GET')
    return respond({ count: 0 });

  // === COUPONS ===
  if (url === '/coupons/available' && method === 'GET')
    return respond(MOCK_COUPONS);
  if (url === '/coupons/validate' && method === 'POST') {
    const body = req.body as any;
    const coupon = MOCK_COUPONS.find((c: any) => c.code === body?.code?.toUpperCase());
    if (coupon)
      return respond({ isValid: true, message: `Coupon applied! You save ₹${coupon.value}`, discountAmount: coupon.value, code: coupon.code });
    return respond({ isValid: false, message: 'Invalid coupon', discountAmount: 0, code: body?.code });
  }

  // === USERS ===
  if (url === '/users/addresses' && method === 'GET')
    return respond([
      { id: 1, label: 'Home', street: '123 MG Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400001', isDefault: true },
      { id: 2, label: 'Work', street: '456 Brigade Road', city: 'Bangalore', state: 'Karnataka', pincode: '560001', isDefault: false }
    ]);
  if (url === '/users/addresses' && method === 'POST') {
    const body = req.body as any;
    return respond({ id: Date.now(), ...body });
  }
  if (url === '/users/profile' && method === 'PUT') {
    const body = req.body as any;
    return respond({ id: 1, phone: '9876543210', name: body?.name || 'User', email: body?.email, avatar: null, role: 'user' });
  }
  if (url.startsWith('/users/addresses/') && method === 'DELETE')
    return respond({ message: 'Deleted' });

  if (url === '/users/wishlist' && method === 'GET')
    return respond([]);
  if (url === '/users/wishlist/ids' && method === 'GET')
    return respond([]);
  if (url.match(/^\/users\/wishlist\/\d+$/) && method === 'POST')
    return respond({ added: true, message: 'Added to wishlist' });

  if (url === '/users/notifications' && method === 'GET')
    return respond([]);

  // === PAYMENTS ===
  if (url === '/payments/create-order' && method === 'POST') {
    const body = req.body as any;
    const amountInPaise = Math.round((body?.amount || 0) * 100);
    return respond({
      orderId: 'order_mock_' + Date.now(),
      amount: amountInPaise,
      currency: 'INR',
      keyId: environment.razorpayKeyId
    });
  }
  if (url === '/payments/verify' && method === 'POST')
    return respond({ verified: true, message: 'Payment verified successfully' });

  // === ORDERS ===
  if (url === '/orders' && method === 'POST') {
    const body = req.body as any;
    const newId = MOCK_ORDERS.length > 0 ? Math.max(...MOCK_ORDERS.map(o => o.id)) + 1 : 1;
    const newOrder = {
      id: newId,
      orderNumber: 'FS-' + Date.now().toString().slice(-8),
      userName: 'Mock User',
      status: 'placed',
      paymentStatus: body?.paymentMethod === 'cod' ? 'pending' : 'paid',
      totalAmount: body?.totalAmount || 0,
      itemCount: body?.items?.length || 0,
      firstItemImage: body?.items?.[0]?.image || 'https://via.placeholder.com/150',
      createdAt: new Date().toISOString(),
      paymentMethod: body?.paymentMethod || 'cod',
      subtotal: body?.totalAmount || 0,
      discount: 0,
      deliveryCharge: 0,
      couponCode: body?.couponCode,
      shippingAddress: { id: 1, label: 'Home', street: '123 MG Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400001', isDefault: true },
      items: body?.items?.map((item: any, i: number) => ({
        id: newId * 100 + i,
        productId: item.productId,
        title: item.title,
        image: item.image,
        sku: `SKU-${item.productId}-${item.variantId}`,
        color: item.color || 'N/A',
        size: item.size || 'N/A',
        priceAtPurchase: item.price,
        quantity: item.quantity
      })) || [],
      tracking: [
        { status: 'placed', description: 'Order placed successfully', timestamp: new Date().toISOString() }
      ],
      estimatedDelivery: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString()
    };
    MOCK_ORDERS.unshift(newOrder);
    return respond({
      orderId: newOrder.id,
      orderNumber: newOrder.orderNumber,
      message: 'Order placed successfully'
    });
  }
  if (url === '/orders' && method === 'GET')
    return respond(MOCK_ORDERS);
  if (url.match(/^\/orders\/\d+$/) && method === 'GET') {
    const id = parseInt(url.split('/').pop()!);
    return respond(MOCK_ORDERS.find((o: any) => o.id === id) || MOCK_ORDERS[0]);
  }

  if (url.match(/^\/orders\/\d+\/cancel$/) && method === 'POST')
    return respond({ message: 'Order cancelled successfully' });

  // === ADMIN ===
  if (url === '/admin/dashboard' && method === 'GET')
    return respond({
      totalRevenue: 125000, totalOrders: 48, totalUsers: 156, totalProducts: MOCK_PRODUCTS.length,
      pendingOrders: 12, deliveredOrders: 30,
      recentOrders: MOCK_ORDERS.slice(0, 5),
      topProducts: MOCK_PRODUCTS.slice(0, 5).map((p, i) => ({ productId: p.id, title: p.title, image: p.firstImage, totalSold: 50 - i * 8, totalRevenue: (50 - i * 8) * p.basePrice }))
    });

  // === ADMIN PRODUCTS ===
  if (url.startsWith('/admin/products') && !url.includes('/images') && method === 'GET') {
    return respond({
      items: MOCK_PRODUCTS,
      totalCount: MOCK_PRODUCTS.length,
      page: 1,
      pageSize: 20,
      totalPages: 1
    });
  }

  if (url === '/admin/products' && method === 'POST') {
    const body = req.body as any;
    const newId = MOCK_PRODUCTS.length > 0 ? Math.max(...MOCK_PRODUCTS.map(p => p.id)) + 1 : 1;
    const newProduct = {
      id: newId,
      title: body.title,
      slug: body.title.toLowerCase().replace(/[^a-z0-9]+/g, '-'),
      brand: body.brand || 'Generic',
      gender: body.gender || 'unisex',
      basePrice: body.basePrice,
      isFeatured: body.isFeatured || false,
      categoryName: 'Category ' + body.categoryId,
      firstImage: 'https://images.unsplash.com/photo-1555529733-0e67056058e1?w=600', // default image
      ratingAverage: 0,
      ratingCount: 0
    };
    MOCK_PRODUCTS.unshift(newProduct);
    return respond({ id: newProduct.id, message: 'Product created successfully' });
  }

  if (url.match(/^\/admin\/products\/\d+\/images$/) && method === 'POST') {
    return respond({
      id: Date.now(),
      url: 'https://images.unsplash.com/photo-1555529733-0e67056058e1?w=600',
      displayOrder: 0
    });
  }

  // === ADMIN COUPONS ===
  if (url === '/admin/coupons' && method === 'GET') {
    return respond([
      { id: 1, code: 'WELCOME50', type: 'flat', value: 50, minOrderAmount: 299, maxDiscount: 50, usageLimit: 1000, usedCount: 150, validFrom: new Date().toISOString(), validUntil: new Date(Date.now() + 90*24*60*60*1000).toISOString(), isActive: true },
      { id: 2, code: 'FLAT20', type: 'percentage', value: 20, minOrderAmount: 999, maxDiscount: 500, usageLimit: 500, usedCount: 42, validFrom: new Date().toISOString(), validUntil: new Date(Date.now() + 30*24*60*60*1000).toISOString(), isActive: true }
    ]);
  }

  if (url === '/admin/coupons' && method === 'POST') {
    return respond({ id: Date.now(), message: 'Coupon created successfully' });
  }

  if (url.match(/^\/admin\/coupons\/\d+$/) && method === 'DELETE') {
    return respond({ message: 'Coupon deleted successfully' });
  }

  // No mock available for this route
  return null;
}
