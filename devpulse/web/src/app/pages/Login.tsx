import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../lib/api";
import { useAuthStore } from "../store/auth";

export default function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const setTokens = useAuthStore((s) => s.setTokens);
  const nav = useNavigate();

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setBusy(true);
    try {
      const { data } = await api.post("/api/auth/login", { email, password });
      setTokens(data.accessToken, data.refreshToken);
      nav("/dashboard");
    } catch (err: any) {
      alert(err?.response?.data?.title ?? "Login failed");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="min-h-screen grid place-items-center">
      <form onSubmit={onSubmit} className="w-[360px] rounded-2xl border p-6 shadow-sm bg-white/5">
        <h1 className="text-center font-semibold mb-4">DevPulse Login</h1>
        <div className="space-y-3">
          <input
            className="w-full rounded-lg bg-black/10 border border-white/10 px-3 py-2"
            placeholder="Email"
            type="email"
            value={email}
            onChange={(e)=>setEmail(e.target.value)}
            required
          />
          <input
            className="w-full rounded-lg bg-black/10 border border-white/10 px-3 py-2"
            placeholder="Password"
            type="password"
            value={password}
            onChange={(e)=>setPassword(e.target.value)}
            required
          />
          <button
            type="submit"
            disabled={busy}
            className="w-full rounded-lg border border-white/10 px-3 py-2 hover:bg-white/5"
          >
            {busy ? "…" : "Log in"}
          </button>
        </div>
      </form>
    </div>
  );
}