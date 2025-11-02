import React from 'react';
import ReactDOM from 'react-dom/client';
import { createBrowserRouter, RouterProvider, redirect } from 'react-router-dom';
import './index.css';
import App from './App';
import { LoginPage } from './pages/auth/LoginPage';
import { DashboardPage } from './pages/dashboard/DashboardPage';
import { PostsPage } from './pages/posts/PostsPage';
import { CategoriesPage } from './pages/categories/CategoriesPage';
import { CommentsPage } from './pages/comments/CommentsPage';
import { UsersPage } from './pages/users/UsersPage';

const guard = () => {
  const token = localStorage.getItem('access_token');
  if (!token) throw redirect('/login');
  return null;
};

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    path: '/admin',
    element: <App />,
    loader: guard,
    children: [
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'posts', element: <PostsPage /> },
      { path: 'categories', element: <CategoriesPage /> },
      { path: 'comments', element: <CommentsPage /> },
      { path: 'users', element: <UsersPage /> }
    ]
  },
  { path: '*', loader: () => redirect('/admin/dashboard') }
]);

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>
);


