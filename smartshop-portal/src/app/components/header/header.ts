import { Component } from '@angular/core';
import { usernameSubject } from '../../rxjs/auth.operator';
import { signal } from '@angular/core';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {
  username = signal('Guest');

  constructor() {
    usernameSubject.subscribe({
      next: (username) => {
        if(username){
          this.username.set(username);
        }else{
          this.username.set('Guest');
        }
    }});
  }

  ondestroy(){
    usernameSubject.unsubscribe();
  }
}
