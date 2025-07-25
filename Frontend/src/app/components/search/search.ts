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

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.http.get<any[]>('https://localhost:7220/api/user') // 
      .subscribe(data => {
        this.users = data;
      });
  }



  get filteredUsers(): any[] {
    const query = this.searchQuery.trim().toLowerCase();
    if (query.length < 3) return [];

    const results = this.users.filter(user =>
      user.fullName.toLowerCase().includes(query)
    );

    return this.showAllResults ? results : results.slice(0, 3);
  }

  triggerSearch(): void {
    this.searchQuery = this.searchQuery.trim();
    if (this.searchQuery.length >= 3) {
      this.showAllResults = true;
    }
  }

  showAllResults = false;


}
