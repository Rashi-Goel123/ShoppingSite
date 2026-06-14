import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ProductListItem, ProductDetail, Category, PagedResult, Review } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  constructor(private http: HttpClient) { }

  getProducts(filters: any = {}) {
    let params = new HttpParams();
    Object.keys(filters).forEach(key => {
      if (filters[key] !== null && filters[key] !== undefined && filters[key] !== '') {
        params = params.set(key, filters[key]);
      }
    });
    return this.http.get<PagedResult<ProductListItem>>(`${environment.apiUrl}/products`, { params });
  }

  getBySlug(slug: string) {
    return this.http.get<ProductDetail>(`${environment.apiUrl}/products/${slug}`);
  }

  getFeatured() {
    return this.http.get<ProductListItem[]>(`${environment.apiUrl}/products/featured`);
  }

  getNewArrivals() {
    return this.http.get<ProductListItem[]>(`${environment.apiUrl}/products/new-arrivals`);
  }

  getCategories() {
    return this.http.get<Category[]>(`${environment.apiUrl}/products/categories`);
  }

  getBrands() {
    return this.http.get<string[]>(`${environment.apiUrl}/products/brands`);
  }

  addReview(productId: number, data: { rating: number; title?: string; comment?: string }) {
    return this.http.post(`${environment.apiUrl}/products/${productId}/review`, data);
  }
}
