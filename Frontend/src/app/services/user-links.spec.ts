import { TestBed } from '@angular/core/testing';

import { UserLinksService } from './user-links';

describe('UserLinks', () => {
  let service: UserLinksService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UserLinksService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
