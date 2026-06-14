import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { User, AuthResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _user = signal<User | null>(null);
  private _token = signal<string | null>(null);

  user = this._user.asReadonly();
  token = this._token.asReadonly();
  isLoggedIn = computed(() => !!this._token());
  isAdmin = computed(() => this._user()?.role === 'admin');

  constructor(private http: HttpClient, private router: Router) {
    this.loadFromStorage();
  }

  private loadFromStorage() {
    const token = localStorage.getItem('token');
    const user = localStorage.getItem('user');
    if (token && user) {
      this._token.set(token);
      this._user.set(JSON.parse(user));
    }
  }

  private saveAuth(res: AuthResponse) {
    this._token.set(res.token);
    this._user.set(res.user);
    localStorage.setItem('token', res.token);
    localStorage.setItem('user', JSON.stringify(res.user));
  }

  sendOtp(phone: string) {
    return this.http.post<{ message: string; dev_otp: string }>(
      `${environment.apiUrl}/auth/send-otp`, { phone }
    );
  }

  verifyOtp(phone: string, code: string) {
    return this.http.post<AuthResponse>(
      `${environment.apiUrl}/auth/verify-otp`, { phone, code }
    );
  }

  adminRegister(data: { name: string; email: string; phone: string; password: string; inviteCode: string }) {
    return this.http.post<AuthResponse>(
      `${environment.apiUrl}/auth/admin-register`, data
    );
  }

  adminLogin(email: string, password: string) {
    return this.http.post<AuthResponse>(
      `${environment.apiUrl}/auth/admin-login`, { email, password }
    );
  }

  handleAuthResponse(res: AuthResponse) {
    this.saveAuth(res);
    if (res.user.role === 'admin') {
      this.router.navigate(['/admin']);
    } else {
      this.router.navigate(['/']);
    }
  }

  updateUser(user: User) {
    this._user.set(user);
    localStorage.setItem('user', JSON.stringify(user));
  }

  logout() {
    this._token.set(null);
    this._user.set(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.router.navigate(['/login']);
  }
}
