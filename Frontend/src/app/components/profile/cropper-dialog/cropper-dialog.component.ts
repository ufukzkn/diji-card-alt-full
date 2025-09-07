import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { TranslocoModule } from '@jsverse/transloco';

export type CropperDialogData = {
  image: File;
  width: number;
  height: number;
};

export type CropperDialogResult = {
  blob: Blob;
  imageUrl: string;
};

@Component({
  selector: 'app-cropper-dialog',
  standalone: true,
  imports: [CommonModule, ImageCropperComponent, TranslocoModule],
  template: `
    <div class="cropper-modal">
      <div class="cropper-content">
  <h2>{{ 'profile.photo.cropTitle' | transloco : { default: fallbackTitle } }}</h2>
        <div class="cropper-container">
          <image-cropper
            [maintainAspectRatio]="true"
            [aspectRatio]="data.width / data.height"
            [resizeToHeight]="data.height"
            [resizeToWidth]="data.width"
            [onlyScaleDown]="true"
            [imageFile]="data.image"
            (imageCropped)="imageCropped($event)"
          ></image-cropper>
        </div>
        <div class="cropper-actions">
        <button class="photo-btn" (click)="done()" [disabled]="!result()">{{ 'common.buttons.save' | transloco }}</button>  
        <button class="photo-btn delete" (click)="cancel()">{{ 'common.buttons.cancel' | transloco }}</button>
          

        </div>
      </div>
    </div>
  `,
  styles: [`
    .cropper-modal { position:fixed; inset:0; background: rgba(0,0,0,.55); display:flex; align-items:center; justify-content:center; z-index:1000; backdrop-filter: blur(4px); }
  .cropper-content { background: var(--color-surface,#fff); color: var(--color-text,#1e293b); padding:1.75rem 2rem 1.5rem; border-radius:18px; box-shadow:0 18px 50px -12px rgba(0,0,0,.28); width: min(640px,92vw); max-height:90vh; overflow:auto; display:flex; flex-direction:column; gap:1rem; border:1px solid var(--color-border,#e2e8f0); animation: cropIn .28s cubic-bezier(.4,0,.2,1); }
    h2 { margin:0; font-size:1.25rem; font-weight:600; letter-spacing:.5px; }
  .cropper-container { border-radius:14px; overflow:hidden; background: linear-gradient(145deg, var(--color-surface-alt,#f8fafc), var(--color-surface,#ffffff)); padding:.85rem; box-shadow: inset 0 0 0 1px var(--color-border,#e2e8f0), 0 4px 10px -6px rgba(0,0,0,.12); }
  image-cropper { max-width:100%; --cropper-outline-color: var(--color-border,#cbd5e1); }
    .cropper-actions { display:flex; gap:.75rem; justify-content:flex-end; flex-wrap:wrap; margin-top:.25rem; }
  .photo-btn { background: linear-gradient(135deg, var(--color-surface-alt,#f1f5f9) 0%, var(--color-surface,#ffffff) 100%); color: var(--color-text,#1e293b); border:1px solid var(--color-border,#d1d5db); border-radius:10px; padding:.65rem 1.25rem; font-size:.85rem; font-weight:600; cursor:pointer; display:inline-flex; align-items:center; gap:.45rem; letter-spacing:.3px; box-shadow:0 2px 6px -2px rgba(0,0,0,.12); transition: background .25s, transform .25s, box-shadow .25s, border-color .25s; }
  .photo-btn:hover:not(:disabled){ background: linear-gradient(135deg, var(--color-surface,#ffffff) 0%, var(--color-surface-alt,#f1f5f9) 100%); transform:translateY(-2px); box-shadow:0 6px 16px -4px rgba(0,0,0,.2); }
  .photo-btn.delete { background: var(--color-danger,#ef4444); color:#fff; border:1px solid rgba(0,0,0,.05); }
  .photo-btn.delete:hover:not(:disabled){ filter:brightness(1.07); }
  .photo-btn.primary { background: linear-gradient(135deg, var(--color-border,#d1d5db) 0%, var(--color-text-muted,#64748b) 100%); color:#fff; border:1px solid var(--color-border,#d1d5db); }
  .photo-btn.primary:hover:not(:disabled){ filter:brightness(1.06); }
    .photo-btn:disabled { opacity:.55; cursor:not-allowed; transform:none; }
    .dark-theme .cropper-content { background: var(--color-surface,#1e293b); border-color: var(--color-border,#334155); box-shadow:0 24px 60px -18px rgba(0,0,0,.7); }
    .dark-theme .cropper-container { background: rgba(255,255,255,.03); box-shadow: inset 0 0 0 1px rgba(255,255,255,.06); }
  .dark-theme .photo-btn { background: linear-gradient(135deg, rgba(255,255,255,.05) 0%, rgba(255,255,255,.02) 100%); color: var(--color-text,#f1f5f9); border:1px solid rgba(255,255,255,.1); box-shadow:0 6px 18px -8px rgba(0,0,0,.7); }
  .dark-theme .photo-btn:hover:not(:disabled){ background: linear-gradient(135deg, rgba(255,255,255,.1) 0%, rgba(255,255,255,.04) 100%); }
  .dark-theme .photo-btn.primary { background: linear-gradient(135deg, rgba(148,163,184,.28) 0%, rgba(100,116,139,.22) 100%); border-color: rgba(148,163,184,.35); }
  .dark-theme .photo-btn.primary:hover:not(:disabled){ filter:brightness(1.08); }
    @keyframes cropIn { from { opacity:0; transform:translateY(12px) scale(.95);} to { opacity:1; transform:translateY(0) scale(1);} }
  `],
})
export class CropperDialogComponent {
  @Input() data!: CropperDialogData;
  @Input() onResult!: (result: CropperDialogResult | undefined) => void;
  fallbackTitle = 'Crop Image';

  result = signal<CropperDialogResult | undefined>(undefined);

  imageCropped(event: ImageCroppedEvent) {
    const { blob, objectUrl } = event;
    if (blob && objectUrl) {
      this.result.set({ blob, imageUrl: objectUrl });
    }
  }

  done() {
    if (this.result()) {
      this.onResult(this.result());
    }
  }

  cancel() {
    this.onResult(undefined);
  }
} 
// CropperDialogComponent bir tutorial'dan direkt olarak aldığım için diğer bileşenlerdeki gibi parçalara ayırmadım.