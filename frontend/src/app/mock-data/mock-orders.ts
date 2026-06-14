export const MOCK_ORDERS = [
  {
    id: 1, orderNumber: 'ORD-20260610-1234', status: 'delivered', paymentStatus: 'paid',
    totalAmount: 2548, itemCount: 2, firstItemImage: 'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=200',
    createdAt: '2026-06-10T10:30:00Z', paymentMethod: 'razorpay', subtotal: 2598, discount: 50, deliveryCharge: 0,
    couponCode: 'WELCOME50',
    shippingAddress: { id: 1, label: 'Home', street: '123 MG Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400001', isDefault: true },
    items: [
      { id: 1, productId: 1, title: 'Classic Cotton Crew Neck T-Shirt', image: 'https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=200', sku: 'UE-TCN-BLK-M', color: 'Black', size: 'M', priceAtPurchase: 499, quantity: 2 },
      { id: 2, productId: 6, title: 'Air Mesh Running Sneakers', image: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=200', sku: 'SX-AMR-BLK-9', color: 'Black/Red', size: '9UK', priceAtPurchase: 1600, quantity: 1 }
    ],
    tracking: [
      { status: 'placed', description: 'Order placed successfully', timestamp: '2026-06-10T10:30:00Z' },
      { status: 'confirmed', description: 'Order confirmed', timestamp: '2026-06-10T11:00:00Z' },
      { status: 'packed', description: 'Items packed', location: 'Mumbai Warehouse', timestamp: '2026-06-11T09:00:00Z' },
      { status: 'shipped', description: 'Shipped via BlueDart', location: 'Mumbai', timestamp: '2026-06-11T14:00:00Z' },
      { status: 'out_for_delivery', description: 'Out for delivery', location: 'Mumbai', timestamp: '2026-06-12T08:00:00Z' },
      { status: 'delivered', description: 'Delivered successfully', location: 'Mumbai', timestamp: '2026-06-12T11:30:00Z' },
    ],
    estimatedDelivery: '2026-06-15T00:00:00Z'
  },
  {
    id: 2, orderNumber: 'ORD-20260612-5678', status: 'shipped', paymentStatus: 'paid',
    totalAmount: 1999, itemCount: 1, firstItemImage: 'https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=200',
    createdAt: '2026-06-12T15:20:00Z', paymentMethod: 'razorpay', subtotal: 1999, discount: 0, deliveryCharge: 0,
    shippingAddress: { id: 1, label: 'Home', street: '123 MG Road', city: 'Mumbai', state: 'Maharashtra', pincode: '400001', isDefault: true },
    items: [
      { id: 3, productId: 4, title: 'Floral Print Maxi Dress', image: 'https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=200', sku: 'BL-MXD-FLR-M', color: 'Floral Multi', size: 'M', priceAtPurchase: 1999, quantity: 1 }
    ],
    tracking: [
      { status: 'placed', description: 'Order placed', timestamp: '2026-06-12T15:20:00Z' },
      { status: 'confirmed', description: 'Order confirmed', timestamp: '2026-06-12T16:00:00Z' },
      { status: 'shipped', description: 'Shipped via Delhivery', location: 'Delhi Hub', timestamp: '2026-06-13T10:00:00Z' },
    ],
    estimatedDelivery: '2026-06-17T00:00:00Z'
  },
  {
    id: 3, orderNumber: 'ORD-20260613-9012', status: 'placed', paymentStatus: 'pending',
    totalAmount: 4498, itemCount: 2, firstItemImage: 'https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=200',
    createdAt: '2026-06-13T09:00:00Z', paymentMethod: 'cod', subtotal: 4498, discount: 0, deliveryCharge: 0,
    shippingAddress: { id: 2, label: 'Work', street: '456 Brigade Road', city: 'Bangalore', state: 'Karnataka', pincode: '560001', isDefault: false },
    items: [
      { id: 4, productId: 7, title: 'Chronograph Steel Watch', image: 'https://images.unsplash.com/photo-1524592094714-0f0654e20314?w=200', sku: 'TC-CHR-SLV-OS', color: 'Silver', size: 'One Size', priceAtPurchase: 3999, quantity: 1 },
      { id: 5, productId: 5, title: 'Ribbed Crop Top', image: 'https://images.unsplash.com/photo-1594938298603-c8148c4dae35?w=200', sku: 'SH-RCT-BLK-S', color: 'Black', size: 'S', priceAtPurchase: 499, quantity: 1 }
    ],
    tracking: [
      { status: 'placed', description: 'Order placed', timestamp: '2026-06-13T09:00:00Z' }
    ],
    estimatedDelivery: '2026-06-18T00:00:00Z'
  }
];
