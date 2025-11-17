import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-connected',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './connected.component.html',
  styleUrls: ['./connected.component.css']
})
export class ConnectedComponent implements OnInit {
  isAuthenticated: boolean = false;
  
  private apiUrl = 'http://localhost:8080/api/test-token';

  constructor(
    private http: HttpClient,
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.checkAuthentication();
    }
  }

  checkAuthentication(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const token = localStorage.getItem('authToken');
    const expirationDate = localStorage.getItem('tokenExpiration');

    if (!token || !expirationDate) {
      this.router.navigate(['/vulnerable-login']);
      return;
    }

    const expiration = new Date(expirationDate);
    const now = new Date();

    if (now >= expiration) {
      this.router.navigate(['/vulnerable-login']);
      return;
    }

    this.verifyToken(token);
  }

  verifyToken(token: string): void {
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    this.http.get(`${this.apiUrl}`, { headers }).subscribe({
      next: (_: any) => {
        this.isAuthenticated = true;
      },
      error: () => {
        this.router.navigate(['/vulnerable-login']);
      }
    });
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('authToken');
      localStorage.removeItem('tokenExpiration');
    }
    this.router.navigate(['/vulnerable-login']);
  }
}
