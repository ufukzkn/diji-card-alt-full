import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

const STORAGE_KEY = 'app_lang';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  constructor(private transloco: TranslocoService) {
    const saved = localStorage.getItem(STORAGE_KEY);
    if (saved) this.setLang(saved);
  }

  setLang(lang: string) {
    this.transloco.setActiveLang(lang);
    localStorage.setItem(STORAGE_KEY, lang);
  }

  get active() {
    return this.transloco.getActiveLang();
  }
}
