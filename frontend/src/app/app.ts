import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './layouts/header/header.component';
import { FooterComponent } from './layouts/footer/footer.component';
import { ChatbotComponent } from './features/chatbot/chatbot.component';
import { ToastComponent } from './shared/components/toast/toast.component';
import { CartDrawerComponent } from './shared/components/cart-drawer/cart-drawer.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, FooterComponent, ChatbotComponent, ToastComponent, CartDrawerComponent],
  template: `
    <app-toast />
    <app-cart-drawer />
    <app-header />
    <main>
      <router-outlet />
    </main>
    <app-footer />
    <app-chatbot />
  `,
  styles: `main { min-height: calc(100vh - 160px); }`
})
export class AppComponent {}
