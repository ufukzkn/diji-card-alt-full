import { provideTransloco, Translation, TranslocoLoader, translocoConfig } from '@jsverse/transloco';
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
  constructor(private http: HttpClient) {}
  getTranslation(lang: string) {
    return this.http.get<Translation>(`/i18n/${lang}.json`);
  }
}

export function provideAppTransloco() {
  return [
    provideTransloco({
      config: translocoConfig({
        availableLangs: [
          { id: 'tr', label: 'Türkçe' },
          { id: 'en', label: 'English' },
          { id: 'de', label: 'Deutsch' }
        ],
        defaultLang: 'tr',
        reRenderOnLangChange: true,
        prodMode: true
      }),
      loader: TranslocoHttpLoader
    })
  ];
}
