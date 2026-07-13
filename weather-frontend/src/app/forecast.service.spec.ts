import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ForecastService, ForecastDay } from './forecast.service';

describe('ForecastService', () => {
  let service: ForecastService;
  let httpMock: HttpTestingController;

  const days: ForecastDay[] = [
    { date: '2026-07-03', minTemp: 13, maxTemp: 21, condition: 'Cloudy' },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(ForecastService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('GETs the cities endpoint', () => {
    let received: string[] | undefined;

    service.getCities().subscribe((r) => (received = r));

    const request = httpMock.expectOne('http://localhost:5151/api/weather/cities');
    expect(request.request.method).toBe('GET');

    request.flush(['Amsterdam', 'Rotterdam']);

    expect(received).toEqual(['Amsterdam', 'Rotterdam']);
  });

  it('GETs the forecast endpoint with the city as a query parameter', () => {
    let received: ForecastDay[] | undefined;

    service.getForecast('Amsterdam').subscribe((r) => (received = r));

    const request = httpMock.expectOne(
      (r) => r.url === 'http://localhost:5151/api/weather/forecast'
    );

    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('city')).toBe('Amsterdam');

    request.flush(days);

    expect(received).toEqual(days);
  });

  it('url-encodes a city name containing a space', () => {
    // "The Hague" must not break the query string. HttpClient encodes it for us,
    // but this pins that down so nobody later hand-builds the URL and breaks it.
    service.getForecast('The Hague').subscribe();

    const request = httpMock.expectOne(
      (r) => r.url === 'http://localhost:5151/api/weather/forecast'
    );

    expect(request.request.params.get('city')).toBe('The Hague');
    expect(request.request.urlWithParams).toContain('city=The%20Hague');

    request.flush([]);
  });
});
