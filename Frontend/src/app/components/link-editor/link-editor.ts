// src/app/components/link-editor/link-editor.ts

import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { DefinitionsService } from '../../services/definitions';
import { UserLinksService } from '../../services/user-links';
import { Definition } from '../../models/definition.model';
import { UserDefinitionValue } from '../../models/user-definition-value.model';

@Component({
  selector: 'app-link-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './link-editor.html',
  styleUrls: ['./link-editor.scss']
})
export class LinkEditor implements OnInit {
  @Input() userId!: string;

  definitions: Definition[] = [];
  userLinks: UserDefinitionValue[] = [];

  mode: 'add' | 'delete' | 'edit' = 'add';

  // add-mod için
  selectedDefinitionId = '';
  newDefinitionName = '';
  value = '';

  // delete-mod için
  selectedToDelete?: number;

  // edit-mod için
  selectedToEdit?: number;
  editValue = '';

  /** Default (sabit) tanım ID’leri: bunların yanına Sil butonu çıkmasın */
  private defaultIds = [
    /* Örnek ID’ler: */
    1, // İsim
    2, // Şirket
    3, // E-posta
    4  // Telefon
  ];

  constructor(
    private defSvc: DefinitionsService,
    private linkSvc: UserLinksService
  ) { }

  ngOnInit(): void {
    this.loadDefinitions();
    this.loadUserLinks();
  }

  toggleMode(m: 'add' | 'delete' | 'edit') {
    this.mode = m;
    this.resetForm();
  }

  private loadDefinitions(): void {
    this.defSvc.getAll().subscribe(d => (this.definitions = d));
  }

  private loadUserLinks(): void {
    this.linkSvc.getByUser(this.userId).subscribe(l => {
      // Definition adına göre alfabetik sırala
      this.userLinks = l.sort((a, b) => {
        const nameA = this.definitionName(a.definitionId).toLocaleLowerCase();
        const nameB = this.definitionName(b.definitionId).toLocaleLowerCase();
        return nameA.localeCompare(nameB);
      });
    });
  }

  availableDefinitions(): Definition[] {
    return this.definitions.filter(
      d => !this.userLinks.some(l => l.definitionId === d.definitionId)
    );
  }

  definitionName(id: number): string {
    return this.definitions.find(d => d.definitionId === id)?.definitionName || '';
  }

  /** Sabit olmayan tanımlar için */
  isDefault(defId: number): boolean {
    return this.defaultIds.includes(defId);
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
      this.linkSvc.add({ userId: this.userId, definitionId: defId, value: this.value })
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
