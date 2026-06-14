import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { User, Address, Notification, ProductListItem } from '../models/models';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private http: HttpClient) { }

  updateProfile(data: { name: string; email?: string }) {
    return this.http.put<User>(`${environment.apiUrl}/users/profile`, data);
  }

  uploadAvatar(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ url: string }>(`${environment.apiUrl}/users/avatar`, formData);
  }

  setPresetAvatar(url: string) {
    return this.http.put<{ url: string }>(`${environment.apiUrl}/users/avatar/preset`, JSON.stringify(url), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
  getAddresses() {
    return this.http.get<Address[]>(`${environment.apiUrl}/users/addresses`);
  }

  addAddress(data: any) {
    return this.http.post<Address>(`${environment.apiUrl}/users/addresses`, data);
  }

  updateAddress(id: number, data: any) {
    return this.http.put<Address>(`${environment.apiUrl}/users/addresses/${id}`, data);
  }

  deleteAddress(id: number) {
    return this.http.delete(`${environment.apiUrl}/users/addresses/${id}`);
  }
  getWishlist() {
    return this.http.get<ProductListItem[]>(`${environment.apiUrl}/users/wishlist`);
  }
  toggleWishlist(productId: number) {
    return this.http.post<{ added: boolean; message: string }>(
      `${environment.apiUrl}/users/wishlist/${productId}`, {}
    );
  }
  getWishlistIds() {
    return this.http.get<number[]>(`${environment.apiUrl}/users/wishlist/ids`);
  }
  getNotifications() {
    return this.http.get<Notification[]>(`${environment.apiUrl}/users/notifications`);
  }
  markNotificationRead(id: number) {
    return this.http.put(`${environment.apiUrl}/users/notifications/${id}/read`, {});
  }
  markAllRead() {
    return this.http.put(`${environment.apiUrl}/users/notifications/read-all`, {});
  }
}
