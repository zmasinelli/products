import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { CategoryService } from '../../services/category.service';
import { UpdateCategoryRequest } from '../../models/category.model';

@Component({
  selector: 'app-category-edit',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './category-edit.component.html',
  styleUrl: './category-edit.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryEditComponent {
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  categoryForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(255)]],
    description: [''],
    isActive: [true, []]
  });

  isSubmitting = false;
  isLoading = true;
  error: string | null = null;
  categoryId: number | null = null;

  category$ = this.route.paramMap.pipe(
    map(params => {
      const id = params.get('id');
      return id ? parseInt(id, 10) : null;
    }),
    switchMap(id => {
      if (!id || isNaN(id)) {
        this.isLoading = false;
        this.error = 'Invalid category ID';
        return of(null);
      }

      this.categoryId = id;
      return this.categoryService.getCategoryById(id).pipe(
        catchError(error => {
          this.isLoading = false;
          this.error = error.message || 'Failed to load category';
          return of(null);
        })
      );
    })
  );

  constructor() {
    this.category$.subscribe(category => {
      if (category) {
        this.isLoading = false;
        this.categoryForm.patchValue({
          name: category.name,
          description: category.description || '',
          isActive: category.isActive === true || category.isActive === false ? category.isActive : true
        });
      }
    });
  }

  get name() {
    return this.categoryForm.get('name');
  }

  get description() {
    return this.categoryForm.get('description');
  }

  onSubmit(): void {
    if (this.categoryForm.invalid || this.isSubmitting || !this.categoryId) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const formValue = this.categoryForm.getRawValue();
    const isActiveValue = this.categoryForm.get('isActive')?.value ?? true;
    
    const category: UpdateCategoryRequest = {
      name: formValue.name,
      description: formValue.description || null,
      isActive: isActiveValue
    };

    this.categoryService.updateCategory(this.categoryId, category).subscribe({
      next: () => {
        this.router.navigate(['/categories']);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.error = error.message || 'Failed to update category';
      }
    });
  }
}

