import { Component, inject, signal, OnInit, OnDestroy, ElementRef, ViewChild, HostListener } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { ProductService } from '../../core/services/product.service';
import { ProductListItem } from '../../core/models/models';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, of } from 'rxjs';

@Component({
  selector: 'app-header',
  imports: [RouterLink, FormsModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css'
})
export class HeaderComponent implements OnInit, OnDestroy {
  auth = inject(AuthService);
  cart = inject(CartService);
  private productService = inject(ProductService);
  private router = inject(Router);

  searchQuery = signal('');
  searchResults = signal<ProductListItem[]>([]);
  showResults = signal(false);
  searching = signal(false);

  private searchSubject = new Subject<string>();
  private searchSub?: Subscription;

  ngOnInit() {
    this.searchSub = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.length < 2) {
          return of(null);
        }
        this.searching.set(true);
        return this.productService.getProducts({ search: term, pageSize: 6 });
      })
    ).subscribe({
      next: (res) => {
        this.searching.set(false);
        if (res) {
          this.searchResults.set(res.items);
          this.showResults.set(true);
        } else {
          this.searchResults.set([]);
          this.showResults.set(false);
        }
      },
      error: () => {
        this.searching.set(false);
        this.searchResults.set([]);
      }
    });
  }

  ngOnDestroy() {
    this.searchSub?.unsubscribe();
  }

  onSearchInput(value: string) {
    this.searchQuery.set(value);
    this.searchSubject.next(value);
  }

  onSearchSubmit() {
    const q = this.searchQuery().trim();
    if (q) {
      this.showResults.set(false);
      this.router.navigate(['/products'], { queryParams: { search: q } });
    }
  }

  goToProduct(slug: string) {
    this.showResults.set(false);
    this.searchQuery.set('');
    this.router.navigate(['/products', slug]);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    this.showResults.set(false);
  }

  onSearchFocus() {
    if (this.searchResults().length > 0) {
      this.showResults.set(true);
    }
  }
}
