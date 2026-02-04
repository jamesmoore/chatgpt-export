export const WRAP_STORAGE_KEY = 'conversation-wrap-enabled';

export function getWrapStatus(): boolean {
  return localStorage.getItem(WRAP_STORAGE_KEY) === 'true';
}

export function setWrapStatus(newValue: boolean) {
  localStorage.setItem(WRAP_STORAGE_KEY, String(newValue));
}