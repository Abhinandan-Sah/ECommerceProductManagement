/**
 * Barrel export file for HTTP interceptors.
 * 
 * Interceptors are registered in the following order in app.config.ts:
 * 1. authInterceptor - Attaches authentication tokens to requests
 * 2. errorInterceptor - Handles errors and token refresh
 * 3. loadingInterceptor - Manages loading state
 * 
 * Order matters: Auth must run first to add tokens, error must run second
 * to handle 401s and refresh tokens, loading runs last to track all requests.
 */

export { authInterceptor } from './auth.interceptor';
export { errorInterceptor } from './error.interceptor';
export { loadingInterceptor } from './loading.interceptor';
