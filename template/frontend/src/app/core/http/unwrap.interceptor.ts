import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';

/**
 * O backend envelopa as respostas em ApiResponseWithData<T>, e em vários endpoints
 * acaba envelopando duas vezes — o payload real fica em `body.data.data`.
 *
 * Este interceptor detecta o padrão {success, message, data: {success, message, data: T}}
 * e nivela para {success, message, data: T}, deixando os componentes Angular
 * trabalharem com uma estrutura previsível.
 */
export const unwrapInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (!(event instanceof HttpResponse)) return event;

      const body = event.body as any;
      if (!isApiEnvelope(body)) return event;

      // Caso duplo-aninhado: body.data eh outro envelope
      if (isApiEnvelope(body.data)) {
        const inner = body.data as ApiEnvelope;
        const unwrapped = {
          success: body.success,
          message: body.message || inner.message,
          data: inner.data,
          errors: body.errors ?? inner.errors
        };
        return event.clone({ body: unwrapped });
      }

      return event;
    })
  );
};

interface ApiEnvelope {
  success: boolean;
  message?: string;
  data: unknown;
  errors?: unknown[];
}

function isApiEnvelope(value: unknown): value is ApiEnvelope {
  return (
    typeof value === 'object' &&
    value !== null &&
    'success' in value &&
    'data' in value
  );
}
