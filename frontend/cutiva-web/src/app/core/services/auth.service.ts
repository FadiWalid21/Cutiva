import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, of, tap } from 'rxjs';
import { AuthApi, AuthMeResponse, AuthTokenResponse } from '../api/auth.api';
import { WorkshopContextService } from './workshop-context.service';

const TOKEN_KEY = 'cutiva.accessToken';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authApi = inject(AuthApi);
  private readonly router = inject(Router);
  private readonly workshops = inject(WorkshopContextService);

  private readonly tokenSignal = signal<string | null>(this.readToken());
  private readonly userSignal = signal<AuthMeResponse | null>(null);

  readonly accessToken = this.tokenSignal.asReadonly();
  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.tokenSignal());
  readonly isAdmin = computed(
    () => this.userSignal()?.roles?.includes('Admin') ?? false,
  );
  readonly isWorkshopOwner = computed(
    () => this.userSignal()?.roles?.includes('WorkshopOwner') ?? false,
  );
  readonly isWorkshopMember = computed(
    () => this.userSignal()?.roles?.includes('WorkshopMember') ?? false,
  );
  /** Resources + product recipes: Owner or platform Admin. */
  readonly canManageCatalog = computed(
    () => this.isAdmin() || this.isWorkshopOwner(),
  );
  readonly homeUrl = computed(() => (this.isAdmin() ? '/admin' : '/dashboard'));

  getToken(): string | null {
    return this.tokenSignal();
  }

  login(email: string, password: string): Observable<AuthTokenResponse> {
    return this.authApi.login({ email, password }).pipe(
      tap((result) => this.persistSession(result)),
    );
  }

  loadMe(): Observable<AuthMeResponse | null> {
    if (!this.tokenSignal()) {
      this.userSignal.set(null);
      return of(null);
    }

    return this.authApi.me().pipe(
      tap((me) => this.userSignal.set(me)),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
    );
  }

  logout(navigate = true): void {
    this.clearSession();
    if (navigate) {
      void this.router.navigateByUrl('/');
    }
  }

  handleUnauthorized(): void {
    this.clearSession();
    void this.router.navigateByUrl('/login');
  }

  private persistSession(result: AuthTokenResponse): void {
    localStorage.setItem(TOKEN_KEY, result.accessToken);
    this.tokenSignal.set(result.accessToken);
    this.userSignal.set({
      userId: '',
      email: result.email,
      displayName: result.displayName,
      roles: result.roles ?? [],
      workshopId: result.workshopId ?? null,
      subscription: result.subscription ?? null,
      features: result.features ?? [],
    });
    if (!(result.roles ?? []).includes('Admin')) {
      this.workshops.load();
    }
  }

  private clearSession(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.tokenSignal.set(null);
    this.userSignal.set(null);
  }

  private readToken(): string | null {
    try {
      return localStorage.getItem(TOKEN_KEY);
    } catch {
      return null;
    }
  }
}
