import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-chatbot',
  imports: [FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrl: './chatbot.component.css'
})
export class ChatbotComponent {
  isOpen = signal(false);
  messages = signal<{ sender: string; message: string; timestamp: Date }[]>([
    { sender: 'bot', message: "Hey there! 👋 I'm your Fashion Store assistant. Ask me about products, sizes, orders, or any style advice!", timestamp: new Date() }
  ]);
  typing = signal(false);
  input = '';

  quickReplies = ['Size guide', 'Return policy', 'Track order', 'Best deals'];

  private mockResponses: Record<string, string> = {
    'size guide': "📏 We follow standard Indian sizing:\n• S: Chest 36-38\"\n• M: Chest 38-40\"\n• L: Chest 40-42\"\n• XL: Chest 42-44\"\n\nFor footwear, we use UK sizing. Check each product for specific size charts!",
    'return policy': "↩️ Our return policy:\n• 7-day easy returns\n• Free return pickup\n• Full refund within 5 business days\n• Items must be unused with tags\n\nSimply go to Orders → Select Order → Request Return",
    'track order': "📦 To track your order:\n1. Go to 'My Orders' from the menu\n2. Click on your order\n3. See real-time tracking updates!\n\nYou'll also receive email & in-app notifications for every status change.",
    'best deals': "🔥 Current best deals:\n• WELCOME50 - ₹50 off first order\n• FLAT20 - 20% off (min ₹999)\n• MEGA30 - 30% off (min ₹1999)\n\nCheck our featured products for items with up to 43% discount!",
    'shipping': "🚚 Shipping info:\n• Free shipping on orders ₹499+\n• Standard delivery: 3-7 business days\n• Express delivery available at checkout\n• We deliver across India!",
  };

  send() {
    if (!this.input.trim()) return;
    const userMsg = this.input.trim();
    this.messages.update(m => [...m, { sender: 'user', message: userMsg, timestamp: new Date() }]);
    this.input = '';
    this.typing.set(true);

    setTimeout(() => {
      const key = Object.keys(this.mockResponses).find(k => userMsg.toLowerCase().includes(k));
      const reply = key ? this.mockResponses[key] : this.getGenericReply(userMsg);
      this.messages.update(m => [...m, { sender: 'bot', message: reply, timestamp: new Date() }]);
      this.typing.set(false);
    }, 800 + Math.random() * 800);
  }

  private getGenericReply(msg: string): string {
    const lower = msg.toLowerCase();
    if (lower.includes('hello') || lower.includes('hi')) return "Hello! 👋 Welcome to Fashion Store. How can I help you today? You can ask about products, sizes, orders, or style advice!";
    if (lower.includes('thanks') || lower.includes('thank')) return "You're welcome! 😊 Happy to help. Is there anything else you'd like to know?";
    if (lower.includes('price') || lower.includes('cost')) return "💰 Our prices range from ₹499 to ₹6,999. We frequently have sales with up to 43% off! Check our featured products for the best deals.";
    if (lower.includes('men') || lower.includes('male')) return "👔 Our Men's collection includes T-Shirts, Shirts, Jeans, Jackets, Ethnic Wear & Activewear. Browse at: Shop → Men";
    if (lower.includes('women') || lower.includes('female')) return "👗 Our Women's collection features Dresses, Tops, Sarees, Palazzos, Jackets & more. Browse at: Shop → Women";
    return "Thanks for your message! 😊 I can help with:\n• 📏 Size guides\n• ↩️ Return policy\n• 📦 Order tracking\n• 🔥 Current deals\n• 👔👗 Product recommendations\n\nJust ask away!";
  }

  formatTime(date: Date): string {
    return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  }
}
