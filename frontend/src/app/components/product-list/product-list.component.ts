import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Observable, of } from 'rxjs';
import { catchError, map, startWith } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product.model';

interface ProductListState {
  products: Product[];
  isLoading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductListComponent {
  private productService = inject(ProductService);

  state$: Observable<ProductListState> = this.productService.getProducts().pipe(
    map(products => ({
      products,
      isLoading: false,
      error: null
    })),
    startWith({
      products: [],
      isLoading: true,
      error: null
    } as ProductListState),
    catchError(error => of({
      products: [],
      isLoading: false,
      error: error.message || 'Failed to load products'
    } as ProductListState))
  );
}
