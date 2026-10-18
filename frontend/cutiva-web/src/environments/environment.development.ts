export const environment = {
  production: false,
  /** Empty: same-origin via Angular proxy (`proxy.conf.json` → localhost:5216). */
  apiBaseUrl: '',
  useDashboardMock: false,
  defaultLanguage: 'en',
  supportedLanguages: ['en', 'ar'] as const,
};
