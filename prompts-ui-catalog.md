# Prompts for ECommerceProductManagement Angular Frontend

---

## PROMPT 1 — Global UI Redesign (Amazon/Flipkart-Inspired)

> Use this prompt when asking Claude (or another AI) to redesign the full frontend UI.

---

```
You are working on an Angular 17+ standalone component project called ECommerceProductManagement.
The project uses Tailwind CSS and the existing components are plain/unstyled.

Your task is to REDESIGN the entire frontend UI to look and feel like a professional ecommerce
platform — inspired by Amazon, Flipkart, and Myntra — with a polished, production-grade design
system. Work ONLY on the frontend (HTML templates + CSS files). Do NOT touch any TypeScript
logic, services, guards, interceptors, or backend code.

---

### DESIGN SYSTEM TO IMPLEMENT

Apply the following design tokens consistently across all components.
Create or update: `src/styles.css` (global styles + CSS variables).

**Color Palette (Flipkart/Amazon hybrid):**
```css
:root {
  /* Primary brand - Flipkart blue */
  --color-primary: #2874F0;
  --color-primary-dark: #1a5dc8;
  --color-primary-light: #e8f0fe;

  /* Accent - Amazon orange */
  --color-accent: #FF9F00;
  --color-accent-dark: #e08c00;
  --color-accent-light: #fff4e0;

  /* Status colors */
  --color-success: #26A541;
  --color-warning: #FF9F00;
  --color-danger: #FF4141;
  --color-info: #2874F0;

  /* Neutrals */
  --color-bg: #F1F3F6;       /* Flipkart page background */
  --color-surface: #FFFFFF;
  --color-border: #E0E0E0;
  --color-text-primary: #212121;
  --color-text-secondary: #878787;
  --color-text-muted: #B0B0B0;

  /* Header */
  --color-header-bg: #2874F0;
  --color-header-text: #FFFFFF;

  /* Shadows */
  --shadow-card: 0 2px 8px rgba(0,0,0,0.10);
  --shadow-hover: 0 4px 16px rgba(40,116,240,0.18);
  --shadow-dropdown: 0 8px 24px rgba(0,0,0,0.15);

  /* Spacing */
  --radius-sm: 4px;
  --radius-md: 8px;
  --radius-lg: 12px;
  --radius-full: 9999px;

  /* Typography */
  --font-body: 'Roboto', sans-serif;
  --font-display: 'Roboto', sans-serif;
}
```

Add Google Font import at the top of styles.css:
```css
@import url('https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap');
```

---

### COMPONENT-BY-COMPONENT REDESIGN INSTRUCTIONS

#### 1. `navigation.component.html` + `navigation.component.css`

Redesign to look like the Flipkart/Amazon top header. Structure:

```
[HEADER - full width, bg: var(--color-primary)]
  [LEFT]  Logo text "ShopAdmin" in white bold + tagline "Management Portal"
  [CENTER] Search bar with magnifier icon (white border, white placeholder)
  [RIGHT]  User avatar icon + username + "Logout" button (white outline button)

[SECONDARY NAV BAR - white background, border-bottom, padding 0 16px]
  Horizontal pill links: Dashboard | Products | Categories | Users
  Active link: bold + blue underline
  Hover: light blue background pill
```

CSS requirements:
- Header height: 56px
- Box shadow: 0 2px 4px rgba(0,0,0,0.15)
- Logo font-size: 22px, font-weight: 700
- Search bar: border-radius 2px; width: 40%; background white; height: 36px
- Nav pills: padding 6px 16px; border-radius 4px; font-size 14px

---

#### 2. `login.component.html` + `login.component.css`

Redesign as a two-column page:

LEFT PANEL (50%): Deep blue gradient background (`#2874F0` to `#1a5dc8`). 
  - Large white text: "Welcome to ShopAdmin"
  - Subtitle: "Your complete ecommerce management platform"
  - Decorative shopping cart SVG icon (white, 80px)
  - 3 feature bullets with checkmarks (white text, 14px):
    - "Manage 10,000+ products effortlessly"
    - "Real-time catalog sync"
    - "Role-based access control"

