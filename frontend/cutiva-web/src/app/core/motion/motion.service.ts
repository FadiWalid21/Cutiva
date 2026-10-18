import { Injectable } from '@angular/core';
import gsap from 'gsap';

@Injectable({ providedIn: 'root' })
export class MotionService {
  prefersReducedMotion(): boolean {
    if (typeof window === 'undefined' || !window.matchMedia) {
      return false;
    }
    return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  }

  pageEnter(root: HTMLElement | null): gsap.core.Tween | null {
    if (!root || this.prefersReducedMotion()) {
      if (root) {
        gsap.set(root, { clearProps: 'all', opacity: 1, y: 0 });
      }
      return null;
    }

    return gsap.fromTo(
      root,
      { opacity: 0, y: 18 },
      { opacity: 1, y: 0, duration: 0.55, ease: 'power2.out' },
    );
  }

  staggerIn(elements: HTMLElement[] | NodeListOf<Element>): gsap.core.Tween | null {
    if (!elements.length || this.prefersReducedMotion()) {
      gsap.set(elements, { clearProps: 'all', opacity: 1, y: 0 });
      return null;
    }

    return gsap.fromTo(
      elements,
      { opacity: 0, y: 14 },
      {
        opacity: 1,
        y: 0,
        duration: 0.45,
        stagger: 0.07,
        ease: 'power2.out',
        clearProps: 'transform',
      },
    );
  }

  authPanelEnter(panel: HTMLElement | null, brand: HTMLElement | null): void {
    if (this.prefersReducedMotion()) {
      if (panel) gsap.set(panel, { opacity: 1, y: 0 });
      if (brand) gsap.set(brand, { opacity: 1, y: 0 });
      return;
    }

    const tl = gsap.timeline({ defaults: { ease: 'power2.out' } });
    if (brand) {
      tl.fromTo(brand, { opacity: 0, y: 24 }, { opacity: 1, y: 0, duration: 0.7 });
    }
    if (panel) {
      tl.fromTo(
        panel,
        { opacity: 0, y: 20 },
        { opacity: 1, y: 0, duration: 0.55 },
        brand ? '-=0.35' : 0,
      );
    }
  }
}
