import axios from 'axios';

export const api = axios.create({
  baseURL: 'http://localhost:5052/api',
  timeout: 20000
});

api.interceptors.request.use(cfg => {
  const token = localStorage.getItem('access_token');
  if (token) {
    cfg.headers = cfg.headers ?? {};
    (cfg.headers as any).Authorization = `Bearer ${token}`;
  }
  return cfg;
});

api.interceptors.response.use(
  r => r,
  err => {
    if (err?.response?.status === 401) {
      localStorage.removeItem('access_token');
      if (!location.pathname.startsWith('/login')) location.href = '/login';
    }
    return Promise.reject(err);
  }
);

export async function loginRequest(email: string, password: string) {
  const { data } = await api.post('/Auth/login', { email, password });
  // Backend returns { accessToken, expiresAt, ... }
  const token = (data?.accessToken ?? data?.AccessToken ?? data?.token) as string;
  return { token } as { token: string };
}

export async function meRequest() {
  const { data } = await api.get('/Auth/me');
  return data as { id: string; displayName: string; email: string; role: 'User'|'Admin' };
}

export type PostListItem = { id: string; title: string; categoryName: string; isPublished: boolean; createdAt: string };
export async function postsList(params: { search?: string; page: number; pageSize: number }) {
  // Public published posts endpoint
  const { data } = await api.get('/Posts', { params });
  return data as { items: PostListItem[]; page: number; pageSize: number; total: number };
}
export async function postDelete(id: string) { await api.delete(`/Admin/AdminPosts/${id}`); }
export async function postTogglePublish(id: string, publish: boolean) { await api.post(`/Admin/AdminPosts/${id}/${publish ? 'publish' : 'unpublish'}`, {}); }
export async function postCreate(body: { title: string; slug: string; content: string; categoryId: string; summary?: string | null; isActive: boolean; }) {
  const { data } = await api.post('/Admin/AdminPosts', body);
  return data as { id: string };
}

export type Category = { id: string; name: string; slug: string; parentId?: string | null; displayOrder: number; isActive: boolean };
export async function categoriesList() { const { data } = await api.get('/Categories/active'); return data as Category[]; }
export async function categoryUpsert(id: string | null, body: Omit<Category,'id'> & { description?: string | null }) {
  if (id) await api.put(`/Admin/AdminCategories/${id}`, body); else await api.post('/Admin/AdminCategories', body);
}
export async function categoryDelete(id: string) { await api.delete(`/Admin/AdminCategories/${id}`); }

export type CommentRow = { id: string; postTitle: string; author: string; content: string; isApproved: boolean; createdAt: string };
export async function commentsList() {
  try {
    const { data } = await api.get('/Admin/Comments');
    return data as CommentRow[];
  } catch (e: any) {
    if (e?.response?.status === 404) return [] as CommentRow[]; // backend liste endpointi yoksa boş dön
    throw e;
  }
}
export async function commentApprove(id: string) { await api.post(`/Admin/Comments/${id}/approve`, {}); }
export async function commentDelete(id: string) { await api.delete(`/Admin/Comments/${id}`); }

export type UserRow = { id: string; displayName: string; email: string; role: 'User'|'Admin'; isActive: boolean };
export async function usersList() {
  try {
    const { data } = await api.get('/Admin/Users');
    return data as UserRow[];
  } catch (e: any) {
    if (e?.response?.status === 404) return [] as UserRow[]; // backend liste endpointi yoksa boş dön
    throw e;
  }
}
export async function userSetActive(id: string, active: boolean) { await api.post(`/Admin/Users/${id}/${active ? 'activate' : 'deactivate'}`, {}); }


