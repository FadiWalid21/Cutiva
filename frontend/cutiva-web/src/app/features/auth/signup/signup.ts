import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

/** Public signup is disabled; kept only to avoid stale deep links. */
@Component({
  selector: 'app-signup',
  template: '',
})
export class Signup implements OnInit {
  private readonly router = inject(Router);

  ngOnInit(): void {
    void this.router.navigateByUrl('/login');
  }
}
