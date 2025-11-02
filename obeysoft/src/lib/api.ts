import axios from "axios";

const BASE_URL = (import.meta as any).env?.VITE_API_BASE_URL || "http://localhost:5052";
const TOKEN_KEY = "access_token";

export const api = axios.create({
  baseURL: `${BASE_URL}/api`,
  timeout: 25000
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    config.headers = config.headers ?? {};
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error?.response?.status === 401) {
      localStorage.removeItem(TOKEN_KEY);
      if (!window.location.pathname.startsWith("/login")) {
        window.location.replace("/login");
      }
    }
    return Promise.reject(error);
  }
);

export const tokenStorage = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (token: string) => localStorage.setItem(TOKEN_KEY, token),
  clear: () => localStorage.removeItem(TOKEN_KEY)
};

export type Me = {
  id: string;
  displayName: string;
  email: string;
  role: "Admin" | "User";
};

export type Category = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  displayOrder?: number;
  isActive?: boolean;
  parentId?: string | null;
};

export type Paginated<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type PostListItem = {
  id: string;
  title: string;
  slug: string;
  summary?: string | null;
  content?: string;
  categoryName: string;
  categoryId?: string;
  isPublished: boolean;
  isActive?: boolean;
  createdAt: string;
  publishedAt?: string | null;
};

export type PostDetail = {
  id: string;
  title: string;
  slug: string;
  summary?: string | null;
  content: string;
  categoryId: string;
  categoryName: string;
  publishedAt?: string | null;
  isPublished: boolean;
  isActive: boolean;
};

export type CommentRow = {
  id: string;
  postTitle: string;
  author: string;
  content: string;
  isApproved: boolean;
  createdAt: string;
};

export type UserRow = {
  id: string;
  displayName: string;
  email: string;
  role: "Admin" | "User";
  isActive: boolean;
};

export type NavigationItemRow = {
  id: string;
  label: string;
  href: string;
  parentId?: string | null;
  displayOrder: number;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string | null;
  parentName?: string | null;
};

export async function loginRequest(email: string, password: string) {
  const { data } = await api.post("/Auth/login", { email, password });
  const token = (data?.accessToken ?? data?.AccessToken ?? data?.token) as string;
  return { token };
}

export async function meRequest(): Promise<Me> {
  const { data } = await api.get("/Auth/me");
  const normalized: Me = {
    id: data?.userId ?? data?.id,
    displayName: data?.displayName,
    email: data?.email,
    role: (data?.role ?? "User") as Me["role"]
  };
  return normalized;
}

export async function getCategories(): Promise<Category[]> {
  const { data } = await api.get("/Categories/active");
  return data ?? [];
}

export async function getPosts(params: {
  page: number;
  pageSize: number;
  category?: string;
  search?: string;
}): Promise<Paginated<PostListItem>> {
  const { data } = await api.get("/Posts", { params });
  return {
    items: data?.items ?? [],
    page: data?.page ?? params.page,
    pageSize: data?.pageSize ?? params.pageSize,
    totalCount: data?.totalCount ?? data?.total ?? (data?.items?.length ?? 0),
    totalPages: data?.totalPages ?? 1
  };
}

export async function getPostDetail(slug: string): Promise<PostDetail> {
  const { data } = await api.get(`/Posts/${slug}`);
  return data;
}

export async function adminPostsList(params: {
  page: number;
  pageSize: number;
  search?: string;
}): Promise<Paginated<PostListItem>> {
  const { data } = await api.get("/Admin/AdminPosts", { params });
  return {
    items: data?.items ?? [],
    page: data?.page ?? params.page,
    pageSize: data?.pageSize ?? params.pageSize,
    totalCount: data?.totalCount ?? 0,
    totalPages: data?.totalPages ?? 1
  };
}

export async function adminPostCreate(payload: {
  title: string;
  slug: string;
  content: string;
  categoryId: string;
  summary?: string | null;
  isActive: boolean;
}) {
  const { data } = await api.post("/Admin/AdminPosts", payload);
  return data as { id: string };
}

export async function adminPostUpdate(
  id: string,
  payload: {
    title: string;
    slug: string;
    content: string;
    categoryId: string;
    summary?: string | null;
    isActive: boolean;
  }
) {
  const { data } = await api.put(`/Admin/AdminPosts/${id}`, payload);
  return data as { id: string };
}

export async function adminPostPublish(id: string, publish: boolean) {
  await api.post(`/Admin/AdminPosts/${id}/${publish ? "publish" : "unpublish"}`);
}

export async function adminPostDelete(id: string) {
  await api.delete(`/Admin/AdminPosts/${id}`);
}

export async function adminCategoriesList(): Promise<Category[]> {
  const { data } = await api.get("/Admin/AdminCategories");
  return data ?? [];
}

export async function adminCategoryUpsert(
  id: string | null,
  payload: {
    name: string;
    slug: string;
    description?: string | null;
    parentId?: string | null;
    displayOrder: number;
    isActive: boolean;
  }
) {
  if (id) {
    await api.put(`/Admin/AdminCategories/${id}`, payload);
  } else {
    await api.post(`/Admin/AdminCategories`, payload);
  }
}

export async function adminCategoryDelete(id: string) {
  await api.delete(`/Admin/AdminCategories/${id}`);
}

export async function adminCommentsList(): Promise<CommentRow[]> {
  const { data } = await api.get("/Admin/AdminComments");
  return data ?? [];
}

export async function adminCommentApprove(id: string) {
  await api.post(`/Admin/AdminComments/${id}/approve`);
}

export async function adminCommentDelete(id: string) {
  await api.delete(`/Admin/AdminComments/${id}`);
}

export async function adminUsersList(): Promise<UserRow[]> {
  const { data } = await api.get("/Admin/AdminUsers");
  return data ?? [];
}

export async function adminUserSetActive(id: string, active: boolean) {
  await api.post(`/Admin/AdminUsers/${id}/${active ? "activate" : "deactivate"}`);
}

export async function adminNavigationList(): Promise<NavigationItemRow[]> {
  const { data } = await api.get("/Admin/Navigation");
  return data ?? [];
}

export async function adminNavigationUpsert(
  id: string | null,
  payload: { label: string; href: string; parentId?: string | null; displayOrder: number; isActive: boolean }
) {
  if (id) {
    await api.put(`/Admin/Navigation/${id}`, payload);
  } else {
    await api.post(`/Admin/Navigation`, payload);
  }
}

export async function adminNavigationDelete(id: string) {
  await api.delete(`/Admin/Navigation/${id}`);
}

export async function adminUpload(file: File) {
  const form = new FormData();
  form.append("File", file);
  const { data } = await api.post("/Uploads", form, {
    headers: { "Content-Type": "multipart/form-data" }
  });
  return data as { url: string };
}

export async function getDashboardSnapshot() {
  const [posts, categories, users] = await Promise.all([
    adminPostsList({ page: 1, pageSize: 5 }).catch(
      () =>
        ({
          items: [],
          page: 1,
          pageSize: 5,
          totalCount: 0,
          totalPages: 1
        } satisfies Paginated<PostListItem>)
    ),
    adminCategoriesList().catch(() => [] as Category[]),
    adminUsersList().catch(() => [] as UserRow[])
  ]);

  const published = posts.items.filter((p) => p.isPublished).length;

  return {
    totalPosts: posts.totalCount ?? posts.items.length,
    publishedPosts: published,
    totalCategories: categories.length,
    totalUsers: users.length,
    latestPosts: posts.items
  };
}
