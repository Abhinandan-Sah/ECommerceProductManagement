import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest } from '../../shared/models/auth.model';
import { UserRole } from '../../shared/models/user.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let tokenStorage: TokenStorageService;
  let router: Router;
  const apiUrl = `${environment.apiUrl}/api/auth`;

  // Mock JWT token with valid structure
  const mockAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwiZW1haWwiOiJ0ZXN0QGV4YW1wbGUuY29tIiwicm9sZSI6IlVzZXIiLCJleHAiOjk5OTk5OTk5OTksImlhdCI6MTUxNjIzOTAyMn0.4Adcj0vfIRa7KSKYz5lZpZqLqYcP5fJZ5z5z5z5z5z5';
  const mockRefreshToken = 'mock-refresh-token';

  beforeEach(() => {
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        TokenStorageService,
        { provide: Router, useValue: routerSpy }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    tokenStorage = TestBed.inject(TokenStorageService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpMock.verify();
    tokenStorage.clearTokens();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('register', () => {
    it('should send POST request to register endpoint', () => {
      const registerData: RegisterRequest = {
        email: 'test@example.com',
        password: 'Password123!',
        firstName: 'Test',
        lastName: 'User'
      };

      service.register(registerData).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        email: registerData.email,
        password: registerData.password,
        fullName: `${registerData.firstName} ${registerData.lastName}`
      });
      req.flush(null);
    });
  });

  describe('login', () => {
    it('should send POST request to login endpoint and store tokens', () => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'Password123!'
      };

      const mockResponse = {
        accessToken: mockAccessToken,
        refreshToken: mockRefreshToken,
        expiresIn: 900
      };

      spyOn(tokenStorage, 'saveTokens');

      service.login(credentials).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(tokenStorage.saveTokens).toHaveBeenCalledWith(mockAccessToken, mockRefreshToken);
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(credentials);
      req.flush(mockResponse);
    });

    it('should update authentication state on successful login', (done) => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'Password123!'
      };

      const mockResponse = {
        accessToken: mockAccessToken,
        refreshToken: mockRefreshToken,
        expiresIn: 900
      };

      service.login(credentials).subscribe(() => {
        service.isAuthenticated$.subscribe(isAuth => {
          expect(isAuth).toBe(true);
          done();
        });
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      req.flush(mockResponse);
    });
  });

  describe('logout', () => {
    it('should send POST request to logout endpoint with refresh token', () => {
      spyOn(tokenStorage, 'getRefreshToken').and.returnValue(mockRefreshToken);
      spyOn(tokenStorage, 'clearTokens');

      service.logout().subscribe();

      const req = httpMock.expectOne(`${apiUrl}/logout`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ refreshToken: mockRefreshToken });
      req.flush(null);

      expect(tokenStorage.clearTokens).toHaveBeenCalled();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    it('should clear tokens even if logout request fails', () => {
      spyOn(tokenStorage, 'getRefreshToken').and.returnValue(mockRefreshToken);
      spyOn(tokenStorage, 'clearTokens');

      service.logout().subscribe({
        error: () => {
          expect(tokenStorage.clearTokens).toHaveBeenCalled();
          expect(router.navigate).toHaveBeenCalledWith(['/login']);
        }
      });

      const req = httpMock.expectOne(`${apiUrl}/logout`);
      req.error(new ProgressEvent('error'));
    });
  });

  describe('refreshToken', () => {
    it('should send POST request to refresh endpoint and update tokens', () => {
      const mockResponse = {
        accessToken: mockAccessToken,
        refreshToken: mockRefreshToken,
        expiresIn: 900
      };

      spyOn(tokenStorage, 'saveTokens');

      service.refreshToken(mockRefreshToken).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(tokenStorage.saveTokens).toHaveBeenCalledWith(mockAccessToken, mockRefreshToken);
      });

      const req = httpMock.expectOne(`${apiUrl}/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ refreshToken: mockRefreshToken });
      req.flush(mockResponse);
    });
  });

  describe('forgotPassword', () => {
    it('should send POST request to forgot-password endpoint', () => {
      const email = 'test@example.com';

      service.forgotPassword(email).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/forgot-password`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email });
      req.flush(null);
    });
  });

  describe('resetPassword', () => {
    it('should send POST request to reset-password endpoint', () => {
      const token = 'reset-token';
      const newPassword = 'NewPassword123!';

      service.resetPassword(token, newPassword).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/reset-password`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ token, newPassword });
      req.flush(null);
    });
  });

  describe('changePassword', () => {
    it('should send POST request to change-password endpoint', () => {
      const currentPassword = 'OldPassword123!';
      const newPassword = 'NewPassword123!';

      service.changePassword(currentPassword, newPassword).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/change-password`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ currentPassword, newPassword });
      req.flush(null);
    });
  });

  describe('getProfile', () => {
    it('should send GET request to profile endpoint', () => {
      const mockUser = {
        id: '123',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: UserRole.User,
        isActive: true,
        createdAt: '2024-01-01',
        updatedAt: '2024-01-01'
      };

      service.getProfile().subscribe(user => {
        expect(user).toEqual(mockUser);
      });

      const req = httpMock.expectOne(`${apiUrl}/profile`);
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when valid token exists', () => {
      spyOn(tokenStorage, 'getAccessToken').and.returnValue(mockAccessToken);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when no token exists', () => {
      spyOn(tokenStorage, 'getAccessToken').and.returnValue(null);
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should return false when token is invalid', () => {
      spyOn(tokenStorage, 'getAccessToken').and.returnValue('invalid-token');
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('getCurrentUser', () => {
    it('should return current user from state', () => {
      const user = service.getCurrentUser();
      expect(user).toBeDefined();
    });
  });

  describe('getUserRole', () => {
    it('should return user role when authenticated', () => {
      const credentials: LoginRequest = {
        email: 'test@example.com',
        password: 'Password123!'
      };

      const mockResponse = {
        accessToken: mockAccessToken,
        refreshToken: mockRefreshToken,
        expiresIn: 900
      };

      service.login(credentials).subscribe(() => {
        const role = service.getUserRole();
        expect(role).toBe(UserRole.User);
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      req.flush(mockResponse);
    });

    it('should return null when not authenticated', () => {
      const role = service.getUserRole();
      expect(role).toBeNull();
    });
  });

  describe('clearAuthState', () => {
    it('should clear tokens and reset state', () => {
      spyOn(tokenStorage, 'clearTokens');

      service.clearAuthState();

      expect(tokenStorage.clearTokens).toHaveBeenCalled();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});
