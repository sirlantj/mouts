import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';

/**
 * The backend wraps responses in ApiResponseWithData<T>. Historically several
 * endpoints wrapped the payload twice — the real data lived in `body.data.data`.
 *
 * This interceptor detects the {success, message, data: {success, message, data: T}}
 * shape and flattens it to {success, message, data: T}, so Angular components can
 * always work against the same predictable structure. Once the backend no longer
 * double-wraps the envelope (current state) this is a no-op safety net.
 */
export const unwrapInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map(event => {
      if (!(event instanceof HttpResponse)) return event;

      const body = event.body as any;
      if (!isApiEnvelope(body)) return event;

      // Double-nested case: body.data is itself an envelope.
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
