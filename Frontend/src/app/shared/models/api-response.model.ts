export interface ApiResponse<T> {
  success: boolean;
  data:    T;
  message: string;
  errors:  string[];
}

export interface ApiError {
  message:    string;
  statusCode: number;
  errors:     string[];
}
