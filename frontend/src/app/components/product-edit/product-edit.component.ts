import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/category.model';
import { UpdateProductRequest } from '../../models/product.model';

@Component({
  selector: 'app-product-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './product-edit.component.html',
  styleUrl: './product-edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductEditComponent {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  categories$ = this.categoryService.getCategories().pipe(
    catchError(() => of([] as Category[]))
  );

  productForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(255)]],
    description: [''],
    price: [null, [Validators.required, Validators.min(0.01)]],
    categoryId: [null, [Validators.required]],
    stockQuantity: [0, [Validators.min(0)]],
    isActive: [true, []]
  });

  isSubmitting = false;
  isLoading = true;
  error: string | null = null;
  productId: number | null = null;

  product$ = this.route.paramMap.pipe(
    map(params => {
      const id = params.get('id');
      return id ? parseInt(id, 10) : null;
    }),
    switchMap(id => {
      if (!id || isNaN(id)) {
        this.isLoading = false;
        this.error = 'Invalid product ID';
        return of(null);
      }

      this.productId = id;
      return this.productService.getProductById(id).pipe(
        catchError(error => {
          this.isLoading = false;
          this.error = error.message || 'Failed to load product';
          return of(null);
        })
      );
    })
  );

  constructor() {
    this.product$.subscribe(product => {
      if (product) {
        this.isLoading = false;
        this.productForm.patchValue({
          name: product.name,
          description: product.description || '',
          price: product.price,
          categoryId: product.categoryId,
          stockQuantity: product.stockQuantity,
          isActive: product.isActive ?? true
        });
      }
    });
  }

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
    if (this.productForm.invalid || this.isSubmitting || !this.productId) {
      this.productForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const formValue = this.productForm.getRawValue();
    // #region agent log
    fetch('http://127.0.0.1:7242/ingest/acf325a0-3256-4a55-9d83-c6c0009382cd',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'product-edit.component.ts:113',message:'Form raw value before processing',data:{formValue,isActiveRaw:formValue.isActive,isActiveType:typeof formValue.isActive},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'A'})}).catch(()=>{});
    // #endregion
    
    const product: UpdateProductRequest = {
      name: formValue.name,
      description: formValue.description || null,
      price: formValue.price,
      categoryId: formValue.categoryId,
      stockQuantity: formValue.stockQuantity || 0,
      isActive: Boolean(formValue.isActive)
    };
    // #region agent log
    fetch('http://127.0.0.1:7242/ingest/acf325a0-3256-4a55-9d83-c6c0009382cd',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'product-edit.component.ts:122',message:'UpdateProductRequest object created',data:{product,isActiveValue:product.isActive,productId:this.productId},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'B'})}).catch(()=>{});
    // #endregion

    this.productService.updateProduct(this.productId, product).subscribe({
      next: () => {
        // #region agent log
        fetch('http://127.0.0.1:7242/ingest/acf325a0-3256-4a55-9d83-c6c0009382cd',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'product-edit.component.ts:127',message:'Update request succeeded',data:{productId:this.productId},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'D'})}).catch(()=>{});
        // #endregion
        this.router.navigate(['/products', this.productId]);
      },
      error: (error) => {
        // #region agent log
        fetch('http://127.0.0.1:7242/ingest/acf325a0-3256-4a55-9d83-c6c0009382cd',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'product-edit.component.ts:131',message:'Update request failed',data:{error:error.message,productId:this.productId},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'D'})}).catch(()=>{});
        // #endregion
        this.isSubmitting = false;
        this.error = error.message || 'Failed to update product';
      }
    });
  }
}

