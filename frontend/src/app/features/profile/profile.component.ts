import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';

const PRESET_AVATARS = [
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Felix',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Luna',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Buddy',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Milo',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Bella',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Max',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Charlie',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Oliver',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Sophie',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Lucy',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Coco',
  'https://api.dicebear.com/8.x/avataaars/svg?seed=Ruby',
];

@Component({
  selector: 'app-profile',
  imports: [FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  auth = inject(AuthService);
  private userService = inject(UserService);

  presetAvatars = PRESET_AVATARS;
  name = '';
  email = '';
  saved = signal(false);
  addresses = signal<any[]>([]);
  showAddAddress = signal(false);
  newAddr = { label: 'Home', street: '', city: '', state: '', pincode: '', isDefault: false };

  ngOnInit() {
    this.name = this.auth.user()?.name || '';
    this.email = this.auth.user()?.email || '';
    this.userService.getAddresses().subscribe(a => this.addresses.set(a));
  }

  saveProfile() {
    this.userService.updateProfile({ name: this.name, email: this.email }).subscribe(user => {
      this.auth.updateUser(user);
      this.saved.set(true);
      setTimeout(() => this.saved.set(false), 2000);
    });
  }

  selectAvatar(url: string) {
    this.auth.updateUser({ ...this.auth.user()!, avatar: url });
  }

  toggleAddAddress() {
    this.showAddAddress.update(v => !v);
  }

  addAddress() {
    this.userService.addAddress(this.newAddr).subscribe(addr => {
      this.addresses.update(list => [...list, addr]);
      this.showAddAddress.set(false);
      this.newAddr = { label: 'Home', street: '', city: '', state: '', pincode: '', isDefault: false };
    });
  }

  deleteAddress(id: number) {
    this.userService.deleteAddress(id).subscribe(() => {
      this.addresses.update(list => list.filter(a => a.id !== id));
    });
  }
}
