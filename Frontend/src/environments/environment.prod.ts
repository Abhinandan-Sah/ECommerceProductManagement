export const environment = {
  production: true,
  apiUrl: 'https://api.yourdomain.com',
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
