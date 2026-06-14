import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-register',
  imports: [FormsModule, RouterLink],
  templateUrl: './admin-register.component.html',
  styleUrl: './admin-register.component.css'
})
export class AdminRegisterComponent {
  private auth = inject(AuthService);
  name = ''; email = ''; phone = ''; password = ''; inviteCode = '';
  loading = signal(false);
  error = signal('');

  register() {
    this.loading.set(true); this.error.set('');
    this.auth.adminRegister({ name: this.name, email: this.email, phone: this.phone, password: this.password, inviteCode: this.inviteCode }).subscribe({
      next: (res) => { this.auth.handleAuthResponse(res); this.loading.set(false); },
      error: (err) => { this.error.set(err.error?.message || 'Registration failed'); this.loading.set(false); }
    });
  }
}
