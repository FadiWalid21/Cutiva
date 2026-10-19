import { PricingDisplay } from '../../core/api/plans.api';

export type LandingTierId = 'starter' | 'workshop' | 'studio';

export interface LandingTier {
  id: LandingTierId;
  code: string;
  name: string;
  price: number;
  currency: string;
  maxUsers: number | null;
  featured: boolean;
}

/** Fallback if public pricing API is unavailable (SAR). */
export const LANDING_TIERS_FALLBACK: LandingTier[] = [
  {
    id: 'starter',
    code: 'Starter',
    name: 'Starter',
    price: 99,
    currency: 'SAR',
    maxUsers: 1,
    featured: false,
  },
  {
    id: 'workshop',
    code: 'Workshop',
    name: 'Workshop',
    price: 149,
    currency: 'SAR',
    maxUsers: 3,
    featured: true,
  },
  {
    id: 'studio',
    code: 'Studio',
    name: 'Studio',
    price: 249,
    currency: 'SAR',
    maxUsers: null,
    featured: false,
  },
];

const CODE_ORDER: LandingTierId[] = ['starter', 'workshop', 'studio'];

export function mapPricingDisplayToTiers(display: PricingDisplay): LandingTier[] {
  const currency = (display.currency || 'SAR').toUpperCase();
  const byCode = new Map(display.plans.map((p) => [p.code.toLowerCase(), p]));
  return CODE_ORDER.map((id) => {
    const api = byCode.get(id);
    const fallback = LANDING_TIERS_FALLBACK.find((t) => t.id === id)!;
    if (!api) {
      return { ...fallback, currency };
    }
    return {
      id,
      code: api.code,
      name: api.name,
      price: Math.trunc(Number(api.price) || 0),
      currency,
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
