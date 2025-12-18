import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model';

interface ProductDetailsState {
  product: Product | null;
  isLoading: boolean;
  error: string | null;
  isDeleting: boolean;
}

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetailsComponent {
  private productService = inject(ProductService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  private productId$ = this.route.paramMap.pipe(
    map(params => {
      const id = params.get('id');
      return id ? parseInt(id, 10) : null;
    })
  );

  state$: Observable<ProductDetailsState> = this.productId$.pipe(
    switchMap(id => {
      if (!id || isNaN(id)) {
        return of({
          product: null,
          isLoading: false,
          error: 'Invalid product ID',
          isDeleting: false
        } as ProductDetailsState);
      }

      return this.productService.getProductById(id).pipe(
        map(product => ({
          product,
          isLoading: false,
          error: null,
          isDeleting: false
        } as ProductDetailsState)),
        startWith({
          product: null,
          isLoading: true,
          error: null,
          isDeleting: false
        } as ProductDetailsState),
        catchError(error => of({
          product: null,
          isLoading: false,
          error: error.message || 'Failed to load product',
          isDeleting: false
        } as ProductDetailsState))
      );
    })
  );

  deleteProduct(id: number): void {
    if (!confirm('Are you sure you want to delete this product? This action cannot be undone.')) {
      return;
    }

    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.router.navigate(['/products']);
      },
      error: (error) => {
        alert('Failed to delete product: ' + (error.message || 'Unknown error'));
      }
    });
  }
}

