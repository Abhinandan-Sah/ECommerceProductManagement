import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  // PUBLIC — no auth required
  { path: '', loadComponent: () => import('./features/storefront/home/home.component').then(m => m.HomeComponent) },
  { path: 'store', loadComponent: () => import('./features/storefront/home/home.component').then(m => m.HomeComponent) },
  { path: 'store/products', loadComponent: () => import('./features/storefront/product-browse/product-browse.component').then(m => m.ProductBrowseComponent) },

  // AUTH routes
  { path: 'login',    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent) },

  // DASHBOARD — role-based redirect after login
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard-redirect/dashboard-redirect.component').then(m => m.DashboardRedirectComponent)
  },

  // ADMIN DASHBOARD (role: Admin)
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Admin'] },
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
      { path: 'users', loadComponent: () => import('./features/admin/user-list/user-list.component').then(m => m.UserListComponent) },
    ]
  },

  // PRODUCT MANAGER DASHBOARD (role: ProductManager)
  {
    path: 'product-manager',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ProductManager'] },
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/product-manager-dashboard/product-manager-dashboard.component').then(m => m.ProductManagerDashboardComponent) },
    ]
  },

  // CONTENT EXECUTIVE DASHBOARD (role: ContentExecutive)
  {
    path: 'content-executive',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ContentExecutive'] },
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/content-executive-dashboard/content-executive-dashboard.component').then(m => m.ContentExecutiveDashboardComponent) },
    ]
  },

  // CUSTOMER DASHBOARD (role: Customer)
  {
    path: 'my-account',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Customer'] },
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/customer-dashboard/customer-dashboard.component').then(m => m.CustomerDashboardComponent) },
    ]
  },

  // CATALOG (Admin + ProductManager + ContentExecutive)
  {
    path: 'catalog',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'products', pathMatch: 'full' },
      { path: 'products', loadComponent: () => import('./features/catalog/products/product-list/product-list.component').then(m => m.ProductListComponent) },
      { path: 'products/new', loadComponent: () => import('./features/catalog/products/product-form/product-form.component').then(m => m.ProductFormComponent) },
      { path: 'products/:id/edit', loadComponent: () => import('./features/catalog/products/product-form/product-form.component').then(m => m.ProductFormComponent) },
      { path: 'products/:productId/variants', loadComponent: () => import('./features/catalog/products/product-variants/product-variants.component').then(m => m.ProductVariantsComponent) },
      { path: 'categories', loadComponent: () => import('./features/catalog/categories/category-list/category-list.component').then(m => m.CategoryListComponent) },
    ]
  },

  // PROFILE (all authenticated users)
  {
    path: 'profile',
    canActivate: [authGuard],
    children: [
      { path: '', loadComponent: () => import('./features/profile/view-profile/view-profile.component').then(m => m.ViewProfileComponent) },
      { path: 'edit', loadComponent: () => import('./features/profile/edit-profile/edit-profile.component').then(m => m.EditProfileComponent) },
      { path: 'change-password', loadComponent: () => import('./features/profile/change-password/change-password.component').then(m => m.ChangePasswordComponent) },
    ]
  },

  { path: 'unauthorized', loadComponent: () => import('./shared/components/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent) },
  { path: '**', redirectTo: '/' }
];
