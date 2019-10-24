import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';


@Injectable()
export class MemberEditResolver implements Resolve<User> {

    constructor(
        private userService: UserService, private router: Router,
        private authService: AuthService, private alertify: AlertifyService) {

    }

    resolve(route: ActivatedRouteSnapshot): Observable<User> {

        return this.userService.GetUser(this.authService.decodedToken.nameid).pipe(
            catchError(error => {
                this.alertify.error('Problems while retrieving data!');
                // tslint:disable-next-line: no-unused-expression
                this.router.navigate['/members'];
                return of(null);
            })
        );
    }

}
