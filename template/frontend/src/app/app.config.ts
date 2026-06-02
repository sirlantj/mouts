import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { unwrapInterceptor } from './core/http/unwrap.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // unwrap roda apos auth (ordem invertida na resposta) e normaliza o envelope duplo
    provideHttpClient(withInterceptors([authInterceptor, unwrapInterceptor]))
  ]
};
