// src/app/pages/OAuthSuccess.tsx
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useAuthStore } from "../store/auth";

export default function OAuthSuccess() {
  const setTokens = useAuthStore((s) => s.setTokens);
  const nav = useNavigate();

  useEffect(() => {
    const hash = new URLSearchParams(window.location.hash.replace(/^#/, ""));
    const access = hash.get("access");
    const refresh = hash.get("refresh");

    if (access && refresh) {
      setTokens(access, refresh);
      sessionStorage.removeItem("pkce_verifier");
      nav("/dashboard", { replace: true });
    } else {
      nav("/login", { replace: true });
    }
  }, [nav, setTokens]);

  return (
    <div className="min-h-screen grid place-items-center text-white/80">
      <div className="rounded-xl border border-white/10 p-6 bg-white/5">
        Finalizing session…
      </div>
    </div>
  );
}