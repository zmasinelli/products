import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { catchError, throwError } from 'rxjs';
import { ErrorResponse } from '../models/error-response.model';

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  // Clone request and prepend base URL to relative URLs
  const apiUrl = environment.apiUrl;
  let apiReq = req;

  if (!req.url.startsWith('http://') && !req.url.startsWith('https://')) {
    apiReq = req.clone({
      url: `${apiUrl}${req.url}`
    });
  }

  return next(apiReq).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An error occurred while processing your request.';

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Network error: ${error.error.message}`;
      } else {
        // Server-side error
        if (error.error && typeof error.error === 'object') {
          const errorResponse = error.error as ErrorResponse;
          if (errorResponse.message) {
            errorMessage = errorResponse.message;
          }
        } else if (error.status === 0) {
          errorMessage = 'Unable to connect to the server. Please check your connection.';
        } else if (error.status >= 500) {
          errorMessage = 'Server error. Please try again later.';
        } else if (error.status === 404) {
          errorMessage = 'Resource not found.';
        }
      }

      // Log error in development
      if (!environment.production) {
        console.error('API Error:', error);
      }

      // Re-throw with user-friendly message
      return throwError(() => new Error(errorMessage));
    })
  );
};
