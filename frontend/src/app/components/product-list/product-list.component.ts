import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Observable, of, BehaviorSubject, combineLatest } from 'rxjs';
import { catchError, map, startWith, switchMap, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ProductService, SearchProductsParams } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { Product, SearchProductsResponse } from '../../models/product.model';
import { Category } from '../../models/category.model';

interface ProductListState {
  products: Product[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  isLoading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductListComponent {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private fb = inject(FormBuilder);

  categories$ = this.categoryService.getCategories().pipe(
    catchError(() => of([] as Category[]))
  );

  filterForm = this.fb.group({
    searchTerm: [''],
    categoryId: [null as number | null],
    minPrice: [null as number | null],
    maxPrice: [null as number | null],
    inStock: [null as boolean | null],
    sortBy: [''],
    sortOrder: ['asc']
  });

  private filterParams$ = new BehaviorSubject<SearchProductsParams>({
    pageNumber: 1,
    pageSize: 10
  });

  state$: Observable<ProductListState> = combineLatest([
    this.filterParams$,
    this.categories$
  ]).pipe(
    switchMap(([params]) => 
      this.productService.searchProducts(params).pipe(
        map(response => ({
          products: response.items,
          totalCount: response.totalCount,
          pageNumber: response.pageNumber,
          pageSize: response.pageSize,
          totalPages: response.totalPages,
          isLoading: false,
          error: null
        } as ProductListState)),
        startWith({
          products: [],
          totalCount: 0,
          pageNumber: 1,
          pageSize: 10,
          totalPages: 0,
          isLoading: true,
          error: null
        } as ProductListState),
        catchError(error => of({
          products: [],
          totalCount: 0,
          pageNumber: 1,
          pageSize: 10,
          totalPages: 0,
          isLoading: false,
          error: error.message || 'Failed to load products'
        } as ProductListState))
      )
    )
  );

  constructor() {
    // Watch form changes and update filter params
    this.filterForm.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(value => {
      const params: SearchProductsParams = {
        pageNumber: 1,
        pageSize: 10
      };

      if (value.searchTerm?.trim()) {
        params.searchTerm = value.searchTerm.trim();
      }
      if (value.categoryId !== null && value.categoryId !== undefined) {
        params.categoryId = value.categoryId;
      }
      if (value.minPrice !== null && value.minPrice !== undefined) {
        params.minPrice = value.minPrice;
      }
      if (value.maxPrice !== null && value.maxPrice !== undefined) {
        params.maxPrice = value.maxPrice;
      }
      if (value.inStock !== null && value.inStock !== undefined) {
        params.inStock = value.inStock;
      }
      if (value.sortBy) {
        params.sortBy = value.sortBy;
      }
      if (value.sortOrder) {
        params.sortOrder = value.sortOrder;
      }

      this.filterParams$.next(params);
    });
  }

  clearFilters(): void {
    this.filterForm.reset({
      searchTerm: '',
      categoryId: null,
      minPrice: null,
      maxPrice: null,
      inStock: null,
      sortBy: '',
      sortOrder: 'asc'
    });
  }

  goToPage(page: number, state: ProductListState): void {
    if (page >= 1 && page <= state.totalPages) {
      const params = { ...this.filterParams$.value };
      params.pageNumber = page;
      this.filterParams$.next(params);
    }
  }
}
