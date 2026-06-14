import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { DatePipe, DecimalPipe } from '@angular/common';
import { environment } from '../../../../environments/environment';
import { DashboardStats } from '../../../core/models/models';

@Component({
  selector: 'app-admin-dashboard',
  imports: [RouterLink, DatePipe, DecimalPipe],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  private http = inject(HttpClient);
  stats = signal<DashboardStats | null>(null);

  ngOnInit() {
    this.http.get<DashboardStats>(`${environment.apiUrl}/admin/dashboard`).subscribe(s => this.stats.set(s));
  }
}
