// src/app/components/link-editor/link-editor.ts

import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { firstValueFrom } from 'rxjs';

import { DefinitionsService } from '../../services/definitions';
import { UserLinksService } from '../../services/user-links';
import { Definition } from '../../models/definition.model';
import { UserDefinitionValue } from '../../models/user-definition-value.model';

@Component({
  selector: 'app-link-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, DragDropModule],
  templateUrl: './link-editor.html',
  styleUrls: ['./link-editor.scss']
})
export class LinkEditor implements OnInit {
  @Input() userId!: string;

  definitions: Definition[] = [];
  userLinks: UserDefinitionValue[] = [];
  userLinksArray: UserDefinitionValue[] = [];

  mode: 'add' | 'delete' | 'edit' | 'sort' | '' = '';
  lastOpenedDropdown: 'add' | 'delete' | 'edit' | 'sort' | '' = '';

  // add-mod için
  selectedDefinitionId = '';
  newDefinitionName = '';
  value = '';

  // delete-mod için
  selectedToDelete?: number;

  // edit-mod için
  selectedToEdit?: number;
  editValue = '';

  constructor(
    private defSvc: DefinitionsService,
    private linkSvc: UserLinksService
  ) {}

  ngOnInit(): void {
    this.mode = '';
    this.lastOpenedDropdown = '';
    this.loadDefinitions();
    this.loadUserLinks();
  }

  toggleMode(m: 'add' | 'delete' | 'edit' | 'sort') {
    if (this.mode === m) {
      this.mode = '';
      this.lastOpenedDropdown = '';
      this.resetForm();
    } else {
      this.mode = m;
      this.lastOpenedDropdown = m;
      this.resetForm();
      if (m === 'sort') {
        // Sıralama moduna girerken array'i güncelle
        this.userLinksArray = [...this.userLinks].sort((a, b) => a.sortId - b.sortId);
      }
    }
  }

  availableDefinitions(): Definition[] {
    return this.definitions.filter(
      d => !this.userLinks.some(l => l.definitionId === d.definitionId)
    );
  }

  definitionName(id: number): string {
    return this.definitions.find(d => d.definitionId === id)?.definitionName || '';
  }

  private loadDefinitions(): void {
    this.defSvc.getAll().subscribe(d => (this.definitions = d));
  }

  private loadUserLinks(): void {
    this.linkSvc.getByUser(this.userId).subscribe(l => {
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
    await firstValueFrom(this.linkSvc.updateSortOrder(this.userLinksArray));
    this.loadUserLinks();
  }

  async add(): Promise<void> {
    if (!this.value) return;

    let defId: number;

    if (this.selectedDefinitionId === '__new') {
      if (!this.newDefinitionName.trim()) {
        alert('İsim gir');
        return;
      }
      const created = await firstValueFrom(
        this.defSvc.add(this.newDefinitionName.trim())
      );
      this.definitions.push(created);
      defId = created.definitionId;
    } else {
      defId = +this.selectedDefinitionId;
    }

    if (this.userLinks.some(l => l.definitionId === defId)) {
      alert('Bu tanım zaten eklenmiş!');
      return;
    }

    await firstValueFrom(
      this.linkSvc.add({ userId: this.userId, definitionId: defId, value: this.value, sortId: this.userLinks.length })
    );

    this.loadUserLinks();
    this.resetForm();
  }

  /** Silme işlemi */
  async delete(defId: number): Promise<void> {
    if (!confirm('Bu bağlantıyı silmek istediğine emin misin?')) return;
    await firstValueFrom(this.linkSvc.delete(this.userId, defId));
    this.loadUserLinks();
  }

  /** Edit işlemi */
  async edit(): Promise<void> {
    if (!this.selectedToEdit || !this.editValue) return;
    await firstValueFrom(
      this.linkSvc.update(this.userId, this.selectedToEdit, this.editValue)
    );
    this.loadUserLinks();
    this.resetForm();
  }

  private resetForm() {
    this.selectedDefinitionId = '';
    this.newDefinitionName = '';
    this.value = '';
    this.selectedToDelete = undefined;
    this.selectedToEdit = undefined;
    this.editValue = '';
  }
}
