import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private auth = inject(AuthService);

  phone = '';
  otp = '';
  otpSent = signal(false);
  loading = signal(false);
  error = signal('');

  sendOtp() {
    this.loading.set(true);
    this.error.set('');
    this.auth.sendOtp(this.phone).subscribe({
      next: () => { this.otpSent.set(true); this.loading.set(false); },
      error: (err) => { this.error.set(err.error?.message || 'Failed to send OTP'); this.loading.set(false); }
    });
  }

  verifyOtp() {
    this.loading.set(true);
    this.error.set('');
    this.auth.verifyOtp(this.phone, this.otp).subscribe({
      next: (res) => { this.auth.handleAuthResponse(res); this.loading.set(false); },
      error: (err) => { this.error.set(err.error?.message || 'Invalid OTP'); this.loading.set(false); }
    });
  }
}
