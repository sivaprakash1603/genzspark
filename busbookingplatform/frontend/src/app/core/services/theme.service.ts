import { Injectable } from '@angular/core';

type ThemeMode = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'bus_booking_theme';

  constructor() {
    this.apply(this.getTheme());
  }

  getTheme(): ThemeMode {
    const stored = localStorage.getItem(this.storageKey) as ThemeMode | null;
    if (stored === 'light' || stored === 'dark') {
      return stored;
    }

    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  toggle(): ThemeMode {
    const next = this.getTheme() === 'dark' ? 'light' : 'dark';
    this.apply(next);
    return next;
  }

  apply(theme: ThemeMode): void {
    localStorage.setItem(this.storageKey, theme);
    document.documentElement.setAttribute('data-theme', theme);
    document.body.setAttribute('data-theme', theme);
  }
}
