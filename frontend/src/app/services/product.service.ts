import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, shareReplay } from 'rxjs/operators';
import { Product, SearchProductsResponse, CreateProductRequest, UpdateProductRequest } from '../models/product.model';
import { HttpErrorResponse } from '@angular/common/http';

export interface SearchProductsParams {
  searchTerm?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
  sortBy?: string;
  sortOrder?: string;
  pageNumber?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private http = inject(HttpClient);
  private apiUrl = '/api/products';

  getProducts(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl).pipe(
      shareReplay(1), // Cache the response
      catchError(this.handleError)
    );
  }

  searchProducts(params: SearchProductsParams = {}): Observable<SearchProductsResponse> {
    let httpParams = new HttpParams();
    
    if (params.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }
    if (params.categoryId !== undefined && params.categoryId !== null) {
      httpParams = httpParams.set('categoryId', params.categoryId.toString());
    }
    if (params.minPrice !== undefined) {
      httpParams = httpParams.set('minPrice', params.minPrice.toString());
    }
    if (params.maxPrice !== undefined) {
      httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
    }
    if (params.inStock !== undefined && params.inStock !== null) {
      httpParams = httpParams.set('inStock', params.inStock.toString());
    }
    if (params.sortBy) {
      httpParams = httpParams.set('sortBy', params.sortBy);
    }
    if (params.sortOrder) {
      httpParams = httpParams.set('sortOrder', params.sortOrder);
    }
    if (params.pageNumber !== undefined) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<SearchProductsResponse>(`${this.apiUrl}/search`, { params: httpParams }).pipe(
      catchError(this.handleError)
    );
  }

  getProductById(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  createProduct(product: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product).pipe(
      catchError(this.handleError)
    );
  }

  updateProduct(id: number, product: UpdateProductRequest): Observable<void> {
    // #region agent log
    fetch('http://127.0.0.1:7242/ingest/acf325a0-3256-4a55-9d83-c6c0009382cd',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({location:'product.service.ts:82',message:'HTTP PUT request being sent',data:{id,product,isActiveInRequest:product.isActive,requestBody:JSON.stringify(product)},timestamp:Date.now(),sessionId:'debug-session',runId:'run1',hypothesisId:'B'})}).catch(()=>{});
    // #endregion
    return this.http.put<void>(`${this.apiUrl}/${id}`, product).pipe(
      catchError(this.handleError)
    );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      catchError(this.handleError)
    );
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An error occurred while fetching products.';
    
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Network error: ${error.error.message}`;
    } else {
      if (error.error && typeof error.error === 'object' && 'message' in error.error) {
        errorMessage = (error.error as { message: string }).message;
      } else if (error.status === 0) {
        errorMessage = 'Unable to connect to the server. Please check your connection.';
      } else if (error.status === 404) {
        errorMessage = 'Product not found.';
      } else if (error.status === 400) {
        errorMessage = error.error?.message || 'Invalid request.';
      }
    }

    return throwError(() => new Error(errorMessage));
  }
}
