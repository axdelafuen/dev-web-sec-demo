import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-vulnerable-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './vulnerable-login.component.html',
  styleUrls: ['./vulnerable-login.component.css']
})
export class VulnerableLoginComponent {
  username: string = '';
  password: string = '';
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;
  attemptCount: number = 0;
  logs: string[] = [];

  regUsername: string = '';
  regPassword: string = '';
  registerError: string = '';
  registerSuccess: string = '';
  isRegisterLoading: boolean = false;

  private apiUrl = 'http://localhost:8080/api/vulnerable';

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  onSubmit(): void {
    if (!this.username || !this.password) {
      this.errorMessage = 'Please fill all fields';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';
    this.attemptCount++;

    const credentials = {
      username: this.username,
      password: this.password
    };

    const timestamp = new Date().toLocaleTimeString();
    this.addLog(`[${timestamp}] Tentative ${this.attemptCount}: ${this.username}`);

    this.http.post(`${this.apiUrl}/login`, credentials).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        this.successMessage = 'Login successful';
        this.addLog(`[${timestamp}] SUCCESS - Token received`);
        console.log('Login successful:', response);
      },
      error: (error) => {
        this.isLoading = false;
        // VULNERABILITY: Detailed error messages are displayed
        this.errorMessage = error.error?.message || 'Connection error';
        this.addLog(`[${timestamp}] FAILED - ${error.error?.message || 'Error'}`);
      }
    });
  }

  bruteForce(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.addLog('=== BRUTE FORCE START ===');
    
    const commonPasswords = ['123456', 'password', '123', 'admin', 'qwerty', '12345', 'letmein'];
    let delay = 0;

    commonPasswords.forEach((pwd, index) => {
      setTimeout(() => {
        this.password = pwd;
        this.onSubmit();
      }, delay);
      delay += 500; // 500ms between attempts
    });
  }

  private addLog(message: string): void {
    this.logs.unshift(message);
    if (this.logs.length > 20) {
      this.logs.pop();
    }
  }

  clearLogs(): void {
    this.logs = [];
    this.attemptCount = 0;
  }

  onRegister(): void {
    if (!this.regUsername || !this.regPassword) {
      this.registerError = 'Please fill all fields';
      return;
    }

    this.isRegisterLoading = true;
    this.registerError = '';
    this.registerSuccess = '';

    const credentials = {
      username: this.regUsername,
      password: this.regPassword
    };

    this.http.post(`${this.apiUrl}/register`, credentials).subscribe({
      next: () => {
        this.isRegisterLoading = false;
        this.registerSuccess = 'Account created successfully';
        this.regUsername = '';
        this.regPassword = '';
      },
      error: (error) => {
        this.isRegisterLoading = false;
        this.registerError = error.error?.message || 'Registration failed';
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
