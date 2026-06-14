import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface RazorpayOrderResult {
  orderId: string;
  amount: number;
  currency: string;
  keyId: string;
}

export interface RazorpayPaymentResult {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

export interface RazorpayVerifyResult {
  verified: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class RazorpayService {
  private http = inject(HttpClient);
  private scriptLoaded = false;
  loadScript(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (typeof window !== 'undefined' && window.Razorpay) {
        this.scriptLoaded = true;
        resolve();
        return;
      }
      const existingScript = document.querySelector('script[src*="checkout.razorpay.com"]');
      if (existingScript) {
        existingScript.addEventListener('load', () => {
          this.scriptLoaded = true;
          resolve();
        });
        existingScript.addEventListener('error', () => reject(new Error('Failed to load Razorpay SDK')));
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://checkout.razorpay.com/v1/checkout.js';
      script.onload = () => {
        this.scriptLoaded = true;
        resolve();
      };
      script.onerror = () => reject(new Error('Failed to load Razorpay SDK'));
      document.head.appendChild(script);
    });
  }
  createOrder(amount: number): Observable<RazorpayOrderResult> {
    return this.http.post<RazorpayOrderResult>(
      `${environment.apiUrl}/payments/create-order`,
      { amount }
    );
  }
  verifyPayment(data: RazorpayPaymentResult): Observable<RazorpayVerifyResult> {
    return this.http.post<RazorpayVerifyResult>(
      `${environment.apiUrl}/payments/verify`,
      data
    );
  }
  openCheckout(options: {
    orderId?: string;
    amount: number;
    currency?: string;
    keyId: string;
    userName?: string;
    userEmail?: string;
    userPhone?: string;
    description?: string;
    notes?: Record<string, string>;
  }): Promise<RazorpayPaymentResult> {
    return new Promise(async (resolve, reject) => {
      try {
        await this.loadScript();
      } catch {
        reject(new Error('Failed to load Razorpay. Check your internet connection.'));
        return;
      }
      if (typeof window === 'undefined' || !window.Razorpay) {
        reject(new Error('Razorpay SDK not available. Please refresh the page.'));
        return;
      }
      const rzpOptions: any = {
        key: options.keyId,
        amount: options.amount,
        currency: options.currency || 'INR',
        name: 'Fashion Store',
        description: options.description || 'Fashion Store Order Payment',
        image: 'https://api.dicebear.com/8.x/shapes/svg?seed=fashion-store&backgroundColor=6c5ce7',
        handler: (response: any) => {
          console.log('Razorpay Payment Success:', response);
          resolve({
            razorpayOrderId: response.razorpay_order_id || '',
            razorpayPaymentId: response.razorpay_payment_id,
            razorpaySignature: response.razorpay_signature || '',
          });
        },
        prefill: {
          name: options.userName || 'Fashion User',
          email: options.userEmail || 'user@fashionstore.com',
          contact: options.userPhone ? (options.userPhone.startsWith('+') ? options.userPhone : `+91${options.userPhone}`) : '+919876543210',
        },
        notes: options.notes || {
          store: 'Fashion Store',
          environment: environment.production ? 'production' : 'test',
        },
        theme: {
          color: '#6C5CE7',
          backdrop_color: 'rgba(0,0,0,0.75)',
        },
        modal: {
          ondismiss: () => {
            console.log('Razorpay checkout dismissed by user');
            reject(new Error('Payment cancelled by user'));
          },
          confirm_close: true,
          animation: true,
          escape: true,
        },
        retry: {
          enabled: true,
          max_count: 3,
        },
      };
      if (options.orderId) {
        rzpOptions.order_id = options.orderId;
      }

      try {
        const rzp = new window.Razorpay(rzpOptions);

        rzp.on('payment.failed', (response: any) => {
          console.error('Razorpay Payment Failed:', response.error);
          reject(new Error(
            response.error?.description ||
            response.error?.reason ||
            'Payment failed. Please try again.'
          ));
        });
        rzp.open();
      } catch (err: any) {
        reject(new Error(err.message || 'Failed to open Razorpay checkout'));
      }
    });
  }
}
