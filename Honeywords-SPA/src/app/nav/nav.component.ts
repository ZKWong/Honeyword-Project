import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';
import { User } from '../_models/user';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
  user: User;

  constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) { }

  ngOnInit() {
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.alertify.success('Logged in successfully');
    }, error => {
      this.alertify.error(error);
    }, () => {
      this.user = JSON.parse(localStorage.getItem('user'));
      console.log(this.user);
      console.log(this.user['weather']);
      if (this.user.weather == '0') {
        this.router.navigate(['/mainpage']);
      } else {
        // this.router.navigate(['/fakepage']);
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        this.authService.decodedToken = null;
        this.authService.honeyuser = null;
        this.alertify.message('logged out');
        this.router.navigate(['/home']);
        this.model.username = '';
        this.model.password = '';
        window.location.href = 'http://www2.cs.siu.edu/~zwong/new.html';
      }
    });
  }
  loggedIn() {
    return this.authService.loggedIn();
  }
  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authService.decodedToken = null;
    this.authService.honeyuser = null;
    this.alertify.message('logged out');
    this.router.navigate(['/home']);
    this.model.username = '';
    this.model.password = '';
  }

}
