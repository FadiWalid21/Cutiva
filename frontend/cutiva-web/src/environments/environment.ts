export const environment = {
  production: true,
  apiBaseUrl: 'http://localhost:5216',
  useDashboardMock: false,
  defaultLanguage: 'en',
  supportedLanguages: ['en', 'ar'] as const,
  /** Off in production builds until Phase 6 flag removal is approved. */
  features: {
    resourcesV2: false,
    productTemplates: false,
  },
};
