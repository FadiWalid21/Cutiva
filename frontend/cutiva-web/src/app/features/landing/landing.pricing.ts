export type LandingTierId = 'starter' | 'workshop' | 'studio';

export interface LandingTier {
  id: LandingTierId;
  code: string;
  name: string;
  priceSar: number;
  maxUsers: number | null;
  featured: boolean;
}

/** Fallback if public plans API is unavailable. */
export const LANDING_TIERS_FALLBACK: LandingTier[] = [
  { id: 'starter', code: 'Starter', name: 'Starter', priceSar: 99, maxUsers: 1, featured: false },
  {
    id: 'workshop',
    code: 'Workshop',
    name: 'Workshop',
    priceSar: 149,
    maxUsers: 3,
    featured: true,
  },
  { id: 'studio', code: 'Studio', name: 'Studio', priceSar: 249, maxUsers: null, featured: false },
];

const CODE_ORDER: LandingTierId[] = ['starter', 'workshop', 'studio'];

export function mapPublicPlansToTiers(
  plans: { code: string; name: string; priceSar: number; maxUsers: number | null }[],
): LandingTier[] {
  const byCode = new Map(plans.map((p) => [p.code.toLowerCase(), p]));
  return CODE_ORDER.map((id) => {
    const api = byCode.get(id);
    const fallback = LANDING_TIERS_FALLBACK.find((t) => t.id === id)!;
    if (!api) return fallback;
    return {
      id,
      code: api.code,
      name: api.name,
      priceSar: api.priceSar,
      maxUsers: api.maxUsers,
      featured: id === 'workshop',
    };
  });
}

export const LANDING_CONTACT = {
  email: 'fadiwd21@gmail.com',
  phoneDisplay: '+966 56 238 5677',
  phoneTel: '+966 56 238 5677',
} as const;

export type StorySceneId = 'stock' | 'cutPlan' | 'frame' | 'remnant';

export const STORY_SCENES: StorySceneId[] = ['stock', 'cutPlan', 'frame', 'remnant'];
