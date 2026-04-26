# Identity UI - Angular Frontend

This is the Angular frontend application for the Identity/Auth microservice. It provides a modern, responsive user interface for authentication, user profile management, and administrative user management.

## Features

- User authentication (login, register, logout)
- Password management (forgot password, reset password, change password)
- User profile management
- Admin user management (view, edit roles, activate/deactivate, delete users)
- JWT-based authentication with automatic token refresh
- Responsive design for mobile, tablet, and desktop
- Route guards for authentication and role-based access control
- HTTP interceptors for centralized request/response handling

## Technology Stack

- **Framework**: Angular 17
- **Language**: TypeScript 5+
- **UI Components**: Angular Material 17
- **HTTP Client**: Angular HttpClient
- **Forms**: Angular Reactive Forms
- **State Management**: RxJS BehaviorSubjects
- **Testing**: Jasmine, Karma, fast-check (property-based testing)
- **Styling**: SCSS

## Project Structure

```
src/app/
├── core/                    # Core singleton services
│   ├── services/           # Auth, User, TokenStorage services
│   ├── interceptors/       # HTTP interceptors
│   └── guards/             # Route guards
├── shared/                 # Shared components and utilities
│   ├── components/         # Reusable components
│   └── models/             # Interfaces and types
├── features/               # Feature modules
│   ├── auth/              # Authentication pages
│   ├── profile/           # User profile pages
│   └── admin/             # Admin pages
└── app.routes.ts          # Application routes
```

## Prerequisites

- Node.js 18+ (currently using v22.16.0)
- npm 9+ (currently using v11.12.1)
- Angular CLI 17

## Installation

```bash
npm install
```

## Development Server

Run the development server:

```bash
npm start
```

Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Build

Build the project for production:

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

## Running Tests

Execute unit tests via Karma:

```bash
npm test
```

## Environment Configuration

The application uses environment files for configuration:

- `src/environments/environment.ts` - Development environment
- `src/environments/environment.prod.ts` - Production environment

Update the `apiUrl` in these files to point to your Identity.API backend service.

## API Integration

The frontend communicates with the Identity.API backend service. Default API URL is `http://localhost:5000`.

## Authentication Flow

1. User logs in with email and password
2. Backend returns access token and refresh token
3. Tokens are stored in localStorage
4. Access token is attached to all authenticated requests via AuthInterceptor
5. On 401 response, AuthInterceptor automatically refreshes the token
6. On refresh failure, user is redirected to login page

## Route Guards

- **AuthGuard**: Protects routes requiring authentication
- **RoleGuard**: Protects routes requiring specific roles (e.g., Admin)

## HTTP Interceptors

- **AuthInterceptor**: Attaches JWT token to requests and handles token refresh
- **ErrorInterceptor**: Centralized error handling and user notifications
- **LoadingInterceptor**: Manages global loading state

## Further Help

To get more help on the Angular CLI use `ng help` or check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.
