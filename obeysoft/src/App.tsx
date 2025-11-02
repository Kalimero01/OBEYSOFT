import { useEffect, useRef } from "react";
import { Outlet, useNavigate } from "react-router-dom";

import { tokenStorage } from "./lib/api";
import { useAuthStore } from "./store/auth";

export default function App() {
  const navigate = useNavigate();
  const check = useAuthStore((state) => state.check);
  const hasChecked = useRef(false);

  useEffect(() => {
    if (hasChecked.current) return;
    hasChecked.current = true;

    if (!tokenStorage.get()) {
      navigate("/login", { replace: true });
      return;
    }

    check().catch(() => {
      navigate("/login", { replace: true });
    });
  }, [check, navigate]);

  return <Outlet />;
}


