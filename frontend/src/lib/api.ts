import { API_BASE_URL } from "@/lib/config";

export interface ApiError {
  status: number;
  message: string;
}

async function request<T>(input: RequestInfo, init?: RequestInit): Promise<T> {
  const res = await fetch(input, {
    ...init,
    credentials: "include",
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers ?? {}),
    },
  });

  if (!res.ok) {
    let message = res.statusText;
    try {
      const body = await res.json();
      message = body?.title ?? body?.message ?? message;
    } catch {
      // ignore
    }

    const error: ApiError = {
      status: res.status,
      message,
    };
    throw error;
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return (await res.json()) as T;
}

export function getJson<T>(path: string, token?: string): Promise<T> {
  return request<T>(`${API_BASE_URL}${path}`, {
    method: "GET",
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
}

export function postJson<TBody, TResult>(path: string, body: TBody, token?: string): Promise<TResult> {
  return request<TResult>(`${API_BASE_URL}${path}`, {
    method: "POST",
    body: JSON.stringify(body),
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
}

export function putJson<TBody, TResult>(path: string, body: TBody, token?: string): Promise<TResult> {
  return request<TResult>(`${API_BASE_URL}${path}`, {
    method: "PUT",
    body: JSON.stringify(body),
    headers: token ? { Authorization: `Bearer ${token}` } : undefined,
  });
}
