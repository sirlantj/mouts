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
    // The unwrap interceptor runs after auth on the response side and flattens the
    // legacy double-nested ApiResponse envelope when it appears.
    provideHttpClient(withInterceptors([authInterceptor, unwrapInterceptor]))
  ]
};
