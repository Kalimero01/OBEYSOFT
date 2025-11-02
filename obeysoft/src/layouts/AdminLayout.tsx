import { useMemo, useState, type ReactNode } from "react";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import {
  FileText,
  Folder,
  LayoutDashboard,
  LogOut,
  Menu,
  MessageSquare,
  PanelsTopLeft,
  UploadCloud,
  Users
} from "lucide-react";

import { Avatar, AvatarFallback } from "../components/ui/Avatar";
import { Button } from "../components/ui/Button";
import { cn } from "../lib/utils";
import { useAuthStore } from "../store/auth";

type NavItem = {
  label: string;
  to: string;
  icon: React.ReactNode;
  roles?: Array<"Admin" | "User">;
};

const NAV_ITEMS: NavItem[] = [
  { label: "Dashboard", to: "/admin/dashboard", icon: <LayoutDashboard size={18} /> },
  { label: "Yazılar", to: "/admin/posts", icon: <FileText size={18} /> },
  { label: "Kategoriler", to: "/admin/categories", icon: <Folder size={18} /> },
  { label: "Yorumlar", to: "/admin/comments", icon: <MessageSquare size={18} /> },
  { label: "Üst Menü", to: "/admin/navigation", icon: <PanelsTopLeft size={18} /> },
  { label: "Upload", to: "/admin/uploads", icon: <UploadCloud size={18} /> },
  { label: "Kullanıcılar", to: "/admin/users", icon: <Users size={18} />, roles: ["Admin"] }
];

export function AdminLayout({ children }: { children?: ReactNode }) {
  const location = useLocation();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  const availableItems = useMemo(() => {
    return NAV_ITEMS.filter((item) => {
      if (!item.roles || item.roles.length === 0) return true;
      return item.roles.includes((user?.role ?? "User") as "Admin" | "User");
    });
  }, [user?.role]);

  const pageTitle = useMemo(() => {
    const found = NAV_ITEMS.find((item) => location.pathname.startsWith(item.to));
    return found?.label ?? "Yönetim Paneli";
  }, [location.pathname]);

  const initials = (user?.displayName ?? user?.email ?? "OB").split(" ").map((part) => part[0]).join("").slice(0, 2).toUpperCase();

  const handleLogout = () => {
    logout();
    navigate("/login", { replace: true });
  };

  return (
    <div className="min-h-screen bg-background text-foreground">
      <div className="flex min-h-screen">
        <aside
          className={cn(
            "fixed inset-y-0 left-0 z-30 w-64 border-r border-border/60 bg-card/60 backdrop-blur-xl transition-transform duration-200 md:static md:translate-x-0",
            sidebarOpen ? "translate-x-0" : "-translate-x-full md:translate-x-0"
          )}
        >
          <div className="flex h-16 items-center justify-between border-b border-border/60 px-5">
            <span className="text-sm font-semibold tracking-[0.35em] text-primary">OBEYSOFT</span>
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setSidebarOpen(false)}
              aria-label="Menüyü kapat"
            >
              <Menu size={18} />
            </Button>
          </div>
          <nav className="flex flex-1 flex-col gap-1 px-3 py-5">
            {availableItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={() => setSidebarOpen(false)}
                className={({ isActive }) =>
                  cn(
                    "group flex items-center gap-3 rounded-2xl px-3 py-2 text-sm font-medium text-muted-foreground transition",
                    isActive && "bg-primary/15 text-primary"
                  )
                }
              >
                <span className="rounded-xl bg-secondary/60 p-2 text-secondary-foreground transition group-hover:bg-primary/20 group-hover:text-primary">
                  {item.icon}
                </span>
                {item.label}
              </NavLink>
            ))}
          </nav>
        </aside>

        <div className="flex flex-1 flex-col md:pl-64">
          <header className="sticky top-0 z-20 border-b border-border/60 bg-background/80 backdrop-blur">
            <div className="flex h-16 items-center gap-3 px-4">
              <Button
                variant="ghost"
                size="icon"
                className="md:hidden"
                onClick={() => setSidebarOpen((prev) => !prev)}
                aria-label="Menüyü aç"
              >
                <Menu size={18} />
              </Button>
              <div className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                {pageTitle}
              </div>
              <div className="ml-auto flex items-center gap-3">
                <div className="text-right">
                  <div className="text-sm font-semibold text-foreground">{user?.displayName ?? "Super Admin"}</div>
                  <div className="text-xs uppercase tracking-widest text-muted-foreground">{user?.role ?? "Admin"}</div>
                </div>
                <Avatar>
                  <AvatarFallback>{initials}</AvatarFallback>
                </Avatar>
                <Button variant="ghost" onClick={handleLogout}>
                  <LogOut size={16} className="mr-2" /> Çıkış
                </Button>
              </div>
            </div>
          </header>

          <main className="flex-1 bg-background/40 pb-12">
            <div className="mx-auto w-full max-w-6xl px-4 py-8">{children}</div>
          </main>
        </div>
      </div>
    </div>
  );
}


