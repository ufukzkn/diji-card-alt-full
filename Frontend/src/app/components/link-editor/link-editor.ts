// src/app/components/link-editor/link-editor.ts

import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { firstValueFrom } from 'rxjs';

import { DefinitionsService } from '../../services/definitions';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { UserLinksService, AddUserLinkRequest, UpdateUserLinkRequest } from '../../services/user-links';
import { Definition } from '../../models/definition.model';
import { NotificationService } from '../../services/notification.service';
import { UserDefinitionValue } from '../../models/user-definition-value.model';

@Component({
  selector: 'app-link-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule, TranslocoModule],
  templateUrl: './link-editor.html',
  styleUrls: ['./link-editor.scss']
})
export class LinkEditor implements OnInit {
  @Input() userId!: string;
  @Input() canEdit: boolean = false;
  @Output() linksChanged = new EventEmitter<void>();
  @Output() modeChanged = new EventEmitter<string>();

  definitions: Definition[] = [];
  userLinks: UserDefinitionValue[] = [];
  userLinksArray: UserDefinitionValue[] = [];

  mode: 'add' | 'delete' | 'edit' | 'sort' | '' = '';
  lastOpenedDropdown: 'add' | 'delete' | 'edit' | 'sort' | '' = '';

  // add-mod için
  selectedDefinitionId = '';
  selectedToAdd?: number;
  newDefinitionName = '';
  value = '';

  // delete-mod için ('' => placeholder state)
  selectedToDelete: number | string | '' = '';
  deleteDefinitionName = '';

  // edit-mod için ('' => placeholder state)
  selectedToEdit: number | string | '' = '';
  editValue = '';

  // custom-mod için
  customDefinitionName = '';

  constructor(
  private defSvc: DefinitionsService,
  private linkSvc: UserLinksService,
  private transloco: TranslocoService,
  private notify: NotificationService
  ) {}

  ngOnInit(): void {
    console.log('Link-editor ngOnInit, userId:', this.userId);
    this.mode = '';
    this.lastOpenedDropdown = '';
    this.loadDefinitions();
    this.loadUserLinks();
  }

  toggleMode(m: 'add' | 'delete' | 'edit' | 'sort') {
    console.log('toggleMode çağrıldı:', m, 'mevcut mode:', this.mode);
    if (this.mode === m) {
      this.mode = '';
      this.lastOpenedDropdown = '';
      this.resetForm();
  this.modeChanged.emit(this.mode);
    } else {
      this.mode = m;
      this.lastOpenedDropdown = m;
      this.resetForm();
  if (m === 'delete') { this.selectedToDelete = ''; }
  if (m === 'edit') { this.selectedToEdit = ''; }
      if (m === 'sort') {
        // Sıralama moduna girerken array'i güncelle
        this.userLinksArray = [...this.userLinks].sort((a, b) => a.sortId - b.sortId);
      }
  this.modeChanged.emit(this.mode);
    }
    console.log('Yeni mode:', this.mode);
  }

  availableDefinitions(): Definition[] {
    const available = this.definitions.filter(
      d => !this.userLinks.some(l => l.definitionId === d.definitionId)
    );
    console.log('availableDefinitions:', available, 'tüm definitions:', this.definitions);
    return available;
  }

  definitionName(id: number): string {
    // Önce userLinks'te bu ID'yi ara
    const userLink = this.userLinks.find(l => l.definitionId === id);
    if (userLink && userLink.displayName) {
      return userLink.displayName;
    }
    
    // Fallback: definitions tablosundan çek
    return this.definitions.find(d => d.definitionId === id)?.definitionName || '';
  }

  canAddLink(): boolean {
    if (!this.selectedDefinitionId || !this.value.trim()) {
      return false;
    }
    
    if (this.selectedDefinitionId === '__new') {
      return this.newDefinitionName.trim() !== '';
    }
    
    if (this.selectedDefinitionId === '__custom') {
      return this.customDefinitionName.trim() !== '';
    }
    
    return true;
  }

  private loadDefinitions(): void {
    console.log('loadDefinitions çağrıldı');
    this.defSvc.getAll().subscribe(d => {
      console.log('Definitions yüklendi:', d);
      this.definitions = d;
    });
  }

  private loadUserLinks(): void {
    console.log('loadUserLinks çağrıldı, userId:', this.userId);
    this.linkSvc.getByUser(this.userId).subscribe(l => {
      console.log('User links yüklendi:', l);
      this.userLinks = l.sort((a, b) => a.sortId - b.sortId);
    });
  }

  drop(event: CdkDragDrop<UserDefinitionValue[]>) {
    // Dikey sıralama için array mantığı
    const prevIndex = event.previousIndex;
    const currIndex = event.currentIndex;
    const arr = this.userLinksArray;
    const [movedItem] = arr.splice(prevIndex, 1);
    arr.splice(currIndex, 0, movedItem);
    // sortId'leri güncelle (array sırasına göre)
    arr.forEach((link, idx) => link.sortId = idx);
  }

  async saveSortOrder(): Promise<void> {
    try {
      await firstValueFrom(this.linkSvc.updateSortOrder(this.userLinksArray));
      this.loadUserLinks();
      this.linksChanged.emit();
      this.notify.showToast(this.transloco.translate('profile.links.sortSuccess'), 'success');
    } catch(e) {
      this.notify.showToast(this.transloco.translate('common.errors.generic'), 'error');
    }
  }

