import { Component } from '@angular/core';

@Component({
  selector: 'app-customer',
  imports: [],
  templateUrl: './customer.html',
  styleUrl: './customer.css',
})
export class Customer {
  protected readonly ProductName = 'Headphone';
  protected readonly ProductDescription = 'High-quality wireless headphones with noise cancellation and long battery life.';
  protected readonly ProductPrice = '$199.99';


}
