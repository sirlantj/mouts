import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

// O unwrap interceptor (core/http/unwrap.interceptor.ts) normaliza o envelope
// duplo do backend para um nivel unico, entao aqui usamos a forma simples.
export interface AuthResponse {
  success: boolean;
  message: string;
  data: {
    token: string;
    email: string;
    name: string;
    role: string;
  };
}

interface AuthUser {
  token: string;
  email: string;
  name: string;
  role: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'auth_user';
  private currentUser = signal<AuthUser | null>(this.loadFromStorage());

  readonly isAuthenticated = computed(() => !!this.currentUser());
  readonly user = computed(() => this.currentUser());
  readonly token = computed(() => this.currentUser()?.token ?? null);

  constructor(private http: HttpClient, private router: Router) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth`, { email, password }).pipe(
      tap(response => {
        if (response.success && response.data?.token) {
          const user: AuthUser = {
            token: response.data.token,
            email: response.data.email,
            name: response.data.name,
            role: response.data.role
          };
          localStorage.setItem(this.storageKey, JSON.stringify(user));
          this.currentUser.set(user);
        }
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  private loadFromStorage(): AuthUser | null {
    const stored = localStorage.getItem(this.storageKey);
    if (!stored) return null;
    try {
      return JSON.parse(stored);
    } catch {
      return null;
    }
  }
}
