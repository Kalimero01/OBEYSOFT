import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useEffect } from 'react';
import { AdminLayout } from './layouts/AdminLayout';
import { meRequest } from './lib/api';

export default function App() {
  const nav = useNavigate();
  const loc = useLocation();

  useEffect(() => {
    (async () => {
      try {
        await meRequest();
      } catch {
        localStorage.removeItem('access_token');
        nav('/login', { replace: true });
      }
    })();
  }, [loc.pathname, nav]);

  return <AdminLayout><Outlet /></AdminLayout>;
}


