import { Injectable } from '@angular/core';
// import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/PaginatedResult';

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

  // GetUsers(): Observable<User[]> {
  //   // return this.http.get<User[]>(this.baseUrl + 'users/', httpOptions);
  //   return this.http.get<User[]>(this.baseUrl + 'users/');
  // }

  GetUsers(page?, itemsPerPage?): Observable<PaginatedResult<User[]>> {

    const paginatedResult: PaginatedResult<User[]> = { result : null, pagination: null };

    let params = new HttpParams();
    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
      // console.log(params);
    }
    return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params })
                    .pipe(map(response => {
                                paginatedResult.result = response.body;
                                if (response.headers.get('Pagination') != null) {
                                  paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
                                }
                                return paginatedResult;
                                }));
  }

  GetUser(id): Observable<User> {
    // return this.http.get<User>(this.baseUrl + 'users/' + id, httpOptions);
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userId: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + id + '/SetMain', {});
  }

  deletePhoto(userId: number, id: number) {
    return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id, {});
  }
}
