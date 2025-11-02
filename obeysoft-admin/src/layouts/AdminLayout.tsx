import { ReactNode } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { LayoutDashboard, FileText, Folder, MessageSquare, Users, LogOut } from 'lucide-react';
import clsx from 'clsx';

export function AdminLayout({ children }: { children: ReactNode }) {
  const nav = useNavigate();
  function logout() {
    localStorage.removeItem('access_token');
    nav('/login', { replace: true });
  }

  return (
    <div className="min-h-screen grid grid-cols-12 bg-[#0b0f15] text-white">
      <aside className="hidden md:block col-span-3 lg:col-span-2 p-4 border-r border-white/10 bg-panel">
        <div className="font-bold tracking-widest text-primary mb-4">OBEYSOFT</div>
        <nav className="flex flex-col gap-1">
          <Item to="/admin/dashboard" icon={<LayoutDashboard size={16} />}>Dashboard</Item>
          <Item to="/admin/posts" icon={<FileText size={16} />}>Yazılar</Item>
          <Item to="/admin/categories" icon={<Folder size={16} />}>Kategoriler</Item>
          <Item to="/admin/comments" icon={<MessageSquare size={16} />}>Yorumlar</Item>
          <Item to="/admin/users" icon={<Users size={16} />}>Kullanıcılar</Item>
        </nav>
      </aside>
      <main className="col-span-12 md:col-span-9 lg:col-span-10">
        <header className="sticky top-0 z-10 bg-surface/60 backdrop-blur border-b border-white/10 px-4 py-3 flex items-center gap-3">
          <div className="md:hidden font-bold text-primary">OBEYSOFT</div>
          <div className="ml-auto">
            <button className="btn btn-ghost" onClick={logout}><LogOut className="mr-1" size={16} />Çıkış</button>
          </div>
        </header>
        <div className="p-4 md:p-6 max-w-6xl mx-auto">{children}</div>
      </main>
    </div>
  );
}

function Item({ to, icon, children }:{ to:string; icon:ReactNode; children:ReactNode }) {
  return (
    <NavLink
      to={to}
      className={({ isActive }) =>
        clsx('px-3 py-2 rounded flex items-center gap-2 hover:bg-white/10',
          isActive && 'bg-white/10 text-white'
        )
      }
      end
    >
      {icon}{children}
    </NavLink>
  );
}


