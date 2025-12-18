export interface ErrorResponse {
  message: string;
  details?: string;
  errors?: Record<string, string[]>;
}

