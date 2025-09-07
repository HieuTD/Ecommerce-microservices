import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IProduct } from './shared/models/product';
import { IPagination } from './shared/models/pagination';

@Component({
  selector: 'app-root',
  // standalone: true,
  // imports: [RouterOutlet, NavbarComponent, HttpClientModule, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'eShoping';
  products: IProduct[] = []

  constructor(private http: HttpClient) {}
  
  ngOnInit(): void {
    this.http.get<IPagination<IProduct[]>>('http://localhost:8000/api/v1/Catalog/GetAllProducts').subscribe(
      {
        next:response => {
          this.products = response.data,
          console.log(response)
        },
        error:error => console.log(error),
        complete:()=> {
          console.log('Catalog API call completed');
        }
      }
    )  
  }
}
