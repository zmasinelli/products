import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, map, startWith } from 'rxjs/operators';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/category.model';

interface CategoryListState {
  categories: Category[];
  isLoading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './category-list.component.html',
  styleUrl: './category-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryListComponent {
  private categoryService = inject(CategoryService);

  state$: Observable<CategoryListState> = this.categoryService.getCategories().pipe(
    map(categories => ({
      categories,
      isLoading: false,
      error: null
    } as CategoryListState)),
    startWith({
      categories: [],
      isLoading: true,
      error: null
    } as CategoryListState),
    catchError(error => of({
      categories: [],
      isLoading: false,
      error: error.message || 'Failed to load categories'
    } as CategoryListState))
  );
}

