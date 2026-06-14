import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { OrderListItem, OrderDetail, PaymentOrder, CouponValidation } from '../models/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  constructor(private http: HttpClient) { }

  placeOrder(data: any) {
    return this.http.post<{ orderId: number; orderNumber: string; message: string }>(
      `${environment.apiUrl}/orders`, data
    );
  }

  getOrders() {
    return this.http.get<OrderListItem[]>(`${environment.apiUrl}/orders`);
  }

  getOrder(id: number) {
    return this.http.get<OrderDetail>(`${environment.apiUrl}/orders/${id}`);
  }

  cancelOrder(id: number) {
    return this.http.post<{ message: string }>(`${environment.apiUrl}/orders/${id}/cancel`, {});
  }
  createPaymentOrder(amount: number) {
    return this.http.post<PaymentOrder>(`${environment.apiUrl}/payments/create-order`, { amount });
  }

  verifyPayment(data: { razorpayOrderId: string; razorpayPaymentId: string; razorpaySignature: string }) {
    return this.http.post<{ verified: boolean; message: string }>(
      `${environment.apiUrl}/payments/verify`, data
    );
  }
  validateCoupon(code: string, orderAmount: number) {
    return this.http.post<CouponValidation>(
      `${environment.apiUrl}/coupons/validate`, { code, orderAmount }
    );
  }
  getAvailableCoupons() {
    return this.http.get<any[]>(`${environment.apiUrl}/coupons/available`);
  }
}
