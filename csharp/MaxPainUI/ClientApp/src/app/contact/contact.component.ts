// https://thecodegarden.net/angular-contact-component
// https://github.com/JeffreyRosselle/angular-contact-component/blob/master/src/app/contact/models/contact.model.ts

import { Component, OnInit } from '@angular/core';
import { Title } from "@angular/platform-browser";

import { DataService } from '../services/data.service';
import { EmailMessage } from '../models/email-message';

@Component( {
	selector: 'contactform',
	templateUrl: './contact.component.html',
	//This is just for styling and will add the class "row" to the parent-tag
	host: { 'class': 'row' }
})
export class ContactComponent implements OnInit {
	msg: EmailMessage = new EmailMessage();
	submitted: boolean = true;
	notification: string = "";

	constructor(
		private data: DataService,
		private title: Title) { 
	}
		
	ngOnInit() {
		this.submitted = false;
		this.title.setTitle("Contact");
	}

	onSubmit() {
		this.submitted = true;

		let body: string = "Email:\r\n" + this.msg.from + "\r\n\r\n" 
			+ "Subject:\r\n" + this.msg.subject + "\r\n\r\n"	
			+ this.msg.content;

		this.msg.to="info@maximum-pain.com";
		this.msg.from="info@maximum-pain.com";
		this.msg.isHtml = false;
		this.msg.body = body;

		//console.log(this.msg);
		//Don't forget to subscribe on an observable, or it will never be called.
		this.data.sendMail(this.msg).subscribe(
			() => {
				//Success
				this.msg = new EmailMessage();
				this.submitted = true;
				//this.toastr.success('Message send.');
				this.notification = "Message sent.";
			},
			() => {
				//Failed
				this.submitted = false;                
				//this.toastr.error('Woops, something went wrong.');
				this.notification = "Oops, something went wrong.";
			}
		);
	}
}