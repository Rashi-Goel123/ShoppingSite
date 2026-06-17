import { Component, OnInit, OnDestroy, inject, signal, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
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
export class ProductListComponent implements OnInit, AfterViewInit, OnDestroy {
  private productService = inject(ProductService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  products = signal<ProductListItem[]>([]);
  categories = signal<Category[]>([]);
  loading = signal(true);
  loadingMore = signal(false);
  totalCount = signal(0);
  totalPages = signal(1);
  currentPage = signal(1);
  hasMore = signal(false);
  pageTitle = signal('All Products');

  filters: any = { category: '', gender: '', sortBy: 'newest', search: '', minPrice: null, maxPrice: null, page: 1, pageSize: 12 };

  @ViewChild('scrollSentinel') scrollSentinel?: ElementRef;
  private observer?: IntersectionObserver;

  ngOnInit() {
    this.productService.getCategories().subscribe(c => this.categories.set(c));
    this.route.queryParams.subscribe(params => {
      this.filters.category = params['category'] || '';
      this.filters.gender = params['gender'] || '';
      this.filters.search = params['search'] || '';
      this.filters.sortBy = params['sortBy'] || 'newest';
      this.filters.minPrice = params['minPrice'] || null;
      this.filters.maxPrice = params['maxPrice'] || null;
      this.filters.page = 1;
      this.currentPage.set(1);
      this.updateTitle();
      this.loadProducts(true);
    });
  }

  ngAfterViewInit() {
    this.setupIntersectionObserver();
  }

  ngOnDestroy() {
    this.observer?.disconnect();
  }

  private setupIntersectionObserver() {
    this.observer = new IntersectionObserver(
      (entries) => {
        const entry = entries[0];
        if (entry.isIntersecting && this.hasMore() && !this.loadingMore() && !this.loading()) {
          this.loadNextPage();
        }
      },
      { rootMargin: '200px' }
    );

    // Observe after a short delay to ensure the element exists
    setTimeout(() => {
      if (this.scrollSentinel?.nativeElement) {
        this.observer!.observe(this.scrollSentinel.nativeElement);
      }
    }, 500);
  }

  private reobserveSentinel() {
    // Re-observe the sentinel after view updates
    setTimeout(() => {
      this.observer?.disconnect();
      if (this.scrollSentinel?.nativeElement) {
        this.observer!.observe(this.scrollSentinel.nativeElement);
      }
    }, 100);
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

  loadProducts(reset: boolean) {
    if (reset) {
      this.loading.set(true);
      this.products.set([]);
      this.filters.page = 1;
      this.currentPage.set(1);
    }

    this.productService.getProducts(this.filters).subscribe({
      next: (res) => {
        if (reset) {
          this.products.set(res.items);
        } else {
          this.products.update(current => [...current, ...res.items]);
        }
        this.totalCount.set(res.totalCount);
        this.totalPages.set(res.totalPages);
        this.hasMore.set(this.currentPage() < res.totalPages);
        this.loading.set(false);
        this.loadingMore.set(false);
        this.reobserveSentinel();
      },
      error: () => {
        this.loading.set(false);
        this.loadingMore.set(false);
      }
    });
  }

  loadNextPage() {
    if (this.loadingMore() || !this.hasMore()) return;
    this.loadingMore.set(true);
    const nextPage = this.currentPage() + 1;
    this.currentPage.set(nextPage);
    this.filters.page = nextPage;
    this.loadProducts(false);
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
