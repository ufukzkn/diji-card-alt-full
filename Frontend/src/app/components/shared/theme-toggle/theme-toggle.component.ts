import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../../services/theme.service';

@Component({
  selector: 'app-theme-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
  <button type="button" class="theme-toggle" (click)="toggle()" [attr.aria-label]="dark ? 'Light mode' : 'Dark mode'">
    <i class="fas" [ngClass]="dark ? 'fa-sun' : 'fa-moon'"></i>
  </button>
  `,
  styleUrls: ['./theme-toggle.component.scss']
})
export class ThemeToggleComponent {
  dark = false;
  constructor(private theme: ThemeService) { this.dark = this.theme.isDark(); }
  toggle(){ this.theme.toggle(); this.dark = this.theme.isDark(); }
}
