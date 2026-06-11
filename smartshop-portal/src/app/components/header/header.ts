import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  imports: [AsyncPipe],
  templateUrl: './header.html',
  styleUrl: './header.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user$ = this.authService.user$;

  protected logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
