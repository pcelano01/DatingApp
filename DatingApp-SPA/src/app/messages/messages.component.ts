import { Component, OnInit } from '@angular/core';
import { PaginatedResult } from '../_models/PaginatedResult';
import { Pagination } from '../_models/pagination';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Message } from '../_models/message';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[];
  pagination: Pagination;
  messageContainer: any = 'Unread';

  // tslint:disable-next-line: max-line-length
  constructor(private userService: UserService, private alertify: AlertifyService, private route: ActivatedRoute, private authService: AuthService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.messages = data['messages'].result;
      this.pagination = data['messages'].pagination;
    });
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    this.loadMessages(this.messageContainer);
  }

  loadMessages(container: any) {
    // tslint:disable-next-line: max-line-length
    this.messageContainer = container;
    this.userService.getMessages(this.authService.decodedToken.nameid, this.pagination.currentPage,
      this.pagination.itemsPerPage, this.messageContainer)
    .subscribe((res: PaginatedResult<Message[]>) => {
      this.messages = res.result;
      this.pagination = res.pagination;
      // console.log(this.messages);
    }, error => {
      this.alertify.error(error);
    });
  }

  deleteMessage(id: number): void {
    this.alertify.confirm('Are you sure to delete message?', () => {

      this.userService.deleteMessage(id, this.authService.decodedToken.nameid)
      .subscribe(() => {
      this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
      this.alertify.success('Message has been deleted!');
      }, error => {
        this.alertify.error('Failed to delete the message!');
      });

    });
  }
}
