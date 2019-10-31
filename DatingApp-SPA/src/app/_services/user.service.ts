import { Injectable } from '@angular/core';
// import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from 'src/environments/environment';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/PaginatedResult';
import { Message } from '../_models/message';

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

  GetUsers(page?, itemsPerPage?, userParams?, LikesParams?): Observable<PaginatedResult<User[]>> {

    const paginatedResult: PaginatedResult<User[]> = { result : null, pagination: null };

    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
      // console.log(params);
    }

    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
      // console.log(params);
    }

    if (LikesParams === 'Likers') {
      params = params.append('likers', 'true');
      // console.log(params);
    }

    if (LikesParams === 'Likees') {
      params = params.append('likees', 'true');
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

  sendLike(userId: number, recipientId: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/like/' + recipientId, {});
  }

  getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
    const paginatedResult: PaginatedResult<Message[]> = { result : null, pagination: null };

    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
      // console.log(params);
    }

    params = params.append('messageContainer', messageContainer);

    return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages', { observe: 'response', params })
                    .pipe(map(response => {
                                paginatedResult.result = response.body;
                                if (response.headers.get('Pagination') != null) {
                                  paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
                                }
                                return paginatedResult;
                                }));
  }

  getMessageThread(id: number, recipientId: number) {
        return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages/thread/' + recipientId);
  }

  sendMessage(id: number, message: Message) {
    return this.http.post(this.baseUrl + 'users/' + id + '/messages', message);
  }

  deleteMessage(id: number, userId: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/messages/' + id , {});
  }

  markAsRead(userId: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/messages/' + id + '/read' , {})
    .subscribe();
  }
}
