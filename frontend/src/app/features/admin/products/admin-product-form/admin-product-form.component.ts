import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { environment } from '../../../../../environments/environment';
import { ToastService } from '../../../../core/services/toast.service';

@Component({
  selector: 'app-admin-product-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './admin-product-form.component.html',
  styleUrl: './admin-product-form.component.css'
})
export class AdminProductFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private router = inject(Router);
  private toast = inject(ToastService);

  productForm!: FormGroup;
  categories = signal<any[]>([]);
  submitting = signal(false);
  
  // After creation, we switch to image upload mode
  createdProductId = signal<number | null>(null);
  uploading = signal(false);
  uploadedImages = signal<any[]>([]);

  ngOnInit() {
    this.initForm();
    this.loadCategories();
  }

  initForm() {
    this.productForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      categoryId: ['', Validators.required],
      brand: [''],
      gender: ['unisex'],
      material: [''],
      basePrice: [0, [Validators.required, Validators.min(0)]],
      isFeatured: [false],
      variants: this.fb.array([this.createVariantGroup()])
    });
  }

  createVariantGroup(): FormGroup {
    return this.fb.group({
      sku: ['', Validators.required],
      color: [''],
      colorHex: [''],
      size: [''],
      price: [0, [Validators.required, Validators.min(0)]],
      mrp: [0],
      stock: [0, [Validators.required, Validators.min(0)]]
    });
  }

  get variants(): FormArray {
    return this.productForm.get('variants') as FormArray;
  }

  addVariant() {
    this.variants.push(this.createVariantGroup());
  }

  removeVariant(index: number) {
    if (this.variants.length > 1) {
      this.variants.removeAt(index);
    }
  }

  loadCategories() {
    this.http.get<any[]>(`${environment.apiUrl}/products/categories`).subscribe({
      next: (res) => this.categories.set(res),
      error: () => this.toast.error('Failed to load categories')
    });
  }

  onSubmit() {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      this.toast.error('Please fill all required fields correctly');
      return;
    }

    this.submitting.set(true);
    const formValue = this.productForm.value;
    // Convert IDs/Numbers properly
    formValue.categoryId = Number(formValue.categoryId);
    
    this.http.post<any>(`${environment.apiUrl}/admin/products`, formValue).subscribe({
      next: (res) => {
        this.toast.success('Product created successfully! Please upload images.');
        this.createdProductId.set(res.id);
        this.submitting.set(false);
      },
      error: () => {
        this.toast.error('Failed to create product');
        this.submitting.set(false);
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file && this.createdProductId()) {
      this.uploading.set(true);
      const formData = new FormData();
      formData.append('file', file);

      this.http.post<any>(`${environment.apiUrl}/admin/products/${this.createdProductId()}/images`, formData).subscribe({
        next: (res) => {
          this.toast.success('Image uploaded');
          this.uploadedImages.update(imgs => [...imgs, res]);
          this.uploading.set(false);
        },
        error: () => {
          this.toast.error('Failed to upload image');
          this.uploading.set(false);
        }
      });
    }
  }

  finish() {
    this.router.navigate(['/admin/products']);
  }
}
