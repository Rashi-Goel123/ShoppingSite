import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../core/services/product.service';
import { ProductListItem, Category } from '../../core/models/models';

@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  private productService = inject(ProductService);
  categories = signal<Category[]>([]);
  featured = signal<ProductListItem[]>([]);
  newArrivals = signal<ProductListItem[]>([]);

  ngOnInit() {
    this.productService.getCategories().subscribe(cats => this.categories.set(cats));
    this.productService.getFeatured().subscribe(products => this.featured.set(products));
    this.productService.getNewArrivals().subscribe(products => this.newArrivals.set(products));
  }
}

