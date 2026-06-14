import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-login.component.html',
  styleUrl: './admin-login.component.css'
})
export class AdminLoginComponent {
  private auth = inject(AuthService);
  email = ''; password = '';
  loading = signal(false);
  error = signal('');

  login() {
    this.loading.set(true); this.error.set('');
    this.auth.adminLogin(this.email, this.password).subscribe({
      next: (res) => { this.auth.handleAuthResponse(res); this.loading.set(false); },
      error: (err) => { this.error.set(err.error?.message || 'Login failed'); this.loading.set(false); }
    });
  }
}
