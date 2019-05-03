/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { FakepageComponent } from './fakepage.component';

describe('FakepageComponent', () => {
  let component: FakepageComponent;
  let fixture: ComponentFixture<FakepageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ FakepageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FakepageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
