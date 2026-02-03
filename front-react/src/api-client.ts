import type { ConversationSummary } from './models';

const BaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';
const ApiUrl = `${BaseUrl}/conversations`;

/**
 * Fetches with error handling - throws on non-2xx responses
 */
async function fetchJson<T>(url: string, options?: RequestInit): Promise<T> {
  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(
      `API request failed: ${response.status} ${response.statusText}`
    );
  }
  return response.json();
}

/**
 * Fetches as text with error handling - throws on non-2xx responses
 */
async function fetchText(url: string, options?: RequestInit): Promise<string> {
  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(
      `API request failed: ${response.status} ${response.statusText}`
    );
  }
  return response.text();
}

/**
 * Returns the latest conversation details for each instance
 */
export async function getConversations(): Promise<ConversationSummary[]> {
  return fetchJson<ConversationSummary[]>(ApiUrl);
}

/**
 * Returns the conversation in HTML format
 */
export async function getConversationHtml(id: string): Promise<string> {
  return fetchText(`${ApiUrl}/${id}/html`);
}

/**
 * Returns the conversation in Markdown format
 */
export async function getConversationMarkdown(id: string): Promise<string> {
  return fetchText(`${ApiUrl}/${id}/markdown`);
}

/**
 * Returns the conversation in JSON format
 */
export async function getConversationJson(id: string): Promise<string> {
  return fetchText(`${ApiUrl}/${id}/json`);
}

export type { ConversationSummary } from './models';
