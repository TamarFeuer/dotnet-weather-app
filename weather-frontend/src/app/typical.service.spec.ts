import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TypicalService, TypicalInfo } from './typical.service';

// The service is the thing that actually builds and fires the HTTP request, so
// this is the one place where we DO want to look at HTTP. But not real HTTP:
// provideHttpClientTesting swaps the network out for a fake backend that hands
// us the request so we can inspect it, and lets us reply with whatever we like.
//
// So we test our side of the wire (is the URL right? is the month attached as a
// query parameter?) without a server, without the internet, and in milliseconds.
describe('TypicalService', () => {
  let service: TypicalService;
  let httpMock: HttpTestingController;

  const info: TypicalInfo = {
    minTemp: 14,
    maxTemp: 23,
    average: 18,
    description: 'Warm',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(TypicalService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  // Fails the test if a request was fired that we never accounted for.
  afterEach(() => httpMock.verify());

  it('GETs the typical endpoint with the month as a query parameter', () => {
    let received: TypicalInfo | undefined;

    service.getTypical('July').subscribe((r) => (received = r));

    const request = httpMock.expectOne(
      (r) => r.url === 'http://localhost:5151/api/weather/typical'
    );

    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('month')).toBe('July');

    // Reply with a fake response, the way the backend would.
    request.flush(info);

    expect(received).toEqual(info);
  });

  it('does not fire a request until something subscribes', () => {
    // getTypical only BUILDS the request; the Observable is a recipe. Nothing
    // travels until an effect (or a test) subscribes to it.
    service.getTypical('July');

    httpMock.expectNone(() => true);
  });
});
