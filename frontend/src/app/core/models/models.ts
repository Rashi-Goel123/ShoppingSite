export interface User {
  id: number;
  phone: string;
  name: string;
  email?: string;
  avatar?: string;
  role: string;
}
export interface AuthResponse {
  token: string;
  user: User;
}
export interface Address {
  id: number;
  label: string;
  street: string;
  city: string;
  state: string;
  pincode: string;
  isDefault: boolean;
}
export interface Category {
  id: number;
  name: string;
  slug: string;
  icon?: string;
  parentId?: number;
  children?: Category[];
}
export interface ProductListItem {
  id: number;
  title: string;
  slug: string;
  brand: string;
  gender: string;
  basePrice: number;
  firstImage?: string;
  ratingAverage: number;
  ratingCount: number;
  isFeatured: boolean;
  categoryName: string;
  discountedPrice?: number;
  mrp?: number;
  discountPercent?: number;
}
export interface ProductDetail {
  id: number;
  title: string;
  slug: string;
  description: string;
  brand: string;
  gender: string;
  material?: string;
  basePrice: number;
  ratingAverage: number;
  ratingCount: number;
  isFeatured: boolean;
  category: Category;
  images: ProductImage[];
  variants: ProductVariant[];
  reviews?: Review[];
}
export interface ProductImage {
  id: number;
  url: string;
  displayOrder: number;
}
export interface ProductVariant {
  id: number;
  sku: string;
  color?: string;
  colorHex?: string;
  size?: string;
  price: number;
  mrp: number;
  stock: number;
}
export interface Review {
  id: number;
  rating: number;
  title?: string;
  comment?: string;
  userName: string;
  isVerifiedPurchase: boolean;
  createdAt: string;
}
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
export interface CartItem {
  id: number;
  productId: number;
  variantId: number;
  title: string;
  image?: string;
  color?: string;
  size?: string;
  price: number;
  mrp: number;
  quantity: number;
  stock: number;
}
export interface OrderListItem {
  id: number;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  totalAmount: number;
  itemCount: number;
  firstItemImage?: string;
  createdAt: string;
}
export interface OrderDetail {
  id: number;
  orderNumber: string;
  status: string;
  paymentStatus: string;
  paymentMethod: string;
  subtotal: number;
  discount: number;
  deliveryCharge: number;
  totalAmount: number;
  couponCode?: string;
  shippingAddress: Address;
  items: OrderItem[];
  tracking: TrackingEvent[];
  estimatedDelivery?: string;
  createdAt: string;
}
export interface OrderItem {
  id: number;
  productId: number;
  title: string;
  image?: string;
  sku?: string;
  color?: string;
  size?: string;
  priceAtPurchase: number;
  quantity: number;
}
export interface TrackingEvent {
  status: string;
  description?: string;
  location?: string;
  timestamp: string;
}
export interface Coupon {
  id: number;
  code: string;
  type: string;
  value: number;
  minOrderAmount: number;
  maxDiscount: number;
  usageLimit: number;
  usedCount: number;
  validFrom: string;
  validUntil: string;
  isActive: boolean;
}
export interface CouponValidation {
  isValid: boolean;
  message?: string;
  discountAmount: number;
  code: string;
}
export interface Notification {
  id: number;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  data?: string;
  createdAt: string;
}
export interface DashboardStats {
  totalRevenue: number;
  totalOrders: number;
  totalUsers: number;
  totalProducts: number;
  pendingOrders: number;
  deliveredOrders: number;
  recentOrders: RecentOrder[];
  topProducts: TopProduct[];
}
export interface RecentOrder {
  id: number;
  orderNumber: string;
  userName: string;
  totalAmount: number;
  status: string;
  createdAt: string;
}
export interface TopProduct {
  productId: number;
  title: string;
  image?: string;
  totalSold: number;
  totalRevenue: number;
}
export interface PaymentOrder {
  orderId: string;
  amount: number;
  currency: string;
  keyId: string;
}
