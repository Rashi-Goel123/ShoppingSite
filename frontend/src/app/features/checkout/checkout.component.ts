import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { UserService } from '../../core/services/user.service';
import { RazorpayService } from '../../core/services/razorpay.service';
import { AuthService } from '../../core/services/auth.service';
import { Address, CouponValidation } from '../../core/models/models';
import { environment } from '../../../environments/environment';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-checkout',
  imports: [FormsModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  cart = inject(CartService);
  private orderService = inject(OrderService);
  private userService = inject(UserService);
  private razorpayService = inject(RazorpayService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);
  addresses = signal<Address[]>([]);
  selectedAddressId = signal(0);
  showNewAddress = signal(false);
  newAddr = { label: 'Home', street: '', city: '', state: '', pincode: '' };
  couponCode = '';
  couponResult = signal<CouponValidation | null>(null);
  couponDiscount = signal(0);
  paymentMethod = signal<'razorpay' | 'cod'>('razorpay');
  placing = signal(false);
  paymentStep = signal('Processing...');
  error = signal('');
  paymentSuccess = signal(false);

  ngOnInit() {
    this.razorpayService.loadScript().catch(() => { });
    if (this.cart.appliedCoupon()) {
      this.couponCode = this.cart.appliedCoupon().code;
      this.couponResult.set(this.cart.appliedCoupon());
      this.couponDiscount.set(this.cart.couponDiscount());
    }

    this.userService.getAddresses().subscribe(a => {
      this.addresses.set(a);
      const def = a.find(x => x.isDefault);
      if (def) this.selectedAddressId.set(def.id);
      else if (a.length > 0) this.selectedAddressId.set(a[0].id);
    });
  }
  applyCoupon() {
    if (!this.couponCode.trim()) return;
    this.orderService.validateCoupon(this.couponCode.toUpperCase(), this.cart.subtotal()).subscribe(res => {
      this.couponResult.set(res);
      this.couponDiscount.set(res.isValid ? res.discountAmount : 0);
      if (res.isValid) {
        this.cart.appliedCoupon.set(res);
        this.cart.couponDiscount.set(res.discountAmount);
      } else {
        this.cart.appliedCoupon.set(null);
        this.cart.couponDiscount.set(0);
      }
    });
  }
  removeCoupon() {
    this.couponCode = '';
    this.couponResult.set(null);
    this.couponDiscount.set(0);
    this.cart.appliedCoupon.set(null);
    this.cart.couponDiscount.set(0);
  }
  saveNewAddress() {
    if (!this.newAddr.street || !this.newAddr.city) return;
    this.userService.addAddress({ ...this.newAddr, isDefault: this.addresses().length === 0 }).subscribe(addr => {
      this.addresses.update(list => [...list, addr]);
      this.selectedAddressId.set(addr.id);
      this.showNewAddress.set(false);
      this.newAddr = { label: 'Home', street: '', city: '', state: '', pincode: '' };
    });
  }
  getDeliveryCharge(): number { return this.cart.subtotal() >= 499 ? 0 : 49; }
  getTotal(): number { return Math.max(0, this.cart.subtotal() - this.couponDiscount() + this.getDeliveryCharge()); }
  getTotalSavings(): number { return this.couponDiscount() + this.cart.savings() + (this.getDeliveryCharge() === 0 ? 49 : 0); }
  async placeOrder() {
    if (this.placing() || !this.selectedAddressId()) return;

    this.placing.set(true);
    this.error.set('');
    this.paymentSuccess.set(false);

    try {
      if (this.paymentMethod() === 'razorpay') {
        await this.handleRazorpayPayment();
      } else {
        await this.handleCOD();
      }
    } catch (err: any) {
      this.error.set(err.message || 'Something went wrong');
      this.placing.set(false);
    }
  }
  private async handleRazorpayPayment() {
    const user = this.authService.user();
    const totalPaise = Math.round(this.getTotal() * 100);
    let orderId: string | undefined;
    this.paymentStep.set('Creating payment order...');
    const order = await this.toPromise(this.razorpayService.createOrder(this.getTotal()));
    orderId = order.orderId;
    this.paymentStep.set('Opening Razorpay...');
    this.placing.set(false);
    const paymentResult = await this.razorpayService.openCheckout({
      orderId: orderId,
      amount: totalPaise,
      currency: 'INR',
      keyId: environment.razorpayKeyId,
      userName: user?.name,
      userEmail: user?.email,
      userPhone: user?.phone,
      description: `Fashion Store — ${this.cart.items().length} item(s) — ₹${this.getTotal()}`,
      notes: {
        itemCount: String(this.cart.items().length),
        addressId: String(this.selectedAddressId()),
        coupon: this.couponDiscount() > 0 ? this.couponCode : 'none',
      }
    });
    this.placing.set(true);
    console.log('✅ Razorpay Payment Successful:', paymentResult);
    this.paymentStep.set('Verifying payment...');
    const verification = await this.toPromise(this.razorpayService.verifyPayment(paymentResult));
    if (!verification.verified) {
      throw new Error('Payment verification failed on server. Contact support.');
    }
    this.paymentSuccess.set(true);
    this.paymentStep.set('Placing order...');

    const orderData = {
      addressId: this.selectedAddressId(),
      paymentMethod: 'razorpay',
      couponCode: this.couponDiscount() > 0 ? this.couponCode : null,
      razorpayOrderId: paymentResult.razorpayOrderId || orderId || '',
      razorpayPaymentId: paymentResult.razorpayPaymentId,
      razorpaySignature: paymentResult.razorpaySignature,
      items: this.cart.items(),
      totalAmount: this.getTotal()
    };

    const result = await this.toPromise(this.orderService.placeOrder(orderData));
    this.cart.clearLocally();
    this.toastService.success('Payment successful! Order placed.');
    this.router.navigate(['/order-success', result.orderId]);
  }
  private async handleCOD() {
    this.paymentStep.set('Placing order...');

    const result = await this.toPromise(
      this.orderService.placeOrder({
        addressId: this.selectedAddressId(),
        paymentMethod: 'cod',
        couponCode: this.couponDiscount() > 0 ? this.couponCode : null,
        items: this.cart.items(),
        totalAmount: this.getTotal()
      })
    );
    this.cart.clearLocally();
    this.toastService.success('Order placed successfully via Cash on Delivery!');
    this.router.navigate(['/order-success', result.orderId]);
  }
  private toPromise<T>(obs: import('rxjs').Observable<T>): Promise<T> {
    return new Promise((resolve, reject) => {
      obs.subscribe({ next: resolve, error: (err) => reject(new Error(err.error?.message || err.message || 'Request failed')) });
    });
  }

  private delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}
