export const PlanFeatureCodes = {
  resources: 'resources',
  productTemplates: 'productTemplates',
  remnants: 'remnants',
  planComparisons: 'planComparisons',
  clientQuotes: 'clientQuotes',
  workshopReports: 'workshopReports',
} as const;

export type PlanFeatureCode =
  (typeof PlanFeatureCodes)[keyof typeof PlanFeatureCodes];

export const PLAN_FEATURE_CATALOG: {
  code: PlanFeatureCode;
  labelKey: string;
}[] = [
  { code: PlanFeatureCodes.resources, labelKey: 'admin.feature.resources' },
  {
    code: PlanFeatureCodes.productTemplates,
    labelKey: 'admin.feature.productTemplates',
  },
  { code: PlanFeatureCodes.remnants, labelKey: 'admin.feature.remnants' },
  {
    code: PlanFeatureCodes.planComparisons,
    labelKey: 'admin.feature.planComparisons',
  },
  {
    code: PlanFeatureCodes.clientQuotes,
    labelKey: 'admin.feature.clientQuotes',
  },
  {
    code: PlanFeatureCodes.workshopReports,
    labelKey: 'admin.feature.workshopReports',
  },
];
