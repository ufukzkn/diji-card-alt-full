import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClientModule, HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-search',
  standalone: true,
  templateUrl: './search.html',
  styleUrls: ['./search.scss'],
  imports: [CommonModule, FormsModule, RouterModule, HttpClientModule]
})
export class Search implements OnInit {
  searchQuery = '';
  users: any[] = [];
  showAllResults = false;
  isLoading = false;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.http.get<any[]>('http://localhost:5078/api/user')
      .subscribe({
        next: (data) => {
          this.users = data;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Kullanıcılar yüklenirken hata:', error);
          this.isLoading = false;
        }
      });
  }

  get filteredUsers(): any[] {
    const query = this.searchQuery.trim().toLowerCase();
    if (query.length < 3) return [];

    const results = this.users.filter(user =>
      user.fullName.toLowerCase().includes(query) ||
      user.jobTitle?.toLowerCase().includes(query) ||
      user.company?.toLowerCase().includes(query) ||
      user.email?.toLowerCase().includes(query)
    );

    return this.showAllResults ? results : results.slice(0, 3);
  }

  triggerSearch(): void {
    this.searchQuery = this.searchQuery.trim();
    if (this.searchQuery.length >= 3) {
      this.showAllResults = false; // Reset to show limited results first
    }
  }

  showMoreResults(): void {
    this.showAllResults = true;
  }
}
