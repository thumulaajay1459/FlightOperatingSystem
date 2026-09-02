import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';

interface ChatMessage {
  role: string;
  content: string;
  createdAt: Date;
}

interface ChatSession {
  sessionId: number;
  createdAt: Date;
  lastMessageAt: Date;
  messageCount: number;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  messages: ChatMessage[] = [];
  userMessage = '';
  isLoading = false;
  currentSessionId: number | null = null;
  sessions: ChatSession[] = [];
  showSessions = false;
  isAuthenticated = false;
  showLoginPrompt = false;

  private apiUrl = 'http://localhost:5000';

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit() {
    this.checkAuthentication();
    if (this.isAuthenticated) {
      this.loadSessions();
    }
  }

  private checkAuthentication() {
    const token = localStorage.getItem('token') || localStorage.getItem('accessToken');
    this.isAuthenticated = !!token;
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || localStorage.getItem('accessToken');
    if (token) {
      return new HttpHeaders().set('Authorization', `Bearer ${token}`);
    }
    return new HttpHeaders();
  }

  loadSessions() {
    this.http.get<ChatSession[]>(`${this.apiUrl}/chat/sessions`, { headers: this.getHeaders() })
      .subscribe({
        next: (sessions) => this.sessions = sessions,
        error: (err) => console.error('Failed to load sessions', err)
      });
  }

  loadSessionHistory(sessionId: number) {
    this.http.get<{ sessionId: number, messages: ChatMessage[] }>(
      `${this.apiUrl}/chat/sessions/${sessionId}/history`,
      { headers: this.getHeaders() }
    ).subscribe({
      next: (data) => {
        this.currentSessionId = sessionId;
        this.messages = data.messages;
        this.showSessions = false;
      },
      error: (err) => console.error('Failed to load history', err)
    });
  }

  sendMessage() {
    if (!this.userMessage.trim()) return;

    const userMsg = this.userMessage;
    this.messages.push({ role: 'User', content: userMsg, createdAt: new Date() });
    this.userMessage = '';
    this.isLoading = true;

    // Check if user needs authentication for this query
    if (!this.isAuthenticated && this.requiresAuth(userMsg)) {
      this.isLoading = false;
      this.messages.push({ 
        role: 'Assistant', 
        content: 'To access your bookings and personal information, please login first. Click the Login button in the navigation bar.', 
        createdAt: new Date() 
      });
      this.showLoginPrompt = true;
      return;
    }

    const body = {
      message: userMsg,
      sessionId: this.currentSessionId
    };

    const headers = this.isAuthenticated ? this.getHeaders() : new HttpHeaders();

    this.http.post<{ sessionId: number, reply: string, timestamp: Date }>(
      `${this.apiUrl}/chat`,
      body,
      { headers }
    ).subscribe({
      next: (response) => {
        this.currentSessionId = response.sessionId;
        this.messages.push({ role: 'Assistant', content: response.reply, createdAt: response.timestamp });
        this.isLoading = false;
        if (this.isAuthenticated) {
          this.loadSessions();
        }
      },
      error: (err) => {
        console.error('Chat error', err);
        if (err.status === 401) {
          this.messages.push({ 
            role: 'Assistant', 
            content: 'Your session has expired. Please login again to continue.', 
            createdAt: new Date() 
          });
          this.showLoginPrompt = true;
        } else {
          this.messages.push({ role: 'Assistant', content: 'Sorry, I encountered an error. Please try again.', createdAt: new Date() });
        }
        this.isLoading = false;
      }
    });
  }

  private requiresAuth(message: string): boolean {
    const authKeywords = ['my booking', 'my bookings', 'my ticket', 'my tickets', 'my reservation', 
                          'my flight', 'my trips', 'my account', 'my profile', 'cancel booking'];
    const lowerMsg = message.toLowerCase();
    return authKeywords.some(keyword => lowerMsg.includes(keyword));
  }

  newChat() {
    this.currentSessionId = null;
    this.messages = [];
  }

  deleteSession(sessionId: number) {
    this.http.delete(`${this.apiUrl}/chat/sessions/${sessionId}`, { headers: this.getHeaders() })
      .subscribe({
        next: () => {
          this.loadSessions();
          if (this.currentSessionId === sessionId) {
            this.newChat();
          }
        },
        error: (err) => console.error('Failed to delete session', err)
      });
  }

  toggleSessions() {
    this.showSessions = !this.showSessions;
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
