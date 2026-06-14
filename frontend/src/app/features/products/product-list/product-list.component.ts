import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { ProductListItem, Category } from '../../../core/models/models';

@Component({
  selector: 'app-product-list',
  imports: [RouterLink, FormsModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css'
})
export class ProductListComponent implements OnInit {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  products = signal<ProductListItem[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  totalCount = signal(0);
  pageTitle = signal('All Products');

  filters: any = { category: '', gender: '', sortBy: 'newest', search: '', minPrice: null, maxPrice: null, page: 1, pageSize: 12 };

  ngOnInit() {
    this.productService.getCategories().subscribe(c => this.categories.set(c));
    this.route.queryParams.subscribe(params => {
      this.filters.category = params['category'] || '';
      this.filters.gender = params['gender'] || '';
      this.filters.search = params['search'] || '';
      this.filters.sortBy = params['sortBy'] || 'newest';
      this.filters.minPrice = params['minPrice'] || null;
      this.filters.maxPrice = params['maxPrice'] || null;
      this.updateTitle();
      this.loadProducts();
    });
  }

  onFilterChange() {
    const queryParams: any = {};
    if (this.filters.category) queryParams.category = this.filters.category;
    if (this.filters.gender) queryParams.gender = this.filters.gender;
    if (this.filters.search) queryParams.search = this.filters.search;
    if (this.filters.sortBy && this.filters.sortBy !== 'newest') queryParams.sortBy = this.filters.sortBy;
    if (this.filters.minPrice) queryParams.minPrice = this.filters.minPrice;
    if (this.filters.maxPrice) queryParams.maxPrice = this.filters.maxPrice;

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: queryParams
    });
  }

  loadProducts() {
    this.loading.set(true);
    this.productService.getProducts(this.filters).subscribe({
      next: (res) => { this.products.set(res.items); this.totalCount.set(res.totalCount); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  updateTitle() {
    if (this.filters.gender) this.pageTitle.set(this.filters.gender.charAt(0).toUpperCase() + this.filters.gender.slice(1) + "'s Collection");
    else if (this.filters.category) this.pageTitle.set(this.filters.category.replace(/-/g, ' ').replace(/\b\w/g, (l: string) => l.toUpperCase()));
    else if (this.filters.search) this.pageTitle.set(`Results for "${this.filters.search}"`);
    else this.pageTitle.set('All Products');
  }

  clearFilters() {
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
  }
}
