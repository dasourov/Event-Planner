const getAuthHeaders = (): HeadersInit => {
  const token = localStorage.getItem('token');
  return token ? { 'Authorization': `Bearer ${token}` } : {};
};

export interface ApiResponse<T> {
  data?: T;
  error?: string;
  success: boolean;
}

async function request<T>(
  url: string,
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' = 'GET',
  body?: any
): Promise<ApiResponse<T>> {
  try {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
      ...getAuthHeaders(),
    };

    const config: RequestInit = {
      method,
      headers,
    };

    if (body) {
      config.body = JSON.stringify(body);
    }

    const response = await fetch(url, config);
    if (!response.ok) {
      if (response.status === 401) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
      const errText = await response.text();
      let errMsg = 'An error occurred';
      try {
        const errJson = JSON.parse(errText);
        errMsg = errJson.detail || errJson.message || errMsg;
      } catch {
        errMsg = errText || errMsg;
      }
      return { success: false, error: errMsg };
    }

    const text = await response.text();
    const data = text ? JSON.parse(text) : undefined;
    return { success: true, data };
  } catch (err) {
    return { success: false, error: err instanceof Error ? err.message : 'Network error' };
  }
}

export const api = {
  // Auth
  register: (body: any) => request<any>('/api/auth/register', 'POST', body),
  login: (body: any) => request<any>('/api/auth/login', 'POST', body),
  me: () => request<any>('/api/auth/me', 'GET'),

  // Categories
  getCategories: () => request<any[]>('/api/categories', 'GET'),
  adminCreateCategory: (body: any) => request<any>('/api/admin/categories', 'POST', body),
  adminUpdateCategory: (id: string, body: any) => request<any>(`/api/admin/categories/${id}`, 'PUT', body),
  adminDeleteCategory: (id: string) => request<any>(`/api/admin/categories/${id}`, 'DELETE'),

  // Events
  getEvents: (categoryId?: string, searchTerm?: string) => {
    let url = '/api/events';
    const params = new URLSearchParams();
    if (categoryId) params.append('categoryId', categoryId);
    if (searchTerm) params.append('searchTerm', searchTerm);
    const qs = params.toString();
    if (qs) url += `?${qs}`;
    return request<any[]>(url, 'GET');
  },
  getMapEvents: () => request<any[]>('/api/events/map', 'GET'),
  getEventById: (id: string) => request<any>(`/api/events/${id}`, 'GET'),
  createEvent: (body: any) => request<any>('/api/events', 'POST', body),
  updateEvent: (id: string, body: any) => request<any>(`/api/events/${id}`, 'PUT', body),
  deleteEvent: (id: string) => request<any>(`/api/events/${id}`, 'DELETE'),
  publishEvent: (id: string) => request<any>(`/api/events/${id}/publish`, 'POST'),
  cancelEvent: (id: string) => request<any>(`/api/events/${id}/cancel`, 'POST'),

  // Bookings
  joinEvent: (eventId: string) => request<any>(`/api/bookings/${eventId}/join`, 'POST'),
  leaveEvent: (eventId: string) => request<any>(`/api/bookings/${eventId}/leave`, 'DELETE'),
  getMyBookings: () => request<any[]>('/api/bookings/my', 'GET'),
  getEventAttendees: (eventId: string) => request<any[]>(`/api/events/${eventId}/attendees`, 'GET'),

  // Comments
  getComments: (eventId: string) => request<any[]>(`/api/events/${eventId}/comments`, 'GET'),
  createComment: (eventId: string, body: any) => request<any>(`/api/events/${eventId}/comments`, 'POST', body),
  updateComment: (commentId: string, body: any) => request<any>(`/api/comments/${commentId}`, 'PUT', body),
  deleteComment: (commentId: string) => request<any>(`/api/comments/${commentId}`, 'DELETE'),

  // Admin Panel User actions
  adminGetUsers: () => request<any[]>('/api/admin/users', 'GET'),
  adminBanUser: (userId: string) => request<any>(`/api/admin/users/${userId}/ban`, 'POST'),
  adminForceDeleteEvent: (id: string) => request<any>(`/api/admin/events/${id}`, 'DELETE'),
  adminForceDeleteComment: (id: string) => request<any>(`/api/admin/comments/${id}`, 'DELETE'),
};
