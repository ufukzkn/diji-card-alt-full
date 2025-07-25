import { TestBed } from '@angular/core/testing';

import { DefinitionsService } from './definitions';

describe('Definitions', () => {
  let service: DefinitionsService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DefinitionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
