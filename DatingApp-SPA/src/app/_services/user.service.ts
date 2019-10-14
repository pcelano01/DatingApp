import { Injectable } from '@angular/core';
// import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { User } from '../_models/User';

// const httpOptions = {
//         headers: new HttpHeaders({
//           Authorization: 'Bearer ' + localStorage.getItem('token')
//         })
// };

@Injectable({
  providedIn: 'root'
})
export class UserService {

  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  GetUsers(): Observable<User[]> {
    // return this.http.get<User[]>(this.baseUrl + 'users/', httpOptions);
    return this.http.get<User[]>(this.baseUrl + 'users/');
  }

  GetUser(id): Observable<User> {
    // return this.http.get<User>(this.baseUrl + 'users/' + id, httpOptions);
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }
}
