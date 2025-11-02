import { useState } from "react";
import { Link, Outlet, useLocation } from "react-router-dom";
import { LayoutDashboard, LogIn, Menu, Sparkles } from "lucide-react";
import { motion } from "framer-motion";

import { Button } from "../components/ui/Button";
import { cn } from "../lib/utils";
import { tokenStorage } from "../lib/api";
import { useAuthStore } from "../store/auth";

const navLinks = [
  { label: "Anasayfa", to: "/" },
  { label: "Blog", to: "/blog" },
  { label: "Dersler", to: "/dersler" },
  { label: "Galeri", to: "/galeri" },
  { label: "İletişim", to: "/iletisim" }
];

export function PublicLayout() {
  const { pathname } = useLocation();
  const user = useAuthStore((state) => state.user);
  const [mobileOpen, setMobileOpen] = useState(false);
  const isLoggedIn = !!user || !!tokenStorage.get();

  return (
    <div className="min-h-screen bg-background text-foreground">
      <header className="sticky top-0 z-40 border-b border-border/60 bg-gradient-to-r from-background/95 via-background/80 to-background/95 shadow-[0_12px_30px_rgba(9,12,20,0.45)] backdrop-blur">
        <div className="container flex h-20 items-center gap-6">
          <Link to="/" className="flex items-center gap-2 text-lg font-extrabold tracking-[0.35em] text-primary">
            OBEYSOFT
          </Link>
          <div className="hidden items-center gap-2 text-xs text-muted-foreground md:flex">
            <span className="inline-flex items-center gap-2 rounded-full border border-primary/30 bg-primary/10 px-3 py-1 font-semibold uppercase tracking-[0.3em] text-primary">
              <Sparkles size={14} /> Kurumsal Yazılım Studio
            </span>
          </div>
          <nav className="relative ml-auto hidden items-center gap-1 rounded-full border border-border/70 bg-secondary/30 px-1 py-1 backdrop-blur md:flex">
            {navLinks.map((item) => {
              const active =
                pathname === item.to || (item.to !== "/" && pathname.startsWith(item.to));
              return (
                <Link
                  key={item.to}
                  to={item.to}
                  className={cn(
                    "group relative inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-medium text-muted-foreground transition",
                    active && "bg-primary/15 text-foreground shadow-[inset_0_0_0_1px_rgba(59,130,246,0.35)]",
                    !active && "hover:text-foreground hover:shadow-[inset_0_0_0_1px_rgba(148,163,184,0.25)]"
                  )}
                >
                  <span>{item.label}</span>
                  <span
                    className={cn(
                      "absolute bottom-1 left-1/2 hidden h-0.5 w-1/2 -translate-x-1/2 rounded-full bg-primary/70 transition-opacity",
                      active && "block"
                    )}
                  />
                </Link>
              );
            })}
          </nav>
          <div className="flex items-center gap-2">
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setMobileOpen((prev) => !prev)}
              aria-label="Menüyü aç"
            >
              <Menu size={18} />
            </Button>
            {isLoggedIn ? (
              <Button size="sm" asChild>
                <Link to="/admin/dashboard">
                  <LayoutDashboard size={16} className="mr-2" /> Panel
                </Link>
              </Button>
            ) : (
              <Button size="sm" variant="ghost" asChild>
                <Link to="/login">
                  <LogIn size={16} className="mr-2" /> Admin
                </Link>
              </Button>
            )}
          </div>
        </div>
        {mobileOpen && (
          <div className="md:hidden border-t border-border/60 bg-background/95">
            <div className="container flex flex-col py-3">
              {navLinks.map((item) => (
                <Link
                  key={item.to}
                  to={item.to}
                  onClick={() => setMobileOpen(false)}
                  className="rounded-full px-3 py-2 text-sm text-muted-foreground transition hover:bg-secondary/30 hover:text-foreground"
                >
                  {item.label}
                </Link>
              ))}
              <div className="mt-2 flex gap-2">
                {isLoggedIn ? (
                  <Button className="flex-1" asChild>
                    <Link to="/admin/dashboard">Panel</Link>
                  </Button>
                ) : (
                  <Button className="flex-1" variant="ghost" asChild>
                    <Link to="/login">Admin</Link>
                  </Button>
                )}
              </div>
            </div>
          </div>
        )}
      </header>
      <main className="flex-1">
        <motion.div
          initial={{ opacity: 0, y: 6 }}
          animate={{ opacity: 1, y: 0 }}
          className="container py-10"
        >
          <Outlet />
        </motion.div>
      </main>
      <footer className="border-t border-border/60 bg-background/80 py-6">
        <div className="container text-center text-sm text-muted-foreground">
          © {new Date().getFullYear()} OBEYSOFT. Tüm hakları saklıdır.
        </div>
      </footer>
    </div>
  );
}


