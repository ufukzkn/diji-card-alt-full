import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private renderer: Renderer2;
  private dark = false;
  private storageKey = 'app-theme';

  constructor(rendererFactory: RendererFactory2) {
    this.renderer = rendererFactory.createRenderer(null, null);
    const saved = localStorage.getItem(this.storageKey);
    if (saved === 'dark') {
      this.applyDark(true);
    } else if (saved === 'light') {
      this.applyDark(false);
    } else {
      // No preference saved: respect system
      const prefers = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.applyDark(prefers);
      // Listen for changes (optional live sync)
      if (window.matchMedia) {
        const mq = window.matchMedia('(prefers-color-scheme: dark)');
        mq.addEventListener?.('change', e => {
          const currentSaved = localStorage.getItem(this.storageKey);
          if (!currentSaved) { // only auto-switch if user hasn't manually chosen
            this.applyDark(e.matches);
          }
        });
      }
    }
  }

  isDark() { return this.dark; }

  toggle() {
    this.applyDark(!this.dark);
  }

  private applyDark(enable: boolean) {
    this.dark = enable;
    const body = document.body;
    if (enable) {
      this.renderer.addClass(body, 'dark-theme');
      localStorage.setItem(this.storageKey, 'dark');
    } else {
      this.renderer.removeClass(body, 'dark-theme');
      localStorage.setItem(this.storageKey, 'light');
    }
  }
}
