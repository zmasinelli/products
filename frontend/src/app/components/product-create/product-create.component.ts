import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/category.model';
import { CreateProductRequest } from '../../models/product.model';

@Component({
  selector: 'app-product-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './product-create.component.html',
  styleUrl: './product-create.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductCreateComponent {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  categories$ = this.categoryService.getCategories().pipe(
    catchError(() => of([] as Category[]))
  );

  productForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(255)]],
    description: [''],
    price: [null, [Validators.required, Validators.min(0.01)]],
    categoryId: [null, [Validators.required]],
    stockQuantity: [0, [Validators.min(0)]]
  });

  isSubmitting = false;
  error: string | null = null;

  get name() {
    return this.productForm.get('name');
  }

  get description() {
    return this.productForm.get('description');
  }

  get price() {
    return this.productForm.get('price');
  }

  get categoryId() {
    return this.productForm.get('categoryId');
  }

  get stockQuantity() {
    return this.productForm.get('stockQuantity');
  }

  onSubmit(): void {
    if (this.productForm.invalid || this.isSubmitting) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const formValue = this.productForm.value;
    const product: CreateProductRequest = {
      name: formValue.name,
      description: formValue.description || null,
      price: formValue.price,
      categoryId: formValue.categoryId,
      stockQuantity: formValue.stockQuantity || 0
    };

    this.productService.createProduct(product).subscribe({
      next: (createdProduct) => {
        this.router.navigate(['/products', createdProduct.id]);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.error = error.message || 'Failed to create product';
      }
    });
  }
}
