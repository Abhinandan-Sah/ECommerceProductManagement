import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductManagerDashboardComponent } from './product-manager-dashboard.component';

describe('ProductManagerDashboardComponent', () => {
  let component: ProductManagerDashboardComponent;
  let fixture: ComponentFixture<ProductManagerDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductManagerDashboardComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductManagerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
