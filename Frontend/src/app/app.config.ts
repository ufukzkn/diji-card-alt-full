import { provideHttpClient } from '@angular/common/http';
import { routes } from './app.routes';
import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { importProvidersFrom } from '@angular/core';   
import { BrowserModule } from '@angular/platform-browser';





export const appConfig: ApplicationConfig = {
  providers: [
    //provideBrowserGlobalErrorListeners(),
    //provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(),
    importProvidersFrom(FormsModule),
    importProvidersFrom(BrowserModule),
          
  ]
};