RIGHT PANEL (50%): White background, centered card.
  - "Sign In" heading (24px, #212121)
  - Email + Password inputs: height 44px, border-radius 4px, border #E0E0E0
  - Focus state: border-color var(--color-primary), box-shadow 0 0 0 3px var(--color-primary-light)
  - Submit button: full width, background var(--color-primary), height 44px, font-size 16px, font-weight 500
  - Hover: var(--color-primary-dark)
  - "Forgot password?" link: right-aligned, color var(--color-primary)
  - Divider line + "Don't have an account? Register"

---

#### 3. `register.component.html` + `register.component.css`

Same two-column layout as login. Left panel: orange accent (`#FF9F00` to `#e08c00`).
Right panel: Registration form with:
- First Name + Last Name side by side (grid 2-col)
- Email, Password, Confirm Password stacked
- Role selector as styled dropdown
- Register button: full width, accent orange, white text
- All inputs same style as login

---

#### 4. `forgot-password.component.html` + `forgot-password.component.css`
#### `reset-password.component.html` + `reset-password.component.css`

Centered card layout (max-width 440px, centered on page).
Background: var(--color-bg).
Card: white, border-radius 8px, box-shadow var(--shadow-card), padding 40px.
- Envelope/lock icon at top (SVG, 48px, color var(--color-primary))
- Title 20px bold, subtitle 14px muted
- Input + button same design system

---

#### 5. `user-list.component.html` + `user-list.component.css`

Redesign as a full ecommerce admin data table page:

**Page structure:**
```
[PAGE HEADER]
  Title "User Management" (20px bold)  |  [+ Add User] button (blue, right)

[FILTER BAR - white card, padding 16px, margin-bottom 16px]
  Search input (left) | Role filter dropdown | Status filter | [Search] button

[TABLE CARD - white, border-radius 8px, shadow]
  Table with columns: Checkbox | Avatar+Name | Email | Role (badge) | Status (badge) | Actions
  
  Row hover: light blue background #F5F9FF
  
  Role badges:
    Admin → red pill (#FF4141 bg, white text)
    ProductManager → blue pill (#2874F0 bg, white text)  
    ContentExecutive → green pill (#26A541 bg, white text)
    User → grey pill (#878787 bg, white text)
  
  Status badges:
    Active → green dot + "Active"
    Inactive → grey dot + "Inactive"
  
  Action buttons: icon-only (Edit pencil = blue, Delete trash = red), 32px, border-radius 4px

[PAGINATION BAR]
  "Showing 1-10 of 45 results" | Prev < 1 2 3 4 5 > Next
  Pagination buttons: 32px square, border-radius 4px, active = blue filled
```

---

#### 6. `view-profile.component.html` + `view-profile.component.css`

Redesign as a profile page:

```
[PROFILE HERO CARD - white, border-radius 8px, shadow, padding 32px]
  Large avatar circle (80px, blue bg, white initials, 2px blue border)
  Name (24px bold) + Role badge + Email
  [Edit Profile] button (blue outline) | [Change Password] button (grey outline)

[INFO GRID - 2 columns below]
  Card 1: Account Details (joined date, last login, account status)
  Card 2: Role & Permissions info
```

---

#### 7. `edit-profile.component.html` + `edit-profile.component.css`
#### `change-password.component.html` + `change-password.component.css`

Card layout (max-width 600px):
- Section title + breadcrumb ("Profile > Edit Profile")
- Form with same input design system
- Save/Cancel button row at bottom (Save = blue filled, Cancel = grey outline)

---

#### 8. `loading-spinner.component.html` + `loading-spinner.component.css`

Full-screen overlay with centered Flipkart-style spinner:
- Semi-transparent dark overlay (rgba(0,0,0,0.4))
- White card (80px × 80px, border-radius 8px)
- Blue circular spinner (border 3px, animation: spin 0.8s linear infinite)
- Small "Loading..." text below (12px, #878787)

---

#### 9. `confirm-dialog.component.html` + `confirm-dialog.component.css`

Modal dialog:
- Overlay: rgba(0,0,0,0.5) backdrop
- Card: white, 400px wide, border-radius 8px, shadow
- Header: warning triangle icon (orange) + title
- Body: message text (14px, #212121)
- Footer: [Cancel] grey outline + [Confirm] red filled buttons

---

#### 10. Global `styles.css` additions

Add global resets and utility classes:
```css
/* Card */
.ec-card { background: var(--color-surface); border-radius: var(--radius-md); box-shadow: var(--shadow-card); }

/* Buttons */
.btn-primary { background: var(--color-primary); color: white; border: none; padding: 8px 20px; border-radius: 4px; font-weight: 500; cursor: pointer; transition: background 0.2s; }
.btn-primary:hover { background: var(--color-primary-dark); }
.btn-accent { background: var(--color-accent); color: white; }
.btn-outline { background: transparent; border: 1.5px solid var(--color-primary); color: var(--color-primary); }
.btn-danger { background: var(--color-danger); color: white; }

/* Form inputs */
.ec-input { width: 100%; height: 44px; padding: 0 12px; border: 1.5px solid var(--color-border); border-radius: 4px; font-size: 14px; color: var(--color-text-primary); transition: border-color 0.2s, box-shadow 0.2s; }
.ec-input:focus { outline: none; border-color: var(--color-primary); box-shadow: 0 0 0 3px var(--color-primary-light); }
.ec-input.error { border-color: var(--color-danger); }

/* Page layout */
.page-container { max-width: 1280px; margin: 0 auto; padding: 24px 16px; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.page-title { font-size: 20px; font-weight: 700; color: var(--color-text-primary); }

/* Badges */
.badge { display: inline-flex; align-items: center; padding: 3px 10px; border-radius: 12px; font-size: 12px; font-weight: 500; }
```

Also update `app.component.css`:
```css
.main-content {
  min-height: calc(100vh - 96px);  /* account for dual nav bar */
  background: var(--color-bg);
  padding-top: 16px;
}
```

---

### GENERAL RULES
- Keep ALL existing Angular template syntax: `*ngIf`, `[formGroup]`, `(click)`, `routerLink`, `| async`, `@if`, etc. — do NOT touch them.
- Keep ALL class names that are referenced in the `.ts` files unchanged; only ADD new classes or REPLACE purely cosmetic ones.
- Every component `.css` file must use the CSS variables defined in `styles.css` — no hardcoded hex colors.
- Make everything responsive: use flexbox/grid, ensure mobile breakpoints at 768px.
- No external UI libraries — only Tailwind utility classes and custom CSS.
- Transitions on all interactive elements: `transition: all 0.2s ease`.
```

---

---

## PROMPT 2 — Catalog Feature (Frontend Only: Products & Categories)

> Use this prompt when asking Claude to build the full Catalog module in the Angular frontend.

---

```
You are building a new feature module for the ECommerceProductManagement Angular 17+ frontend.
The backend Catalog.API is already running at the API Gateway base URL: `https://localhost:7292`.

Work ONLY on the frontend. Do NOT modify any backend files.

The project already has:
- Auth interceptor that attaches Bearer tokens automatically (src/app/core/interceptors/auth.interceptor.ts)
- NotificationService for toast alerts (src/app/core/services/notification.service.ts)
- LoadingService for spinner overlay (src/app/core/services/loading.service.ts)
- Shared ConfirmDialogComponent for delete confirmations

---

### BACKEND API REFERENCE

Base URL (via Ocelot gateway): `https://localhost:7292`

**Products**
- GET    /api/products                          → ProductResponseDto[]  (query params: categoryId?, status?)
- GET    /api/products/{id}                     → ProductResponseDto
- POST   /api/products                          → ProductResponseDto    (requires Admin/ProductManager/ContentExecutive)
- PUT    /api/products/{id}                     → 204                   (requires Admin/ProductManager)
- DELETE /api/products/{id}                     → 204                   (requires Admin)
- GET    /api/products/{productId}/variants     → ProductVariantResponseDto[]
- GET    /api/products/{productId}/variants/{id}
- POST   /api/products/{productId}/variants     → ProductVariantResponseDto (requires Admin/ProductManager/ContentExecutive)
- PUT    /api/products/{productId}/variants/{id}→ 204 (requires Admin/ProductManager)
- DELETE /api/products/{productId}/variants/{id}→ 204 (requires Admin)

**Categories**
- GET    /api/categories                        → CategoryResponseDto[]
- GET    /api/categories/{id}                   → CategoryResponseDto
- POST   /api/categories                        → CategoryResponseDto   (requires Admin/ProductManager)
- PUT    /api/categories/{id}                   → 204                   (requires Admin/ProductManager)
- DELETE /api/categories/{id}                   → 204                   (requires Admin)

**DTOs (TypeScript interfaces to create in `src/app/features/catalog/models/`):**

```typescript
// product.model.ts
export interface ProductResponse {
  id: string;
  name: string;
  sku: string;
  brand: string;
  description: string;
  publishStatus: PublishStatus;
  categoryName: string;
}

export interface CreateProduct {
  name: string;
  brand: string;
  description: string;
  categoryId: string;
}

export interface UpdateProduct {
  name: string;
  brand: string;
  description: string;
  categoryId: string;
  publishStatus: PublishStatus;
}

export enum PublishStatus {
  Draft = 0,
  Published = 1,
  Archived = 2
}

// category.model.ts
export interface CategoryResponse {
  id: string;
  name: string;
  parentCategoryId?: string;
  parentCategoryName: string;
}

export interface CreateCategory {
  name: string;
  parentCategoryId?: string;
}

// product-variant.model.ts
export interface ProductVariantResponse {
  id: string;
  productId: string;
  color: string;
  size: string;
  barcode: string;
}

export interface CreateProductVariant {
  color: string;
  size: string;
}

export interface UpdateProductVariant {
  color: string;
  size: string;
}
```

---

### FILES TO CREATE

#### A. Services

**`src/app/features/catalog/services/catalog.service.ts`**

Angular injectable service using `HttpClient`. Methods:
- `getProducts(categoryId?: string, status?: number): Observable<ProductResponse[]>`
- `getProductById(id: string): Observable<ProductResponse>`
- `createProduct(dto: CreateProduct): Observable<ProductResponse>`
- `updateProduct(id: string, dto: UpdateProduct): Observable<void>`
- `deleteProduct(id: string): Observable<void>`
- `getVariants(productId: string): Observable<ProductVariantResponse[]>`
- `createVariant(productId: string, dto: CreateProductVariant): Observable<ProductVariantResponse>`
- `updateVariant(productId: string, id: string, dto: UpdateProductVariant): Observable<void>`
- `deleteVariant(productId: string, id: string): Observable<void>`

**`src/app/features/catalog/services/category.service.ts`**

Angular injectable service using `HttpClient`. Methods:
- `getCategories(): Observable<CategoryResponse[]>`
- `getCategoryById(id: string): Observable<CategoryResponse>`
- `createCategory(dto: CreateCategory): Observable<CategoryResponse>`
- `updateCategory(id: string, dto: Partial<CategoryResponse>): Observable<void>`
- `deleteCategory(id: string): Observable<void>`

---

#### B. Product List Page
**`src/app/features/catalog/products/product-list/product-list.component.ts`**
**`src/app/features/catalog/products/product-list/product-list.component.html`**
**`src/app/features/catalog/products/product-list/product-list.component.css`**

UI layout (ecommerce admin style, matching the global design system):

```
[PAGE HEADER]
  Title "Product Catalog"   |   [+ Add Product] button (blue, top right)

[FILTER BAR - white card]
  Search by name input (left, 300px)
  | Category filter dropdown (populated from GET /api/categories)
  | Status filter dropdown: All / Draft / Published / Archived
  | [Search] button

[STATS STRIP - 3 mini cards]
  Total Products | Published | Draft

[PRODUCT TABLE - white card]
  Columns: # | Name + Brand (stacked) | SKU | Category | Status Badge | Actions
  
  Status badges:
    Published → green (#26A541)
    Draft → orange (#FF9F00)
    Archived → grey (#878787)
  
  Action buttons per row:
    [View Variants] (blue outline, eye icon)
    [Edit] (blue icon button)
    [Delete] (red icon button, Admin only — hide for other roles)
  
  Empty state: shopping bag SVG icon + "No products found. Add your first product."

[PAGINATION] same style as user-list
```

TypeScript behaviour:
- Load products on init with `ngOnInit`
- Filter/search triggers new API call
- Delete calls `ConfirmDialogComponent`, then `deleteProduct()`, then reload list
- Role-based visibility: hide [+ Add Product] and [Delete] for non-privileged roles
  (check role from `AuthService` or stored JWT claims)
- Show `LoadingService` spinner during API calls
- Use `NotificationService.success()` / `.error()` for feedback

---

#### C. Product Form (Create + Edit)
**`src/app/features/catalog/products/product-form/product-form.component.ts`**
**`src/app/features/catalog/products/product-form/product-form.component.html`**
**`src/app/features/catalog/products/product-form/product-form.component.css`**

Reusable form for both create and edit. Detect mode via `ActivatedRoute` param (`id` present = edit).

Form fields:
- Product Name* (text, maxLength 200)
- Brand* (text, maxLength 200)
- Description (textarea, maxLength 500, show char count)
- Category* (dropdown populated from GET /api/categories, show parent > child hierarchy)
- Publish Status (dropdown: Draft/Published/Archived) — show only in edit mode

Layout: single-column card, max-width 720px, breadcrumb "Products > Add Product".
Validation: reactive form with `Validators.required`, `Validators.maxLength`.
Submit: call `createProduct()` or `updateProduct()`, then navigate back to `/catalog/products`.

---

#### D. Product Variants Panel
**`src/app/features/catalog/products/product-variants/product-variants.component.ts`**
**`src/app/features/catalog/products/product-variants/product-variants.component.html`**
**`src/app/features/catalog/products/product-variants/product-variants.component.css`**

Rendered as a slide-in panel or sub-page at `/catalog/products/:productId/variants`.

Layout:
```
[HEADER] "Variants — {ProductName}"  |  [+ Add Variant] button

[VARIANTS TABLE]
  Columns: # | Color | Size | Barcode | Actions (Edit / Delete)
  
  Inline edit row: clicking Edit turns the row into an editable form inline
  [Save] / [Cancel] inline buttons

[ADD VARIANT FORM - shown below table when "+ Add Variant" clicked]
  Color (text) | Size (text) | [Save] [Cancel]
```

---

#### E. Category List Page
**`src/app/features/catalog/categories/category-list/category-list.component.ts`**
**`src/app/features/catalog/categories/category-list/category-list.component.html`**
**`src/app/features/catalog/categories/category-list/category-list.component.css`**

```
[PAGE HEADER]
  "Categories"  |  [+ Add Category] button

[CATEGORY TABLE - white card]
  Columns: # | Category Name | Parent Category | Products Count (if available) | Actions
  
  Parent category shown as grey pill badge
  Root categories (no parent): bold name, no badge
  
  Actions: [Edit] (inline edit row) | [Delete] (confirm dialog)

[ADD / EDIT - inline row form]
  Name input | Parent dropdown (list of root categories) | [Save] [Cancel]
```

---

#### F. Routes

Update `src/app/app.routes.ts` — add inside the existing `canActivate: [authGuard]` children
(or as a new protected route block):

```typescript
{
  path: 'catalog',
  canActivate: [authGuard],
  children: [
    { path: '', redirectTo: 'products', pathMatch: 'full' },
    {
      path: 'products',
      loadComponent: () => import('./features/catalog/products/product-list/product-list.component')
        .then(m => m.ProductListComponent)
    },
    {
      path: 'products/new',
      loadComponent: () => import('./features/catalog/products/product-form/product-form.component')
        .then(m => m.ProductFormComponent)
    },
    {
      path: 'products/:id/edit',
      loadComponent: () => import('./features/catalog/products/product-form/product-form.component')
        .then(m => m.ProductFormComponent)
    },
    {
      path: 'products/:productId/variants',
      loadComponent: () => import('./features/catalog/products/product-variants/product-variants.component')
        .then(m => m.ProductVariantsComponent)
    },
    {
      path: 'categories',
      loadComponent: () => import('./features/catalog/categories/category-list/category-list.component')
        .then(m => m.CategoryListComponent)
    }
  ]
}
```

Also add nav links in `navigation.component.html`:
- "Products" → `/catalog/products`
- "Categories" → `/catalog/categories`

---

### DESIGN REQUIREMENTS (match global design system from Prompt 1)

All catalog pages must use:
- Page background: `var(--color-bg)` (#F1F3F6)
- Cards: `var(--color-surface)` white, `var(--shadow-card)`, `var(--radius-md)`
- Primary buttons: `var(--color-primary)` blue
- Danger buttons: `var(--color-danger)` red
- Status badges: colored pills with dot indicators
- Tables: alternating row hover `#F5F9FF`, no heavy borders — only bottom border per row
- Font: Roboto via CSS variable
- All inputs: `.ec-input` class from global styles
- Mobile responsive: table horizontal scroll on < 768px

---

### GENERAL RULES
- All components are Angular standalone (`standalone: true`)
- Import `CommonModule`, `ReactiveFormsModule`, `RouterModule`, `HttpClientModule` as needed
- Use `inject()` function (not constructor injection) where clean
- Use `AsyncPipe` for observables in templates where possible
- Error handling: catch HTTP errors in service, re-throw; component catches and calls `NotificationService.error()`
- No external component libraries (no Angular Material, PrimeNG, etc.)
- All new CSS uses the design token CSS variables from `styles.css`
```

---

## Quick Reference — Project Structure

```
Frontend/src/app/
├── app.routes.ts                ← Add catalog routes here
├── features/
│   ├── catalog/                 ← CREATE THIS ENTIRE FOLDER
│   │   ├── models/
│   │   │   ├── product.model.ts
│   │   │   ├── category.model.ts
│   │   │   └── product-variant.model.ts
│   │   ├── services/
│   │   │   ├── catalog.service.ts
│   │   │   └── category.service.ts
│   │   ├── products/
│   │   │   ├── product-list/
│   │   │   ├── product-form/
│   │   │   └── product-variants/
│   │   └── categories/
│   │       └── category-list/
│   ├── auth/                    ← Existing (redesign HTML+CSS only)
│   ├── admin/                   ← Existing (redesign HTML+CSS only)
│   └── profile/                 ← Existing (redesign HTML+CSS only)
└── shared/components/
    ├── navigation/              ← Redesign HTML+CSS
    ├── loading-spinner/         ← Redesign HTML+CSS
    └── confirm-dialog/          ← Redesign HTML+CSS
```
