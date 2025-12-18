import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { CreateCategoryRequest } from '../../models/category.model';

@Component({
  selector: 'app-category-create',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './category-create.component.html',
  styleUrl: './category-create.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryCreateComponent {
  private categoryService = inject(CategoryService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  categoryForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(255)]],
    description: ['']
  });

  isSubmitting = false;
  error: string | null = null;

  get name() {
    return this.categoryForm.get('name');
  }

  get description() {
    return this.categoryForm.get('description');
  }

  onSubmit(): void {
    if (this.categoryForm.invalid || this.isSubmitting) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const formValue = this.categoryForm.value;
    const category: CreateCategoryRequest = {
      name: formValue.name,
      description: formValue.description || null
    };

    this.categoryService.createCategory(category).subscribe({
      next: (createdCategory) => {
        this.router.navigate(['/categories']);
      },
      error: (error) => {
        this.isSubmitting = false;
        this.error = error.message || 'Failed to create category';
      }
    });
  }
}

