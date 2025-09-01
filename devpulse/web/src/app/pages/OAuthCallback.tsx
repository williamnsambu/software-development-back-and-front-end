import { useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../lib/api";
import { useAuthStore } from "../store/auth";

export default function OauthCallback() {
  const nav = useNavigate();
  const setTokens = useAuthStore((s) => s.setTokens);
  const ran = useRef(false);

  useEffect(() => {
    // Prevent double-run under React.StrictMode in dev
    if (ran.current) return;
    ran.current = true;

    (async () => {
      const qs = new URLSearchParams(window.location.search);
      const code = qs.get("code");
      const state = qs.get("state");
      const codeVerifier = sessionStorage.getItem("pkce_verifier");
      const redirectUri = `${window.location.origin}/oauth/callback`;

      if (!code || !state || !codeVerifier) {
        nav("/dashboard");
        return;
      }

      try {
        const { data } = await api.get<{ accessToken: string; refreshToken: string }>(
          "/api/oauth/github/callback",
          { params: { code, state, codeVerifier, redirectUri } }
        );

        setTokens(data.accessToken, data.refreshToken);
      } catch (e) {
        // If the same code is reused because of a double effect,
        // backend now returns 400 with a helpful detail instead of a generic 502.
        console.error("OAuth callback error:", e);
      } finally {
        sessionStorage.removeItem("pkce_verifier");
        nav("/dashboard");
      }
    })();
  }, [nav, setTokens]);

  return null;
}