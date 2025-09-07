import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslocoModule } from '@jsverse/transloco';
import { LanguageService } from '../../../services/language.service';

@Component({
  selector: 'app-language-switcher',
  standalone: true,
  imports: [CommonModule, TranslocoModule],
  template: `
  <div class="lang-switcher" [class.open]="open">
    <button type="button" class="lang-current" (click)="toggle()" [attr.aria-expanded]="open" aria-label="Language switcher">
      <i class="fas fa-globe globe"></i>
      <span class="code">{{ selected.toUpperCase() }}</span>
      <i class="fas fa-chevron-down chevron" [class.rot]="open"></i>
    </button>
    <ul class="lang-menu" *ngIf="open">
      <li *ngFor="let l of langs" (click)="change(l)" [class.active]="l===selected" [attr.data-lang]="l">
        <i class="fas fa-globe-asia globe-small" *ngIf="l==='tr'"></i>
        <i class="fas fa-globe-americas globe-small" *ngIf="l==='en'"></i>
        <i class="fas fa-globe-europe globe-small" *ngIf="l==='de'"></i>
        <span class="label">{{ labelMap[l] }}</span>
      </li>
    </ul>
  </div>
  `,
  styleUrls: ['./language-switcher.component.scss']
})
export class LanguageSwitcherComponent {
  @Input() compact = false;
  open = false;
  langs = ['tr','en','de'];
  // Keep label map; flags replaced by globe variants
  labelMap: Record<string,string> = { tr:'Türkçe', en:'English', de:'Deutsch' };
  selected: string;

  constructor(private lang: LanguageService) {
    this.selected = this.lang.active || 'tr';
  }

  toggle(){ this.open = !this.open; }
  change(l:string){
    if (l===this.selected) { this.open=false; return; }
    this.lang.setLang(l);
    this.selected = l;
    this.open = false;
  }
}
