export interface AppEnvironment {
  production: boolean;
  apiBase: string;
  imageBase: string;
}

declare global { interface Window { ENV_CONFIG?: Partial<AppEnvironment>; } }

function resolveApiBase(): string {
  if (typeof window === 'undefined') return 'http://localhost:5078';
  if (window.ENV_CONFIG?.apiBase) return window.ENV_CONFIG.apiBase as string;
  const host = window.location.hostname;
  // Local dev in browser (not inside Docker network)
  if (host === 'localhost' || host === '127.0.0.1') {
    return 'http://localhost:5078';
  }
  // If served from container behind same host, try same host + 5078
  const port = window.location.port;
  // If current port already 5078 assume same origin
  if (port === '5078') return window.location.origin;
  return window.location.protocol + '//' + host + ':5078';
}

const baseUrl = resolveApiBase();

export const environment: AppEnvironment = {
  production: false,
  apiBase: baseUrl,
  imageBase: baseUrl
};