  async add(): Promise<void> {
    if (!this.value) return;

    let defId: number;

    if (this.selectedDefinitionId === '__new') {
      if (!this.newDefinitionName.trim()) {
        alert(this.transloco.translate('linkEditor.msg.enterName'));
        return;
      }
      const created = await firstValueFrom(
        this.defSvc.add(this.newDefinitionName.trim())
      );
      this.definitions.push(created);
      defId = created.definitionId;
    } else if (this.selectedDefinitionId === '__custom') {
      // Kişisel tanım ekleme
      if (!this.customDefinitionName.trim()) {
        alert(this.transloco.translate('linkEditor.msg.enterCustomName'));
        return;
      }
      
      try {
        const result = await firstValueFrom(
          this.linkSvc.addCustomDefinition({
            userId: this.userId,
            customDefinitionName: this.customDefinitionName.trim(),
            value: this.value,
            sortId: this.userLinks.length
          })
        );
        
        this.loadUserLinks();
        this.resetForm();
        this.linksChanged.emit();
        return;
      } catch (error) {
  console.error('Custom definition ekleme hatası:', error);
  alert(this.transloco.translate('linkEditor.msg.customAddFail'));
        return;
      }
    } else {
      defId = +this.selectedDefinitionId;
    }

    if (this.userLinks.some(l => l.definitionId === defId)) {
  alert(this.transloco.translate('linkEditor.msg.alreadyExists'));
      return;
    }

    try {
      await firstValueFrom(
        this.linkSvc.add({ 
          userId: this.userId, 
          definitionId: defId, 
          value: this.value, 
          sortId: this.userLinks.length 
        } as AddUserLinkRequest)
      );
      this.loadUserLinks();
      this.resetForm();
      this.linksChanged.emit();
      this.notify.showToast(this.transloco.translate('profile.links.addSuccess'), 'success');
    } catch (e) {
      this.notify.showToast(this.transloco.translate('profile.links.addError'), 'error');
    }
  }

  /** Silme işlemi */
  async delete(id: number): Promise<void> {
  const confirmed = await this.notify.showConfirmation('common.dialogs.delete.title','linkEditor.msg.confirmDeleteLink');
  if (!confirmed) return;
    try {
      await firstValueFrom(this.linkSvc.deleteById(id));
      this.loadUserLinks();
      this.linksChanged.emit();
      this.notify.showToast(this.transloco.translate('profile.links.deleteSuccess'), 'success');
    } catch (e) {
      this.notify.showToast(this.transloco.translate('profile.links.deleteError'), 'error');
    }
  }

  /** Edit işlemi */
  async edit(): Promise<void> {
    console.log('Edit fonksiyonu çağrıldı:', this.selectedToEdit, this.editValue);
    console.log('selectedToEdit type:', typeof this.selectedToEdit);
    
  if (!this.selectedToEdit || this.selectedToEdit === '' || !this.editValue) {
      console.log('Edit validasyon hatası - selectedToEdit veya editValue boş');
      return;
    }
    
    // selectedToEdit'i number'a çevir (select'ten string geliyorsa)
  const id = typeof this.selectedToEdit === 'string' ? parseInt(this.selectedToEdit) : this.selectedToEdit;
    console.log('Çevrilen ID:', id);
    
    try {
      console.log('API çağrısı yapılıyor...');
      // Sadece value'yu güncelle - hem custom hem normal için aynı endpoint
      await firstValueFrom(
        this.linkSvc.updateByIdOnly(id, { value: this.editValue })
      );
      
      console.log('API çağrısı başarılı');
  this.loadUserLinks();
  this.resetForm();
  this.linksChanged.emit();
  this.notify.showToast(this.transloco.translate('profile.links.updateSuccess'), 'success');
    } catch (error) {
      console.error('Edit hatası:', error);
  this.notify.showToast(this.transloco.translate('profile.links.updateError'), 'error');
    }
  }

  /** Tanım silme işlemi */
  async deleteDefinition(): Promise<void> {
    if (!this.deleteDefinitionName) return;

    // Silinecek tanımı bul
    const defToDelete = this.definitions.find(
      d => d.definitionName.toLowerCase() === this.deleteDefinitionName.toLowerCase()
    );

    if (!defToDelete) {
      alert(this.transloco.translate('linkEditor.msg.definitionNotFound'));
      return;
    }

  const confirmDef = await this.notify.showConfirmation('common.dialogs.delete.title','linkEditor.msg.confirmDeleteDefinition');
  if (!confirmDef) return;

    try {
      await firstValueFrom(this.defSvc.delete(defToDelete.definitionId));
      // Başarılı silme işleminden sonra listeleri güncelle
      this.loadDefinitions();
      this.loadUserLinks();
      this.resetForm();
      this.linksChanged.emit(); // Parent'a değişiklik bildir
  alert(this.transloco.translate('linkEditor.msg.deleteSuccess'));
    } catch (err) {
  alert(this.transloco.translate('linkEditor.msg.deleteFail'));
      console.error('Tanım silme hatası:', err);
    }
  }

  onDeleteClick() {
    if (typeof this.selectedToDelete === 'number') {
      this.delete(this.selectedToDelete);
    }
  }

  private resetForm() {
    this.selectedDefinitionId = '';
    this.newDefinitionName = '';
    this.value = '';
  this.selectedToDelete = '';
  this.selectedToEdit = '';
    this.editValue = '';
    this.deleteDefinitionName = '';
    this.customDefinitionName = '';
  }

}
