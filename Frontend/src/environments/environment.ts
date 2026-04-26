export const environment = {
  production: false,
  apiUrl: 'https://localhost:7292',
  apiTimeout: 30000,
  tokenStorageKeys: {
    accessToken: 'access_token',
    refreshToken: 'refresh_token'
  },
  notificationDuration: {
    success: 3000,
    error: 5000,
    warning: 4000,
    info: 3000
  }
};
