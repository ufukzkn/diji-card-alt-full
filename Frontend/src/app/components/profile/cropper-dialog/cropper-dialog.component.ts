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
        <h2>{{ 'profile.photo.cropTitle' | transloco }}</h2>
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
    .cropper-modal {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }
    .cropper-content {
      background: #fff;
      padding: 2rem;
      border-radius: 12px;
      box-shadow: 0 4px 32px rgba(0,0,0,0.15);
      display: flex;
      flex-direction: column;
      align-items: center;
      max-width: 90vw;
      max-height: 90vh;
      overflow: auto;
    }
    .cropper-container {
      margin: 1rem 0;
    }
    .cropper-actions {
      display: flex;
      gap: 1rem;
      margin-top: 1rem;
    }
    .photo-btn {
      background: #1976d2;
      color: #fff;
      border: none;
      border-radius: 6px;
      padding: 0.5rem 1.2rem;
      font-size: 1rem;
      cursor: pointer;
      transition: background 0.2s;
    }
    .photo-btn:hover:not(:disabled) {
      background: #125ea2;
    }
    .photo-btn.delete {
      background: #e53935;
    }
    .photo-btn.delete:hover:not(:disabled) {
      background: #b71c1c;
    }
    .photo-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  `],
})
export class CropperDialogComponent {
  @Input() data!: CropperDialogData;
  @Input() onResult!: (result: CropperDialogResult | undefined) => void;

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