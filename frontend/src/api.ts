const getAuthHeaders = (): HeadersInit => {
  const token = localStorage.getItem('token');
  return token ? { 'Authorization': `Bearer ${token}` } : {};
};

export interface ApiResponse<T> {
  data?: T;
  error?: string;
  success: boolean;
  banned?: boolean;
}

async function request<T>(
  url: string,
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH' = 'GET',
  body?: any,
  isFormData: boolean = false
): Promise<ApiResponse<T>> {
  try {
    const headers: HeadersInit = {
      ...getAuthHeaders(),
    };

    if (!isFormData) {
      (headers as any)['Content-Type'] = 'application/json';
    }

    const config: RequestInit = {
      method,
      headers,
    };

    if (body) {
      config.body = isFormData ? body : JSON.stringify(body);
    }

    const versionedUrl = url.startsWith('/api/') ? url.replace('/api/', '/api/v1/') : url;
    const response = await fetch(versionedUrl, config);
    if (!response.ok) {
      const errText = await response.text();
      let errMsg = 'An error occurred';
      try {
        const errJson = JSON.parse(errText);
        errMsg = errJson.detail || errJson.title || errJson.message || errMsg;
      } catch {
        if (errText) errMsg = errText;
      }

      const isBanned = response.status === 403 && errMsg.toLowerCase().includes('banned');
      if (isBanned || response.status === 401) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
      }
      if (isBanned) {
        window.dispatchEvent(new CustomEvent('account-banned', { detail: errMsg }));
      }

      return { success: false, error: errMsg, banned: isBanned };
    }

    const text = await response.text();
    const data = text ? JSON.parse(text) : undefined;
    return { success: true, data };
  } catch (err) {
    return { success: false, error: err instanceof Error ? err.message : 'Network error' };
  }
}

export interface PaginatedEvents {
  items: any[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export const api = {
  // Auth
  register: (body: any) => request<any>('/api/auth/register', 'POST', body),
  login: (body: any) => request<any>('/api/auth/login', 'POST', body),
  me: () => request<any>('/api/auth/me', 'GET'),
  checkAvailability: (username?: string, email?: string) =>
    request<{ usernameExists: boolean; emailExists: boolean }>(
      `/api/auth/check-availability?username=${encodeURIComponent(username || '')}&email=${encodeURIComponent(email || '')}`,
      'GET'
    ),

  // Categories
  getCategories: () => request<any[]>('/api/categories', 'GET'),
  adminCreateCategory: (body: any) => request<any>('/api/admin/categories', 'POST', body),
  adminUpdateCategory: (id: string, body: any) => request<any>(`/api/admin/categories/${id}`, 'PUT', body),
  adminDeleteCategory: (id: string) => request<any>(`/api/admin/categories/${id}`, 'DELETE'),

  // Events
  getEvents: (categoryId?: string, searchTerm?: string, status?: string, page?: number, pageSize?: number, organizerId?: string) => {
    let url = '/api/events';
    const params = new URLSearchParams();
    if (categoryId) params.append('categoryId', categoryId);
    if (searchTerm) params.append('searchTerm', searchTerm);
    if (status) params.append('status', status);
    if (page) params.append('page', page.toString());
    if (pageSize) params.append('pageSize', pageSize.toString());
    if (organizerId) params.append('organizerId', organizerId);
    const qs = params.toString();
    if (qs) url += `?${qs}`;
    return request<PaginatedEvents>(url, 'GET');
  },
  getMapEvents: () => request<any[]>('/api/events/map', 'GET'),
  getEventById: (id: string) => request<any>(`/api/events/${id}`, 'GET'),
  createEvent: (body: any) => request<any>('/api/events', 'POST', body),
  updateEvent: (id: string, body: any) => request<any>(`/api/events/${id}`, 'PUT', body),
  deleteEvent: (id: string) => request<any>(`/api/events/${id}`, 'DELETE'),
  publishEvent: (id: string) => request<any>(`/api/events/${id}/publish`, 'POST'),
  cancelEvent: (id: string) => request<any>(`/api/events/${id}/cancel`, 'POST'),
  uploadImage: (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    return request<{ success?: boolean; data: { url: string } }>('/api/events/upload-image', 'POST', formData, true);
  },

  // Bookings
  joinEvent: (eventId: string) => request<any>(`/api/bookings/${eventId}/join`, 'POST'),
  leaveEvent: (eventId: string) => request<any>(`/api/bookings/${eventId}/leave`, 'DELETE'),
  getMyBookings: () => request<any[]>('/api/bookings/my', 'GET'),
  getEventAttendees: (eventId: string) => request<any[]>(`/api/events/${eventId}/attendees`, 'GET'),

  // Comments
  getComments: (eventId: string) => request<any[]>(`/api/events/${eventId}/comments`, 'GET'),
  createComment: (eventId: string, body: any) => request<any>(`/api/events/${eventId}/comments`, 'POST', body),
  updateComment: (eventId: string, commentId: string, body: any) => request<any>(`/api/events/${eventId}/comments/${commentId}`, 'PUT', body),
  deleteComment: (eventId: string, commentId: string) => request<any>(`/api/events/${eventId}/comments/${commentId}`, 'DELETE'),

  // Admin Panel User actions
  adminGetUsers: () => request<any[]>('/api/admin/users', 'GET'),
  adminBanUser: (userId: string) => request<any>(`/api/admin/users/${userId}/ban`, 'PATCH'),
  adminUnbanUser: (userId: string) => request<any>(`/api/admin/users/${userId}/unban`, 'PATCH'),
  adminForceDeleteEvent: (id: string) => request<any>(`/api/admin/events/${id}`, 'DELETE'),
  adminForceDeleteComment: (id: string) => request<any>(`/api/admin/comments/${id}`, 'DELETE'),
};
