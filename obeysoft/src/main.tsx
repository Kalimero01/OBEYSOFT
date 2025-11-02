import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import { createBrowserRouter, RouterProvider, redirect } from "react-router-dom";

import App from "./App";
import { AdminLayout } from "./layouts/AdminLayout";
import { PublicLayout } from "./layouts/PublicLayout";
import { LoginPage } from "./pages/auth/LoginPage";
import { RegisterPage } from "./pages/auth/RegisterPage";
import { CategoriesPage } from "./pages/admin/CategoriesPage";
import { CommentsPage } from "./pages/admin/CommentsPage";
import { DashboardPage } from "./pages/admin/DashboardPage";
import { NavigationPage } from "./pages/admin/NavigationPage";
import { PostsPage } from "./pages/admin/PostsPage";
import { UploadsPage } from "./pages/admin/UploadsPage";
import { UsersPage } from "./pages/admin/UsersPage";
import { BlogDetailPage } from "./pages/public/BlogDetailPage";
import { BlogListPage } from "./pages/public/BlogListPage";
import { ContactPage } from "./pages/public/ContactPage";
import { GalleryPage } from "./pages/public/GalleryPage";
import { HomePage } from "./pages/public/HomePage";
import { LessonsPage } from "./pages/public/LessonsPage";
import { PublicCategoriesPage } from "./pages/public/PublicCategoriesPage";
import { tokenStorage } from "./lib/api";

const guard = () => {
  if (!tokenStorage.get()) throw redirect("/login");
  return null;
};

const router = createBrowserRouter([
  { path: "/login", element: <LoginPage /> },
  { path: "/register", element: <RegisterPage /> },
  {
    path: "/",
    element: <PublicLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "blog", element: <BlogListPage /> },
      { path: "blog/:slug", element: <BlogDetailPage /> },
      { path: "iletisim", element: <ContactPage /> },
      { path: "kategoriler", element: <PublicCategoriesPage /> },
      { path: "dersler", element: <LessonsPage /> },
      { path: "galeri", element: <GalleryPage /> }
    ]
  },
  {
    path: "/admin",
    element: <App />,
    loader: guard,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: "dashboard", element: <DashboardPage /> },
          { path: "posts", element: <PostsPage /> },
          { path: "categories", element: <CategoriesPage /> },
          { path: "comments", element: <CommentsPage /> },
          { path: "navigation", element: <NavigationPage /> },
          { path: "uploads", element: <UploadsPage /> },
          { path: "users", element: <UsersPage /> }
        ]
      }
    ]
  }
]);

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>
);


