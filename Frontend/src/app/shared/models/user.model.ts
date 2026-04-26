export enum Role {
  Admin            = 'Admin',
  ProductManager   = 'ProductManager',
  ContentExecutive = 'ContentExecutive',
  Customer         = 'Customer'
}

export interface User {
  id:        string;
  fullName:  string;
  email:     string;
  role:      Role;
  isActive:  boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateProfileRequest {
  fullName: string;
  email:    string;
}

export interface UpdateRoleRequest {
  role: string;
}

export interface UpdateStatusRequest {
  isActive: boolean;
}

export interface PaginatedResponse<T> {
  data:       T[];
  total:      number;
  page:       number;
  pageSize:   number;
  totalPages: number;
}
